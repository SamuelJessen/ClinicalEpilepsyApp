namespace ClinicalEpilepsyApp.Domain.Models;

public class PythonEcgProcessedMeasurement
{
    // from python processing to c# observer and maui app. Sent every 5 seconds
    // NOT saved in database
    public string PatientId { get; set; }
    public DateTime TimeStamp { get; set; }
    public int[] ProcessedEcgChannel1 { get; set; }
    public int[] ProcessedEcgChannel2 { get; set; }
    public int[] ProcessedEcgChannel3 { get; set; }
};





