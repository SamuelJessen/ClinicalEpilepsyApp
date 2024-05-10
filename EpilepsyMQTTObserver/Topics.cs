namespace EpilepsyMQTTObserver;

public static class Topics
{
    public const string RawTopic = "ecg_data_group1/measurements";
    public const string DecodedTopic = "ecg_data_group1/decoded_measurements";
    public const string ProcessedTopic = "ecg_data_group1/processed_measurements";
}