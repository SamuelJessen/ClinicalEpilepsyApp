using System.Text.Json;
using ClinicalEpilepsyApp.Domain.Models;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace EpilepsyMQTTObserver;

public class FakeMAUIPython
{
    public void RunFakeMQTTClients()
    {
        string brokerAddress = "test.mosquitto.org"; // Replace with your MQTT broker address
        string raw_clientId = Guid.NewGuid().ToString(); // Generate a unique client ID
        string processed_clientId = Guid.NewGuid().ToString(); // Generate a unique client ID
        string raw_topic = "ecg_data_group1/measurements"; 
        string processed_topic = "ecg_data_group1/processed_measurements";

        MqttClient raw_mqttClient = new MqttClient(brokerAddress);
        MqttClient processed_mqttClient = new MqttClient(brokerAddress);

        raw_mqttClient.Connect(raw_clientId);
        Console.WriteLine("Raw Connected to MQTT broker!");
        processed_mqttClient.Connect(processed_clientId);
        Console.WriteLine("Processed Connected to MQTT broker!");

        // Serialize and publish EcgRawMeasurement every 5 seconds
        var timer1 = new System.Timers.Timer();
        timer1.Interval = 5000; // 5000 milliseconds = 5 seconds
        timer1.Elapsed += (sender, e) =>
        {
            var measurement = GenerateEcgRawMeasurement();
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize<EcgRawMeasurement>(measurement, options);
            raw_mqttClient.Publish(raw_topic, System.Text.Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        };
        timer1.Start();


        // Serialize and publish EcgProcessedMeasurement every 5 seconds
        var timer = new System.Timers.Timer();
        timer.Interval = 5000; // 5000 milliseconds = 5 seconds
        timer.Elapsed += (sender, e) =>
        {
            var proc_measurement = GenerateProcessedEcgMeasurement();
            string json = JsonSerializer.Serialize(proc_measurement);
            processed_mqttClient.Publish(processed_topic, System.Text.Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        };
        timer.Start();
    }
    public static PythonEcgProcessedMeasurement GenerateProcessedEcgMeasurement()
    {
        // Sample implementation to generate measurement data
        Random rnd = new Random();
        return new PythonEcgProcessedMeasurement
        {
            PatientId = "123456-0000",
            TimeStamp = DateTime.Now,
            ProcessedEcgChannel1 = new int[] { rnd.Next(0, 1023), rnd.Next(0, 1023), rnd.Next(0, 1023) },
            ProcessedEcgChannel2 = new int[] { rnd.Next(0, 1023), rnd.Next(0, 1023), rnd.Next(0, 1023) },
            ProcessedEcgChannel3 = new int[] { rnd.Next(0, 1023), rnd.Next(0, 1023), rnd.Next(0, 1023) }
        };
    }
    public EcgRawMeasurement GenerateEcgRawMeasurement()
    {
        return new EcgRawMeasurement
        {
            PatientId = "123456-0000",
            Timestamp = DateTime.UtcNow,
            EcgRawBytes = new List<sbyte[]>
            {
                new sbyte[] { 0x12, 0x02, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 },
                new sbyte[] { 0x12, 0x02, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x02 },
            }
        };
    }
}