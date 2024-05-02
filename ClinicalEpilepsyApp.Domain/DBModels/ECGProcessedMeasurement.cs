namespace ClinicalEpilepsyApp.Domain.DBModels;

public class EcgProcessedMeasurement
{
    // from python processing to c# observer and maui app. Sent every 5 seconds
    // saved in database
    public Guid ProcessedMeasurementId { get; set; }
    public string PatientId { get; set; }
    public DateTime StartTime { get; set; }
    public List<int> ProcessedEcgChannel1 { get; set; }
    public List<int> ProcessedEcgChannel2 { get; set; }
    public List<int> ProcessedEcgChannel3 { get; set; }
}





