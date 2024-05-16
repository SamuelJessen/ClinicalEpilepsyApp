using System.Text;
using System.Text.Json;
using ClinicalEpilepsyApp.Domain.DBModels;
using ClinicalEpilepsyApp.Domain.Models;
using EpilepsyMQTTObserver;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;

var rawMeasurementDecoder = new RawMeasurementDecoder();
string brokerAddress = "telemonmqtt-wxmbnq.a01.euc1.aws.hivemq.cloud";
string username = "TelemonBroker1";
string password = "RememberTheStack123";
string clientId = "12";

var measurementToSave = new EcgProcessedMeasurement();
var intListChan1 = new List<int>();
var intListChan2 = new List<int>();
var intListChan3 = new List<int>();

var options = new HiveMQClientOptions
{
    UseTLS = true,
    Host = brokerAddress,
    Port = 8883,
    UserName = username,
    Password = password,
    ClientId = clientId,
    SessionExpiryInterval = 150,
    CleanStart = true

};

var client = new HiveMQClient(options);

// Message Handler
//
// It's important that this is setup before we connect to the broker
// otherwise queued messages that are sent down may be lost.
//
client.OnMessageReceived += async (sender, args) =>
{
    var message = args.PublishMessage.PayloadAsString;
    if (args.PublishMessage.Topic == Topics.RawTopic)
    {
        Console.WriteLine($"Received message on topic '{args.PublishMessage.Topic}': {message}");
        var decodedMessage = DecodeMessage(message);
        var messageToPublish = new MQTT5PublishMessage
        {
            Topic = Topics.DecodedTopic,
            Payload = System.Text.Encoding.UTF8.GetBytes(decodedMessage.Result),
            QoS = QualityOfService.AtLeastOnceDelivery,
        };
        var resultPublish = await client.PublishAsync(messageToPublish).ConfigureAwait(false);
        Console.WriteLine($"Published to topic {Topics.DecodedTopic}: {resultPublish.QoS2ReasonCode}");
        Console.WriteLine($"Published message on topic '{Topics.DecodedTopic}'");
    }

    if (args.PublishMessage.Topic == Topics.ProcessedTopic)
    {
        Console.WriteLine($"Received message on topic '{args.PublishMessage.Topic}': {message}");
        PythonEcgProcessedMeasurement measurement = JsonSerializer.Deserialize<PythonEcgProcessedMeasurement>(message)!;
        if (String.IsNullOrEmpty(measurementToSave.PatientID))
        {
            measurementToSave.PatientID = measurement.PatientID;
            measurementToSave.StartTime = measurement.TimeStamp;
            measurementToSave.ProcessedMeasurementId = Guid.NewGuid();
            intListChan1.AddRange(measurement.ProcessedEcgChannel1);
            intListChan2.AddRange(measurement.ProcessedEcgChannel2);
            intListChan3.AddRange(measurement.ProcessedEcgChannel3);
        }
        else
        {
            int samplesToAdd = 12 * 21 * 5; // 5 seconds of samples
            int maxSamples = 12 * 21 * 60 * 5; // = 5 minutes of samples
            int currentSamples = intListChan1.Count;
            double percentDone = ((double)currentSamples/ maxSamples)*100;

            Console.WriteLine($"Percent done: {percentDone}");

            if (currentSamples < maxSamples)
            {
                int startIndex = measurement.ProcessedEcgChannel3.Count - samplesToAdd;
                for (int i = startIndex; i < measurement.ProcessedEcgChannel1.Count; i++)
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
                Console.WriteLine("Saved measurement in database");

            }
        }
        Console.WriteLine($"Processed message on topic '{args.PublishMessage.Topic}'");
    }

};

// Connect to the broker
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
if (connectResult.ReasonCode != HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
{
    throw new Exception($"Failed to connect: {connectResult.ReasonString}");
}

// Subscribe to a topic
var topic = Topics.RawTopic;
var subscribeResult = await client.SubscribeAsync(topic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
var subscribeResult1 = await client.SubscribeAsync(Topics.ProcessedTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
Console.WriteLine($"Subscribed to {topic}: {subscribeResult.Subscriptions[0].SubscribeReasonCode}");
Console.WriteLine($"Subscribed to {Topics.ProcessedTopic}: {subscribeResult1.Subscriptions[0].SubscribeReasonCode}");

Console.WriteLine("Waiting for 5 seconds to receive messages queued on the topic...");
await Task.Delay(5000).ConfigureAwait(false);

Console.ReadKey();

await client.DisconnectAsync().ConfigureAwait(false);

Task<string> DecodeMessage(string message)
{
    // save message to text file
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
        TimeStamp = measurement.TimeStamp,
        PatientID = measurement.PatientID,
        DecodedEcgChannel1 = chan1,
        DecodedEcgChannel2 = chan2,
        DecodedEcgChannel3 = chan3
    };

    DateTime now = DateTime.Now;
    Console.WriteLine("Timestamp:" + now);
    string decodedMessage = JsonSerializer.Serialize(decodedBatchMeasurement);

    return Task.FromResult(decodedMessage);
}

static byte[] ConvertIntArrayToByteArray(int[] array)
{
    byte[] byteArray = new byte[array.Length * sizeof(int)];
    Buffer.BlockCopy(array, 0, byteArray, 0, byteArray.Length);
    return byteArray;
}

async Task SaveToDatabase(EcgProcessedMeasurement measurement)
{
    var json = JsonSerializer.Serialize(measurement);

    var apiUrl = "https://epilepsyapi.azurewebsites.net" + "/measurements";

    using (var httpClient = new HttpClient())
    {
        try
        {
            var response = await httpClient.PostAsync(apiUrl, new StringContent(json, Encoding.UTF8, "application/json"));
            Console.WriteLine(response.IsSuccessStatusCode
                ? "Data saved successfully."
                : $"Failed to save data. Status code: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while calling the API: {ex.Message}");
        }
    }
}