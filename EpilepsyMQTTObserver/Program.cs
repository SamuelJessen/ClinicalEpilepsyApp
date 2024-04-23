using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

string brokerAddress = "test.mosquitto.org"; // Replace with your MQTT broker address
string clientId = Guid.NewGuid().ToString(); // Generate a unique client ID
string topic = "ecg_data_group1"; // Topic to subscribe to

MqttClient mqttClient = new MqttClient(brokerAddress);

// Event handler for receiving messages
mqttClient.MqttMsgPublishReceived += (sender, e) =>
{
	string message = System.Text.Encoding.UTF8.GetString(e.Message);
	Console.WriteLine($"Received message on topic '{e.Topic}': {message}");
};

// Event handler for connection established
mqttClient.ConnectionClosed += (sender, e) =>
{
	Console.WriteLine("Disconnected from MQTT broker!");
	Console.WriteLine("Attempting to reconnect...");
	mqttClient.Connect(clientId);
	mqttClient.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
};

try
{
	mqttClient.Connect(clientId);
	Console.WriteLine("Connected to MQTT broker!");
	mqttClient.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
	Console.WriteLine($"Subscribed to topic '{topic}'");
}
catch (Exception ex)
{
	Console.WriteLine($"Failed to connect to MQTT broker: {ex.Message}");
	return;
}

Console.WriteLine("Press any key to exit.");
Console.ReadKey();

mqttClient.Disconnect();


