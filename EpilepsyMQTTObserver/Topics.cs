namespace EpilepsyMQTTObserver;

public static class Topics
{
    public const string raw_topic = "ecg_data_group1/measurements";
    public const string processed_topic = "ecg_data_group1/processed_measurements";
    public const string alarm_topic = "ecg_data_group1/alarm_values";
}