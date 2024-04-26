namespace ClinicalEpilepsyApp.Domain.Models
{
    public class EcgRawMeasurement
    {
        // from maui app to mqtt c# observer. Sent every 5 seconds. Contains 
        public string PatientId { get; set; }
        public DateTime Timestamp { get; set; }
        public List<sbyte[]> EcgRawBytes { get; set; }
    }
}
