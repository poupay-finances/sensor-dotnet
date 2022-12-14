namespace SensorMessageSender.Models.Sensores
{
    internal class Dht11
    {
        public Dht11()
        {
            baseHumidity = randomNumber(minHumidity, maxHumidity);
            baseTemperature = randomNumber(minTemperature, maxTemperature);
        }

        private Random random = new Random();

        private static int minHumidity = 5;
        private static int maxHumidity = 98;
        private double baseHumidity = 0;

        private static int minTemperature = 10;
        private static int maxTemperature = 50;
        private double baseTemperature = 0;

        private int errorChance = 3;

        internal Boolean dataError()
        {
            return random.Next(100) < errorChance;
        }

        internal double randomNumber(int min, int max)
        {
            return random.Next(min, max);
        }

        internal double UniformRandomNumber(int baseNumber, int range)
        {
            if (dataError())
            {
                return 0;
            }

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
