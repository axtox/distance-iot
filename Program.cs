using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Distance {
    class Program {
        static void Main(string[] args) {
            GpioController controller = new GpioController();

            var sensor = new UltrasonicDistanceSensor(4, 17, controller);
            sensor.DefaultMeasureUnit = MeasureUnit.cm;

            var indicator = new Indicator(23, controller);

            var display = new OledDisplay(controller);
            while(true) 
            {
                var distanceInCantimeters = sensor.Measure();
                Console.WriteLine($"{(distanceInCantimeters):0.#} cm.");
                display.Message($"{(distanceInCantimeters):0.#} cm.");
                
                if(distanceInCantimeters < 20)
                    indicator.On();
                else
                    indicator.Off();

                Thread.Sleep(500);
            }
        }
    }
}
