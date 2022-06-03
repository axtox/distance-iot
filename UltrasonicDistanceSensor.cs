using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using System.Linq;

namespace Distance {
    internal enum MeasureUnit { mm = 1, cm = 10, m = 100 }

    internal class UltrasonicDistanceSensor : Device
    {
        private readonly int _triggerPin;
        private readonly int _echoPin;

        private readonly Stopwatch stopWatch = new Stopwatch();

        // based on the statement that the speed of sound is 343 m/s
        private const decimal SoundWaveSpeedMillimeterPerMillisecond = 343.3M;
        private const int RepeatMeasurmentTimes = 5;
        private const int AccuracyErrorInMillimeters = 50;
        public MeasureUnit DefaultMeasureUnit = MeasureUnit.mm;

        public UltrasonicDistanceSensor(int triggerPin, int echoPin, GpioController controller) : base(controller)
        {
            _triggerPin = triggerPin;
            _echoPin = echoPin;
            
            controller.OpenPin(triggerPin, PinMode.Output);
            controller.OpenPin(echoPin, PinMode.Input);
        }

        /// <summary>
        /// Measures the distance in <see cref="DefaultMeasureUnit"/> using ultrasonic speed
        /// </summary>
        public decimal Measure() 
        {
            return MeasureSignalTravelTime() * SoundWaveSpeedMillimeterPerMillisecond / 2 / (decimal)DefaultMeasureUnit;
        }

        private long MeasureSignalTravelTime() {
            SendSignal();

            stopWatch.Restart();

            WaitForSignal();
            ReadSignal();

            stopWatch.Stop();

            return stopWatch.ElapsedMilliseconds;
        }

        private void SendSignal() 
        {
            //clear previous state of the device
            _controller.Write(_triggerPin, PinValue.Low);
            Thread.Sleep(2);

            //send microwave for measuring
            _controller.Write(_triggerPin, PinValue.High);
            Thread.Sleep(2);
            _controller.Write(_triggerPin, PinValue.Low);
        }

        private void WaitForSignal() 
        {
            while(_controller.Read(_echoPin) == PinValue.Low);
        }

        private void ReadSignal() 
        {
            while(_controller.Read(_echoPin) == PinValue.High);
        }

        /// <summary>
        /// Measures the distance in <see cref="DefaultMeasureUnit"/> using ultrasonic speed. 
        /// Accuracy achieved by repeated measurements and sensor error removal
        /// </summary>
        public decimal MeasureWithPrecision() 
        {
            return CollectAccurateTravelTime() * SoundWaveSpeedMillimeterPerMillisecond / 2 / (decimal)DefaultMeasureUnit;
        }

        private decimal CollectAccurateTravelTime() {
            var allMeasurments = new decimal[RepeatMeasurmentTimes];

            for(var measurment = 0; measurment < RepeatMeasurmentTimes; measurment++) 
            {
                allMeasurments[measurment] = MeasureSignalTravelTime();
            }

            return CalculateAverageExcludingError(allMeasurments);
        }

        private decimal CalculateAverageExcludingError(decimal[] measurments) 
        {
            Array.Sort(measurments);

            var ethalonMeasurment = measurments[(int)Math.Round(measurments.Length / 2d)];

            var accurateMeasurements = new List<decimal>(measurments.Length);
            foreach(var measurement in measurments) 
            {
                var differenceWithEthalon = measurement - ethalonMeasurment;

                if(Math.Abs(differenceWithEthalon) <= AccuracyErrorInMillimeters)
                    accurateMeasurements.Add(measurement);
            }

            return accurateMeasurements.Average();
        }
    }
}