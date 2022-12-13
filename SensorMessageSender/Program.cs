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
        private const int energyLess = 2;

        // IoT Hub Device Client
        private static DeviceClient deviceClient;

        // Device connection string para autenticação com o IoT hub, configurar valor no App.config
        private readonly static string deviceConnectionString = ConfigurationManager.ConnectionStrings["deviceConnectionString"].ConnectionString;

        // Device ID, configurar valor no App.config
        private readonly static string deviceId = ConfigurationManager.AppSettings["deviceId"];

        private static void Main(string[] args)
        {
            ValidarVariaveis();

            ConsoleHelper.WriteGreenMessage("sensor device app running.\n");

            // Conexão com o IoT hub usando o protocolo MQTT
            deviceClient = DeviceClient.CreateFromConnectionString(
                deviceConnectionString,
                TransportType.Mqtt);

            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        private static void ValidarVariaveis()
        {
            if (String.IsNullOrEmpty(deviceConnectionString))
            {
                ConsoleHelper.WriteRedMessage("Por favor defina a connection string do dispositivo nas configurações da aplicação");
                System.Environment.Exit(1);
            }

            if (String.IsNullOrEmpty(deviceId))
            {
                ConsoleHelper.WriteRedMessage("Por favor defina o ID do dispositivo nas configurações da aplicação");
                System.Environment.Exit(1);
            }
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {

            // Bateria do dispositivo
            int energy = 100;

            Dht11 objGeracao = new Dht11();

            while (energy > 0)
            {
                double humidity = objGeracao.getHumidity();
                double temperature = objGeracao.getTemperature();

                await CreateTelemetryMessage(temperature, humidity, energy);
                await Task.Delay(intervalInMilliseconds);

                energy -= energyLess;
            }

            ConsoleHelper.WriteRedMessage($"Device {deviceId}: Dead battery");
            System.Environment.Exit(0);
        }

        private static async Task CreateTelemetryMessage(
            double temperature, double humidity, int energy)
        {
            var telemetryDataPoint = new
            {
                deviceId = deviceId,
                temperature = Math.Round(temperature, 2),
                humidity = humidity,
            };

            var telemetryMessageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var telemetryMessage = new Message(Encoding.ASCII.GetBytes(telemetryMessageString));

            // Última mensagem antes do dispositivo descarregar
            bool lastMessage = energy - energyLess <= 0;

            // Propriedade que indica que essa mensagem cumprirá a consulta de roteamento e ativar o trigger do log - false
            telemetryMessage.Properties.Add("sensorID", deviceId);
            telemetryMessage.Properties.Add("energy", energy.ToString());
            telemetryMessage.Properties.Add("lastWillMessage", lastMessage.ToString());

            Console.WriteLine($"Telemetry data: {telemetryMessageString}");

            // Envio da mensagem
            await deviceClient.SendEventAsync(telemetryMessage);
            ConsoleHelper.WriteColorMessage($"Telemetry sent {DateTime.Now.ToShortTimeString()}\n", ConsoleColor.Blue);
        }
    }

}
