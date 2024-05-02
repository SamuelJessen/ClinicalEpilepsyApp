using System.Text.Json;
using ClinicalEpilepsyApp.Domain.DBModels;
using ClinicalEpilepsyApp.Domain.Models;
using EpilepsyMQTTObserver;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

string brokerAddress = "test.mosquitto.org"; 
string clientId = Guid.NewGuid().ToString(); 

//var fakepublisher = new FakeMAUIPython();
var rawMeasurementDecoder = new RawMeasurementDecoder();
//fakepublisher.RunFakeMQTTClients();

MqttClient mqttClient = new MqttClient(brokerAddress);

// Event handler for receiving messages
mqttClient.MqttMsgPublishReceived += (sender, e) =>
{
	string message = System.Text.Encoding.UTF8.GetString(e.Message);
    if (e.Topic == Topics.raw_topic)
    {
        Console.WriteLine($"Received message on topic '{e.Topic}': {message}");
        EcgRawMeasurement measurement = JsonSerializer.Deserialize<EcgRawMeasurement>(message)!;
        var chan1 = new List<int[]>();
        var chan2 = new List<int[]>();
        var chan3 = new List<int[]>();
        foreach (var bytearray in measurement.EcgRawBytes)
        {
            DecodedEcgMeasurement decoded = rawMeasurementDecoder.DecodeBytes(bytearray);
            chan1.Add(decoded.DecodedEcgChannel1);
            chan2.Add(decoded.DecodedEcgChannel2);
            chan3.Add(decoded.DecodedEcgChannel3);
        }
        var decodedBatchMeasurement = new DecodedEcgBatchMeasurement()
        {
            Timestamp = measurement.Timestamp,
            PatientId = measurement.PatientId,
            DecodedEcgChannel1 = chan1,
            DecodedEcgChannel2 = chan2,
            DecodedEcgChannel3 = chan3
        };


        string decodedMessage = JsonSerializer.Serialize(decodedBatchMeasurement);
        mqttClient.Publish(Topics.decoded_topic, System.Text.Encoding.UTF8.GetBytes(decodedMessage), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
    }

    if (e.Topic == Topics.processed_topic)
    {
        Console.WriteLine($"Received message on topic '{e.Topic}': {message}");
        EcgProcessedMeasurement measurement = JsonSerializer.Deserialize<EcgProcessedMeasurement>(message)!;

    }

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


