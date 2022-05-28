using System.Device.Gpio;

namespace Distance {
    internal class Indicator : Device {
        private readonly int _gpioPin;

        public Indicator(int gpioPin, GpioController controller) : base(controller)
        {
            _gpioPin = gpioPin;
            controller.OpenPin(_gpioPin, PinMode.Output);
        }

        public void On() 
        {            
            _controller.Write(_gpioPin, PinValue.Low);
        }      
        
        public void Off() 
        {
            _controller.Write(_gpioPin, PinValue.High);
        }
    }
}