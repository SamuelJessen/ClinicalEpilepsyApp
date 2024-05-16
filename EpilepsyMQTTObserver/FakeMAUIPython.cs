using HiveMQtt.Client.Options;
using HiveMQtt.Client;

namespace EpilepsyMQTTObserver;

public class FakeMAUIPython
{
    public async void RunFakeMQTTClients()
    {
        string brokerAddress = "telemonmqtt-wxmbnq.a01.euc1.aws.hivemq.cloud";
        string username = "TelemonBroker";
        string password = "RememberTheStack123"; // Replace with your MQTT broker address
        var options = new HiveMQClientOptions
        {
            UseTLS = true,
            Host = brokerAddress,
            Port = 8883,
            UserName = username,
            Password = password,
            ClientId = "123"
        };

        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Console.WriteLine("Connected to MQTT broker!" + connectResult);

        string raw_clientId = Guid.NewGuid().ToString(); // Generate a unique client ID
        string processed_clientId = Guid.NewGuid().ToString(); // Generate a unique client ID
        //load message string from file
        var message =
            System.IO.File.ReadAllText("C:\\Users\\sljes\\OneDrive - Aarhus universitet\\Skrivebord\\message.txt");
        var processedMessage = System.IO.File.ReadAllText("C:\\Users\\sljes\\OneDrive - Aarhus universitet\\Skrivebord\\processedMessage.txt");
        //MqttClient raw_mqttClient = new MqttClient(brokerAddress);
        //MqttClient processed_mqttClient = new MqttClient(brokerAddress);

        //raw_mqttClient.Connect(raw_clientId, username, password);
        //Console.WriteLine("Raw Connected to MQTT broker!");
        //processed_mqttClient.Connect(processed_clientId, username, password);
        //Console.WriteLine("Processed Connected to MQTT broker!");

        // Serialize and publish EcgRawMeasurement every 5 seconds
        var timer1 = new System.Timers.Timer();
        timer1.Interval = 5000; // 5000 milliseconds = 5 seconds
        timer1.Elapsed += async (sender, e) =>
        {
            await client.PublishAsync(Topics.RawTopic, System.Text.Encoding.UTF8.GetBytes(message), HiveMQtt.MQTT5.Types.QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
            Console.WriteLine($"Published message on topic '{Topics.RawTopic}'");
        };
        timer1.Start();


        //// Serialize and publish EcgProcessedMeasurement every 5 seconds
        //var timer = new System.Timers.Timer();
        //timer.Interval = 5000; // 5000 milliseconds = 5 seconds
        //timer.Elapsed += (sender, e) =>
        //{
        //    processed_mqttClient.Publish(Topics.ProcessedTopic, System.Text.Encoding.UTF8.GetBytes(processedMessage), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        //};
        //timer.Start();
    }


}