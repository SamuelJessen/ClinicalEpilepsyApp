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
        //load message string from file
        var message =
            System.IO.File.ReadAllText("C:\\Users\\sljes\\OneDrive - Aarhus universitet\\Skrivebord\\message.txt");
        var processedMessage = System.IO.File.ReadAllText("C:\\Users\\sljes\\OneDrive - Aarhus universitet\\Skrivebord\\processedMessage.txt");
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
            raw_mqttClient.Publish(raw_topic, System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        };
        timer1.Start();


        // Serialize and publish EcgProcessedMeasurement every 5 seconds
        var timer = new System.Timers.Timer();
        timer.Interval = 5000; // 5000 milliseconds = 5 seconds
        timer.Elapsed += (sender, e) =>
        {
            processed_mqttClient.Publish(Topics.ProcessedTopic, System.Text.Encoding.UTF8.GetBytes(processedMessage), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        };
        timer.Start();
    }
}