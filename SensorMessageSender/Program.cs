using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using System.Configuration;
using SensorMessageSender.Models;
using SensorMessageSender.Models.Sensores;

namespace SensorMessageSender
{
    class Program
    {
        // Tempo de delay entre os envios de mensagens
        private const int intervalInMilliseconds = 1000;

        // IoT Hub Device Client
        private static DeviceClient deviceClient;


        // Device connection string para autenticação com o IoT hub, configurar valor no App.config
        private readonly static string deviceConnectionString = ConfigurationManager.ConnectionStrings["deviceConnectionString"].ConnectionString;

        private static void Main(string[] args)
        {
            ConsoleHelper.WriteColorMessage("Sensor device app.\n", ConsoleColor.Yellow);

            // Conexão com o IoT hub usando o protocolo MQTT
            deviceClient = DeviceClient.CreateFromConnectionString(
                deviceConnectionString,
                TransportType.Mqtt);

            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {

            // Bateria do dispositivo
            int energy = 100;
            int energyLess = 2;

            Dht11 objGeracao = new Dht11();

            while (energy > 0)
            {
                double humidity = objGeracao.getHumidity();
                double temperature = objGeracao.getTemperature();

                await CreateTelemetryMessage(temperature, humidity);
                await CreateLoggingMessage(temperature, humidity);
                await Task.Delay(intervalInMilliseconds);

                energy -= energyLess;
            }

            System.Environment.Exit(0);
        }

        private static async Task CreateTelemetryMessage(
            double temperature, double humidity)
        {
            var telemetryDataPoint = new
            {
                temperature = Math.Round(temperature, 2),
                humidity = humidity,
            };

            var telemetryMessageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var telemetryMessage = new Message(Encoding.ASCII.GetBytes(telemetryMessageString));

            // Propriedade que indica que essa mensagem cumprirá a consulta de roteamento e ativar o trigger do log - false
            telemetryMessage.Properties.Add("sensorID", "VSTel");

            Console.WriteLine($"Telemetry data: {telemetryMessageString}");

            // Envio da mensagem
            await deviceClient.SendEventAsync(telemetryMessage);
            ConsoleHelper.WriteGreenMessage($"Telemetry sent {DateTime.Now.ToShortTimeString()}");
        }

        private static async Task CreateLoggingMessage(
            double temperature, double humidity)
        {
            var loggingDataPoint = new
            {
                temperature = Math.Round(temperature, 2),
                humidity = humidity,
            };

            var loggingMessageString = JsonConvert.SerializeObject(loggingDataPoint);
            var loggingMessage = new Message(Encoding.ASCII.GetBytes(loggingMessageString));

            // Propriedade que indica que essa mensagem cumprirá a consulta de roteamento e ativar o trigger do log - true
            loggingMessage.Properties.Add("sensorID", "VSLog");

            Console.WriteLine($"Log data: {loggingMessageString}");

            // Envio da mensagem
            await deviceClient.SendEventAsync(loggingMessage);
            ConsoleHelper.WriteGreenMessage("Log data sent\n");
        }
    }

}
