using System.Reflection.Metadata;

namespace ClinicalEpilepsyApp.Domain.DBModels;

public class EcgProcessedMeasurement
{
    // from python processing to c# observer and maui app. Sent every 5 seconds
    public Guid ProcessedMeasurementId { get; set; }
    public string PatientID { get; set; }
    public DateTime StartTime { get; set; }
    public byte[] ProcessedEcgChannel1 { get; set; }
    public byte[] ProcessedEcgChannel2 { get; set; }
    public byte[] ProcessedEcgChannel3 { get; set; }
}





