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

            while(true) 
            {
                var distanceInCantimeters = sensor.Measure() / 10;
                Console.WriteLine($"{(distanceInCantimeters):0.#} cm.");
                
                if(distanceInCantimeters < 20)
                    indicator.On();
                else
                    indicator.Off();
            }
        }
    }
}
