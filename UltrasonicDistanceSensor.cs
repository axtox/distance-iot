using System.Device.Gpio;
using Iot.Device.Hcsr04;
using UnitsNet;

namespace Distance {
    internal enum MeasureUnit { mm = 1, cm = 10, m = 100 }

    internal class UltrasonicDistanceSensor : Device
    {
        private readonly int _triggerPin;
        private readonly int _echoPin;
        private Hcsr04 _sensor;

        private const int AccuracyErrorInMillimeters = 3000;
        public MeasureUnit DefaultMeasureUnit = MeasureUnit.mm;

        public UltrasonicDistanceSensor(int triggerPin, int echoPin, GpioController controller) : base(controller)
        {
            _triggerPin = triggerPin;
            _echoPin = echoPin;
            _sensor = new Hcsr04(controller, triggerPin, echoPin, false);
        }

        /// <summary>
        /// Measures the distance in <see cref="DefaultMeasureUnit"/> using ultrasonic speed
        /// </summary>
        /// <returns>Returns distance in <see cref="DefaultMeasureUnit"/></returns>
        public double Measure() 
        {
            return Measure(1);
        } 

        /// <summary>
        /// Measures the distance in <see cref="DefaultMeasureUnit"/> using ultrasonic speed. 
        /// Accuracy achieved by repeated measurements and sensor error removal
        /// </summary>
        /// <param name="precisionRepeats">Times to repeat mesurments to calculate average between them</param>
        /// <returns>Returns distance in <see cref="DefaultMeasureUnit"/></returns>
        public double Measure(int precisionRepeats) 
        {
            return CalculateAverageDistanceExcludingError(precisionRepeats) / (double)DefaultMeasureUnit;
        }
        
        private double CalculateAverageDistanceExcludingError(int precisionRepeats) 
        {
            var measurements = CollectMeasurments(precisionRepeats);

            Array.Sort(measurements);

            var ethalonMeasurment = measurements[(int)Math.Floor(measurements.Length / 2d)];

            var accurateMeasurements = new List<Length>(measurements.Length);
            foreach(var measurement in measurements) 
            {
                var differenceWithEthalon = measurement - ethalonMeasurment;

                if(Math.Abs(differenceWithEthalon.Millimeters) <= AccuracyErrorInMillimeters)
                    accurateMeasurements.Add(measurement);
            }

            var averageResult = accurateMeasurements.Average(measurement => measurement.Millimeters);

            return averageResult == 0 ? double.PositiveInfinity : averageResult;
        }

        private Length[] CollectMeasurments(int precisionRepeats) {
            var allMeasurments = new Length[precisionRepeats];

            for(var measurment = 0; measurment < precisionRepeats; measurment++) 
            {
                allMeasurments[measurment] = ReadDistanceFromSensor();
            }

            return allMeasurments;
        }

        private Length ReadDistanceFromSensor() {
            Thread.Sleep(60);
            return _sensor.TryGetDistance(out Length result) 
                    ? result 
                    : Length.FromMillimeters(0);
        }
    }
}