﻿namespace ClinicalEpilepsyApp.Domain.Models
{
    public class DecodedEcgMeasurement
    {
        // from c# observer to python processing. Sent every 5 seconds
        public string PatientId { get; set; }
        public DateTime Timestamp { get; set; }
        public int[] DecodedEcgChannel1 { get; set; }
        public int[] DecodedEcgChannel2 { get; set; }
        public int[] DecodedEcgChannel3 { get; set; }
    }
}