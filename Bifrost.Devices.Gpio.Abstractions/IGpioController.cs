using System.Collections.Generic;

namespace Bifrost.Devices.Gpio.Abstractions
{
    public interface IGpioController
    {
        IDictionary<int, GetGpioPinDelegate> Pins { get; }
    }

    public delegate IGpioPin GetGpioPinDelegate();
}
