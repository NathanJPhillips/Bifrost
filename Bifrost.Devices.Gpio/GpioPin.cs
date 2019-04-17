using Bifrost.Devices.Gpio.Core;
using Bifrost.Devices.Gpio.Abstractions;
using System;
using System.IO;

namespace Bifrost.Devices.Gpio
{
    public class GpioPin : IGpioPin, IDisposable
    {
        private bool IsClosed = false;

        public int PinNumber { get; private set; }

        public string GpioPath { get; private set; }
        public string DevicePath { get; private set; }

        public GpioPin(int pinNumber, string gpioPath, string devicePath)
        {
            this.PinNumber = pinNumber;
            this.GpioPath = gpioPath;
        }

        public void Dispose()
        {
            var controller = GpioController.Instance;
            if (!IsClosed)
            {
                this.Close(PinNumber);
            }
            Dispose();
        }

        private void Close(int pinNumber)
        {
            if (!IsClosed)
            {
                throw new Exception("Pin is already closed");
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

        public void SetDriveMode(GpioPinDriveMode driveMode)
        {
            //var path = GpioController.DevicePath;
            // write in or out to the "direction" file for this pin.
            if (driveMode == GpioPinDriveMode.Output)
            {
                File.WriteAllText(Path.Combine(this.GpioPath, "direction"), "out");
                Directory.SetLastWriteTime(Path.Combine(this.GpioPath), DateTime.UtcNow);
            }
            else
            {
                File.WriteAllText(Path.Combine(this.GpioPath, "direction"), "in");
                Directory.SetLastWriteTime(Path.Combine(this.GpioPath), DateTime.UtcNow);
            }
        }

        public void Write(GpioPinValue pinValue)
        {
            //var path = GpioController.DevicePath;
            // write a 1 or a 0 to the "value" file for this pin
            File.WriteAllText(Path.Combine(this.GpioPath, "value"), ((int)pinValue).ToString());
            Directory.SetLastWriteTime(Path.Combine(this.GpioPath), DateTime.UtcNow);
        }

        public GpioPinValue Read()
        {
            if (File.Exists(Path.Combine(this.GpioPath, "value")))
            {
                var pinValue = File.ReadAllText(Path.Combine(this.GpioPath, "value"));

                return (GpioPinValue)Enum.Parse(typeof(GpioPinValue), pinValue);
            }
            else
            {
                return GpioPinValue.Low;
            }
        }
    }
}
