namespace ClinicalEpilepsyApp.Domain.DBModels
{
    public class EcgAlarm
    {
        // from maui to c# observer and maui app. Sent every 5 seconds
        // saved in database
        public Guid Id { get; set; }
        public string PatientID { get; set; }
        public int CSI30 { get; set; }
        public int CSI50 { get; set; }
        public int CSI100 { get; set; }
        public int ModCSI100 { get; set; }
        public int PatientCSIThreshold { get; set; }
        public int PatientModCSIThreshold { get; set; }
        public DateTime AlarmTimestamp { get; set; }
    }
}
