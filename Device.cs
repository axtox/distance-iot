using System.Device.Gpio;

namespace Distance {
    internal abstract class Device {
        protected GpioController _controller;

        public Device(GpioController controller)
        {
            _controller = controller;
        }
    }
}