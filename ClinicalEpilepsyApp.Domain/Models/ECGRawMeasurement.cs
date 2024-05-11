namespace ClinicalEpilepsyApp.Domain.Models
{
    public class EcgRawMeasurement
    {
        // from maui app to mqtt c# observer. Sent every 5 seconds. Contains 
        public string PatientID { get; set; }
        public DateTime TimeStamp { get; set; }
        public List<sbyte[]> EcgRawBytes { get; set; }
        public int Samples { get; set; }
    }
}
