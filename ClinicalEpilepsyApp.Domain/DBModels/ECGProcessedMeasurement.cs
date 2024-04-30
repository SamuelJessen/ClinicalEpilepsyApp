namespace ClinicalEpilepsyApp.Domain.DBModels;

public class EcgProcessedMeasurement
{
    // from python processing to c# observer and maui app. Sent every 5 seconds
    // saved in database
    public Guid ProcessedMeasurementId { get; set; }
    public string PatientId { get; set; }
    public DateTime StartTime { get; set; }
    public int[] ProcessedEcgChannel1 { get; set; }
    public int[] ProcessedEcgChannel2 { get; set; }
    public int[] ProcessedEcgChannel3 { get; set; }
};

//topics:
// ecg_data_group1/processed_measurements
// ecg_data_group1/measurements
// ecg_data_group1/alarm_values




