// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.

// New features:
// * Similar structure to previous labs
// * Introduces code to simulate a conveyor belt temp and vibration

using System;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace Dht11Device
{
    class Program
    {
        // Telemetry globals.
        private const int intervalInMilliseconds = 2000; // Time interval required by wait function.

        // IoT Hub global variables.
        private static DeviceClient deviceClient;

        // The device connection string to authenticate the device with your IoT hub.
        private readonly static string deviceConnectionString = "$CONNECTION_STRING";

        private static void Main(string[] args)
        {
            ConsoleHelper.WriteColorMessage("DHT11 sensor device app.\n", ConsoleColor.Yellow);

            // Connect to the IoT hub using the MQTT protocol.
            deviceClient = DeviceClient.CreateFromConnectionString(
                deviceConnectionString,
                TransportType.Mqtt);

            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        // Async method to send simulated telemetry.
        private static async void SendDeviceToCloudMessagesAsync()
        {
            // The ConveyorBeltSimulator class is used to create a
            // ConveyorBeltSimulator instance named `conveyor`. The `conveyor`
            // object is first used to capture a vibration reading which is
            // placed into a local `vibration` variable, and is then passed to
            // the two create message methods along with the `vibration` value
            // that was captured at the start of the interval.
            var conveyor = new ConveyorBeltSimulator(intervalInMilliseconds);

            Geracao objGeracao = new Geracao();
            
            // Simulate the vibration telemetry of a conveyor belt.
            while (true)
            {
                double humidity = objGeracao.getHumidity();
                double temperature = objGeracao.getTemperature();

                await CreateTelemetryMessage(conveyor, temperature, humidity);

                await CreateLoggingMessage(conveyor, temperature, humidity);

                await Task.Delay(intervalInMilliseconds);
            }
        }

        // This method creates a JSON message string and uses the Message
        // class to send the message, along with additional properties. Notice
        // the sensorID property - this will be used to route the VSTel values
        // appropriately at the IoT Hub. Also notice the beltAlert property -
        // this is set to true if the conveyor belt haas stopped for more than 5
        // seconds.
        private static async Task CreateTelemetryMessage(
            ConveyorBeltSimulator conveyor,
            double temperature, double humidity)
        {

            var telemetryDataPoint = new
            {
                packages = conveyor.PackageCount,
                temperature = Math.Round(temperature, 2),
                humidity = humidity,
            };
            var telemetryMessageString =
                JsonConvert.SerializeObject(telemetryDataPoint);
            var telemetryMessage =
                new Message(Encoding.ASCII.GetBytes(telemetryMessageString));

            // Add a custom application property to the message. This is used to route the message.
            telemetryMessage.Properties.Add("sensorID", "VSTel");

            // Send an alert if the belt has been stopped for more than five seconds.
            telemetryMessage.Properties
                .Add("beltAlert", (conveyor.BeltStoppedSeconds > 5) ? "true" : "false");

            Console.WriteLine($"Telemetry data: {telemetryMessageString}");

            // Send the telemetry message.
            await deviceClient.SendEventAsync(telemetryMessage);
            ConsoleHelper.WriteGreenMessage($"Telemetry sent {DateTime.Now.ToShortTimeString()}");
        }

        // This method is very similar to the CreateTelemetryMessage method.
        // Here are the key items to note:
        // * The loggingDataPoint contains more information than the telemetry
        //   object. It is common to include as much information as possible for
        //   logging purposes to assist in any fault diagnosis activities or
        //   more detailed analytics in the future.
        // * The logging message includes the sensorID property, this time set
        //   to VSLog. Again, as noted above, his will be used to route the
        //   VSLog values appropriately at the IoT Hub.
        private static async Task CreateLoggingMessage(
            ConveyorBeltSimulator conveyor,
            double temperature, double humidity)
        {
            // Create the logging JSON message.
            var loggingDataPoint = new
            {
                packages = conveyor.PackageCount,
                temperature = Math.Round(temperature, 2),
                humidity = humidity,
            };
            var loggingMessageString = JsonConvert.SerializeObject(loggingDataPoint);
            var loggingMessage = new Message(Encoding.ASCII.GetBytes(loggingMessageString));

            // Add a custom application property to the message. This is used to route the message.
            loggingMessage.Properties.Add("sensorID", "VSLog");

            // Send an alert if the belt has been stopped for more than five seconds.
            loggingMessage.Properties.Add("beltAlert", (conveyor.BeltStoppedSeconds > 5) ? "true" : "false");

            Console.WriteLine($"Log data: {loggingMessageString}");

            // Send the logging message.
            await deviceClient.SendEventAsync(loggingMessage);
            ConsoleHelper.WriteGreenMessage("Log data sent\n");
        }
    }

    // The ConveyorBeltSimulator class simulates the operation of a conveyor
    // belt, modeling a number of speeds and related states to generate
    // vibration data. The ConsoleHelper class is used to write different
    // colored text to the console to highlight different data and values.
    internal class ConveyorBeltSimulator
    {
        Random rand = new Random();

        private readonly int intervalInSeconds;

        // Conveyor belt globals.
        public enum SpeedEnum
        {
            stopped,
            slow,
            fast
        }
        // Count of packages leaving the conveyor belt.
        private int packageCount = 0;
        // Initial state of the conveyor belt.
        private SpeedEnum beltSpeed = SpeedEnum.stopped;
        // Time the belt has been stopped.
        private double beltStoppedSeconds = 0;
        // Ambient temperature of the facility.
        private double temperature = 60;
        // Constant identifying the severity of natural vibration.
        private double naturalConstant;

        public double BeltStoppedSeconds { get => beltStoppedSeconds; }
        public int PackageCount { get => packageCount; }
        public double Temperature { get => temperature; }
        public SpeedEnum BeltSpeed { get => beltSpeed; }

        internal ConveyorBeltSimulator(int intervalInMilliseconds)
        {

            // Create a number between 2 and 4, as a constant for normal vibration levels.
            naturalConstant = 2 + 2 * rand.NextDouble();
            // Time interval in seconds.
            intervalInSeconds = intervalInMilliseconds / 1000;
        }

    }

    internal static class ConsoleHelper
    {
        internal static void WriteColorMessage(string text, ConsoleColor clr)
        {
            Console.ForegroundColor = clr;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        internal static void WriteGreenMessage(string text)
        {
            WriteColorMessage(text, ConsoleColor.Green);
        }

        internal static void WriteRedMessage(string text)
        {
            WriteColorMessage(text, ConsoleColor.Red);
        }
    }

    internal class Geracao
    {
        public Geracao()
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
