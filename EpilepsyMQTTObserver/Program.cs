using System.Text.Json;
using ClinicalEpilepsyApp.Domain.Models;
using EpilepsyMQTTObserver;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

string brokerAddress = "test.mosquitto.org"; // Replace with your MQTT broker address
string clientId = Guid.NewGuid().ToString(); // Generate a unique client ID

var fakepublisher = new FakeMAUIPython();
var rawMeasurementDecoder = new RawMeasurementDecoder();
fakepublisher.RunFakeMQTTClients();

MqttClient mqttClient = new MqttClient(brokerAddress);

// Event handler for receiving messages
mqttClient.MqttMsgPublishReceived += (sender, e) =>
{
	string message = System.Text.Encoding.UTF8.GetString(e.Message);
    EcgRawMeasurement measurement = JsonSerializer.Deserialize<EcgRawMeasurement>(message)!;
    var chan1 = new List<int>();
	var chan2 = new List<int>();
	var chan3 = new List<int>();
    foreach (var bytearray in measurement.EcgRawBytes)
    {
		DecodedEcgMeasurement decoded = rawMeasurementDecoder.DecodeBytes(bytearray);
        for (int i = 0; i < decoded.DecodedEcgChannel1.Length; i++)
        {
            chan1.Add(decoded.DecodedEcgChannel1[i]);
            chan2.Add(decoded.DecodedEcgChannel2[i]);
            chan3.Add(decoded.DecodedEcgChannel3[i]);
        }
    }

    Console.WriteLine($"Received message on topic '{e.Topic}': {message}");
};

// Event handler for connection established
mqttClient.ConnectionClosed += (sender, e) =>
{
	Console.WriteLine("Disconnected from MQTT broker!");
	Console.WriteLine("Attempting to reconnect...");
	mqttClient.Connect(clientId);
	mqttClient.Subscribe(new string[] { Topics.raw_topic, Topics.processed_topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
};

try
{
	mqttClient.Connect(clientId);
	Console.WriteLine("Connected to MQTT broker!");
	mqttClient.Subscribe(new string[] { Topics.raw_topic, Topics.processed_topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
	Console.WriteLine($"Subscribed to topics '{Topics.raw_topic + Topics.processed_topic}'");
}
catch (Exception ex)
{
	Console.WriteLine($"Failed to connect to MQTT broker: {ex.Message}");
	return;
}

Console.WriteLine("Press any key to exit.");
Console.ReadKey();

mqttClient.Disconnect();


