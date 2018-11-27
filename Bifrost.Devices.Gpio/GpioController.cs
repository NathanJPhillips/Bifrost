﻿using System;
using Bifrost.Devices.Gpio.Abstractions;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Bifrost.Devices.Gpio
{
    public class GpioController : IGpioController
    {
        private static GpioController instance = new GpioController();

        public static string DevicePath { get; private set; }

        public static IGpioController Instance
        {
            get
            {
                bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                if (isWindows) 
                {
                    try
                    {
                        var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");

                        localAppData = localAppData.Replace("Administrator", "DefaultAccount");

                        localAppData = Path.Combine(localAppData, "Packages");

                        var piFolders = new DirectoryInfo(localAppData);

                        var biFrostFolder = piFolders.GetDirectories().Single(m => m.Name.StartsWith("Bifrost"));

                        var localStateDirectory = biFrostFolder.GetDirectories().Single(m => m.Name.StartsWith("LocalState"));

                        DevicePath = localStateDirectory.FullName;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Could not find device path for Windows application", ex);
                    }
                }
                else 
                {
                    DevicePath = @"/sys/class/gpio";
                }

                // need to check that the device path exists
                if (string.IsNullOrEmpty(DevicePath))
                {
                    throw new NullReferenceException("There is no value for the GPIO Directory.");
                }

                return instance;
            }
        }
        
        public IDictionary<int, IGpioPin> Pins
        {
            get
            {
                return new DirectoryInfo(DevicePath).GetDirectories()
                    .Where(di => di.Name.StartsWith("gpio"))
                    .Select(di => Tuple.Create(di, int.TryParse(di.Name.Substring("gpio".Length), out int value) ? value : (int?)null))
                    .Where(diAndPinNo => diAndPinNo.Item2.HasValue)
                    .ToDictionary(diAndPinNo => diAndPinNo.Item2.Value, diAndPinNo => (IGpioPin)new GpioPin(diAndPinNo.Item2.Value, diAndPinNo.Item1.FullName));
            }
        }

        public IGpioPin OpenPin(int pinNumber)
        {
            if (pinNumber < 1 || pinNumber > 26)
            {
                throw new ArgumentOutOfRangeException("Valid pins are between 1 and 26.");
            }
            // add a file to the export directory with the name <<pin number>>
            // add folder under device path for "gpio<<pinNumber>>"
            var gpioDirectoryPath = Path.Combine(DevicePath, string.Concat("gpio", pinNumber.ToString()));

            var gpioExportPath = Path.Combine(DevicePath, "export");
            
            if (!Directory.Exists(gpioDirectoryPath))
            {
                File.WriteAllText(gpioExportPath, pinNumber.ToString());
                Directory.CreateDirectory(gpioDirectoryPath);
            }

            // instantiate the gpiopin object to return with the pin number.
            return new GpioPin(pinNumber, gpioDirectoryPath);
        }

        public void ClosePin(int pinNumber)
        {
            if (pinNumber < 1 || pinNumber > 26)
            {
                throw new ArgumentOutOfRangeException("Valid pins are between 1 and 26.");
            }
            // add a file to the export directory with the name <<pin number>>
            // add folder under device path for "gpio<<pinNumber>>"
            var gpioDirectoryPath = Path.Combine(DevicePath, string.Concat("gpio", pinNumber.ToString()));

            var gpioExportPath = Path.Combine(DevicePath, "unexport");

            if (Directory.Exists(gpioDirectoryPath))
            {
                File.WriteAllText(gpioExportPath, pinNumber.ToString());
            }
        }
    }
}
