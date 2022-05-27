using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Distance {
    class Program {
        static void Main(string[] args) {
            GpioController controller = new GpioController();

            var sensor = new UltrasonicDistanceSensor(23, 24, controller);
            var indicator = new Indicator(17, controller);

            indicator.On();
            Thread.Sleep(1000);
            indicator.Off();

            var data = sensor.Measure();
        }
    }
}
