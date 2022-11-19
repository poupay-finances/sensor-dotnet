namespace SensorMessageSender.Models.Sensores
{
    internal class Dht11
    {
        public Dht11()
        {
            baseHumidity = randomNumber(minHumidity, maxHumidity);
            baseTemperature = randomNumber(minTemperature, maxTemperature);
        }

        private static int minHumidity = 30;
        private static int maxHumidity = 98;
        private double baseHumidity = 0;

        private static int minTemperature = 20;
        private static int maxTemperature = 38;
        private double baseTemperature = 0;

        internal double randomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        internal double UniformRandomNumber(int baseNumber, int range)
        {
            Random random = new Random();
            return random.Next(baseNumber, baseNumber + range);
        }

        internal double getHumidity()
        {
            return UniformRandomNumber(((int)baseHumidity), 2);
        }

        internal double getTemperature()
        {
            return UniformRandomNumber(((int)baseTemperature), 2);
        }

    }
}
