using System.Text;
using System.Text.Json;
using ClinicalEpilepsyApp.Domain.DBModels;
using ClinicalEpilepsyApp.Domain.Models;
using EpilepsyMQTTObserver;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

string brokerAddress = "test.mosquitto.org";
string clientId = Guid.NewGuid().ToString();
FakeMAUIPython fakeMAUIPython = new FakeMAUIPython();
fakeMAUIPython.RunFakeMQTTClients();

var rawMeasurementDecoder = new RawMeasurementDecoder();
MqttClient mqttClient = new MqttClient(brokerAddress);

var measurementToSave = new EcgProcessedMeasurement();
var intListChan1 = new List<int>();
var intListChan2 = new List<int>();
var intListChan3 = new List<int>();

// Event handler for receiving messages
mqttClient.MqttMsgPublishReceived += async (sender, e) =>
{
    string message = Encoding.UTF8.GetString(e.Message);
    if (e.Topic == Topics.RawTopic)
    {
        Console.WriteLine($"Received message on topic '{e.Topic}': {message}");
        var decodedMessage = await DecodeMessage(message);

        mqttClient.Publish(Topics.DecodedTopic, Encoding.UTF8.GetBytes(decodedMessage), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        Console.WriteLine($"Published message on topic '{Topics.DecodedTopic}': {decodedMessage}");
    }

    if (e.Topic == Topics.ProcessedTopic)
    {
        Console.WriteLine($"Received message on topic '{e.Topic}': {message}");
        PythonEcgProcessedMeasurement measurement = JsonSerializer.Deserialize<PythonEcgProcessedMeasurement>(message)!;
        if (String.IsNullOrEmpty(measurementToSave.PatientID))
        {
            measurementToSave.PatientID = measurement.PatientID;
            measurementToSave.StartTime = measurement.TimeStamp;
            measurementToSave.ProcessedMeasurementId = Guid.NewGuid();
            for (int i = 0; i < measurement.ProcessedEcgChannel1.Length-1; i++)
            {
                intListChan1.Add(measurement.ProcessedEcgChannel1[i]);
                intListChan2.Add(measurement.ProcessedEcgChannel2[i]);
                intListChan3.Add(measurement.ProcessedEcgChannel3[i]);
            }   
        }
        else
        {
            int samplesToAdd = 12 * 21 * 5; // 5 seconds of samples
            int maxSamples = 12 * 21 * 240; // 240 = 4 minutes of samples

            int currentSamples = intListChan1.Count;

            if (currentSamples < maxSamples)
            {
                int remainingSamples = maxSamples - currentSamples;
                int startIndex = Math.Max(measurement.ProcessedEcgChannel3.Length - remainingSamples, 0);
                for (int i = startIndex; i < measurement.ProcessedEcgChannel3.Length; i++)
                {
                    intListChan1.Add(measurement.ProcessedEcgChannel1[i]);
                    intListChan2.Add(measurement.ProcessedEcgChannel2[i]);
                    intListChan3.Add(measurement.ProcessedEcgChannel3[i]);
                }
            }
            else
            {
                //save to database
                measurementToSave.ProcessedEcgChannel1 = ConvertIntArrayToByteArray(intListChan1.ToArray());
                measurementToSave.ProcessedEcgChannel2 = ConvertIntArrayToByteArray(intListChan2.ToArray());
                measurementToSave.ProcessedEcgChannel3 = ConvertIntArrayToByteArray(intListChan3.ToArray());
                await SaveToDatabase(measurementToSave);
                //Reset the measurement to save:
                measurementToSave = new EcgProcessedMeasurement();
                intListChan1 = new List<int>();
                intListChan2 = new List<int>();
                intListChan3 = new List<int>();

            }
        }
    }
};

// Event handler for connection established
mqttClient.ConnectionClosed += (sender, e) =>
{
    Console.WriteLine("Disconnected from MQTT broker!");
    Console.WriteLine("Attempting to reconnect...");
    mqttClient.Connect(clientId);
    mqttClient.Subscribe(new string[] { Topics.RawTopic, Topics.ProcessedTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
};

try
{
    mqttClient.Connect(clientId);
    Console.WriteLine("Connected to MQTT broker!");
    mqttClient.Subscribe(new string[] { Topics.RawTopic, Topics.ProcessedTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    Console.WriteLine($"Subscribed to topics '{Topics.RawTopic + Topics.ProcessedTopic}'");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to connect to MQTT broker: {ex.Message}");
    return;
}

Console.WriteLine("Press any key to exit.");
Console.ReadKey();

mqttClient.Disconnect();

Task<string> DecodeMessage(string message)
{
    EcgRawMeasurement measurement = JsonSerializer.Deserialize<EcgRawMeasurement>(message)!;
    var chan1 = new List<int[]>();
    var chan2 = new List<int[]>();
    var chan3 = new List<int[]>();
    foreach (var bytearray in measurement.EcgRawBytes)
    {
        DecodedEcgMeasurement decoded = rawMeasurementDecoder.DecodeBytes(bytearray);
        int[] chan1Copy = decoded.DecodedEcgChannel1.ToArray();
        int[] chan2Copy = decoded.DecodedEcgChannel2.ToArray();
        int[] chan3Copy = decoded.DecodedEcgChannel3.ToArray();
        chan1.Add(chan1Copy);
        chan2.Add(chan2Copy);
        chan3.Add(chan3Copy);
    }
    var decodedBatchMeasurement = new DecodedEcgBatchMeasurement()
    {
        TimeStamp = measurement.Timestamp,
        PatientId = measurement.PatientId,
        DecodedEcgChannel1 = chan1,
        DecodedEcgChannel2 = chan2,
        DecodedEcgChannel3 = chan3
    };

    DateTime now = DateTime.Now;
    Console.WriteLine("Timestamp:" + now);
    string decodedMessage = JsonSerializer.Serialize(decodedBatchMeasurement);

    return Task.FromResult(decodedMessage);
}

async Task SaveToDatabase(EcgProcessedMeasurement measurement)
{
    var json = JsonSerializer.Serialize(measurement);

    var apiUrl = "https://epilepsyapi.azurewebsites.net" + "/measurements";

    using (var httpClient = new HttpClient())
    {
        try
        {
            // Post the JSON data to the API endpoint
            var response = await httpClient.PostAsync(apiUrl, new StringContent(json, Encoding.UTF8, "application/json"));

            // Check if the response is successful
            Console.WriteLine(response.IsSuccessStatusCode
                ? "Data saved successfully."
                : $"Failed to save data. Status code: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while calling the API: {ex.Message}");
            // Handle the exception as needed
        }
    }
}
static byte[] ConvertIntArrayToByteArray(int[] array)
{
    byte[] byteArray = new byte[array.Length * sizeof(int)];
    Buffer.BlockCopy(array, 0, byteArray, 0, byteArray.Length);
    return byteArray;
}


