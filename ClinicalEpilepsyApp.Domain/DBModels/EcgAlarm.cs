namespace ClinicalEpilepsyApp.Domain.DBModels
{
    public class EcgAlarm
    {
        // from maui to c# observer and maui app. Sent every 5 seconds
        // saved in database
        public Guid Id { get; set; }
        public string PatientID { get; set; }
        public int PatientCSIThreshold30 { get; set; }
        public int PatientCSIThreshold50 { get; set; }
        public int PatientCSIThreshold100 { get; set; }
        public int PatientModCSIThreshold100 { get; set; }
        public int CSI30 { get; set; }
        public int CSI50 { get; set; }
        public int CSI100 { get; set; }
        public int ModCSI100 { get; set; }
        public bool CSI30Alarm { get; set; }
        public bool CSI50Alarm { get; set; }
        public bool CSI100Alarm { get; set; }
        public bool ModCSI100Alarm { get; set; }
        public DateTime AlarmTimestamp { get; set; }
    }
}
