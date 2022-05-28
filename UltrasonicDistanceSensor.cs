using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;

namespace Distance {
    internal enum MeasureUnit { mm = 1, cm = 10, m = 100 }

    internal class UltrasonicDistanceSensor : Device
    {
        private readonly int _triggerPin;
        private readonly int _echoPin;
        private const double SoundWaveSpeedKilometerPerMillisecond = 0.034;
        public MeasureUnit DefaultMesureUnit = MeasureUnit.mm;


        public UltrasonicDistanceSensor(int triggerPin, int echoPin, GpioController controller) : base(controller)
        {
            _triggerPin = triggerPin;
            _echoPin = echoPin;
            
            controller.OpenPin(triggerPin, PinMode.Output);
            controller.OpenPin(echoPin, PinMode.Input);
        }

        /// <summary>
        /// Measures the distance in millimeters using ultrasonic waves
        /// </summary>
        public double Measure() 
        {
            //clear previous state of the device
            _controller.Write(_triggerPin, PinValue.Low);
            Thread.Sleep(2);

            //send microwave for measuring
            _controller.Write(_triggerPin, PinValue.High);
            Thread.Sleep(10);
            _controller.Write(_triggerPin, PinValue.Low);

            //get millisecond wave time travel (back and forth)
            var waveTravelTime = GetMillisecondTravelTime();

            //calculate distance in kilometers and convert to millimeters
            return waveTravelTime * SoundWaveSpeedKilometerPerMillisecond / 2 * 10000;
        }

        private long GetMillisecondTravelTime() {
            WaitForSignal();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            StartReadingSignal();

            stopWatch.Stop();

            return stopWatch.ElapsedMilliseconds;
        }

        private void WaitForSignal() 
        {
            while(_controller.Read(_echoPin) == PinValue.Low);
        }

        private void StartReadingSignal() 
        {
            while(_controller.Read(_echoPin) == PinValue.High);
        }
    }
}