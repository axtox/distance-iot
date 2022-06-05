using System.Device.Gpio;

namespace Distance {
    internal abstract class Device : IDisposable {
        private bool _disposedValue;

        protected GpioController _controller;

        public Device(GpioController controller)
        {
            _controller = controller;
        }

        ~Device() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _controller.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}