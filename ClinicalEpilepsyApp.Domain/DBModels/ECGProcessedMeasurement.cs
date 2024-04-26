namespace ClinicalEpilepsyApp.Domain.DBModels;

//public class ECGRawMeasurement
//{
//    // from maui app to mqtt c# observer. Sent every 5 seconds. Contains 
//    public string PatientId { get; set; }
//    public DateTime Timestamp { get; set; }
//    public sbyte[][] ECGRawBytes { get; set; }

//};

//public class DecodedECGMeasurement
//{
//    // from c# observer to python processing. Sent every 5 seconds
//    public string PatientId { get; set; }
//    public DateTime Timestamp { get; set; }
//    public int[] DecodedECGChannel1 { get; set; }
//    public int[] DecodedECGChannel2 { get; set; }
//    public int[] DecodedECGChannel3 { get; set; }
//};

public class EcgProcessedMeasurement
{
    // from python processing to c# observer and maui app. Sent every 5 seconds
    // saved in database
    public Guid ProcessedMeasurementId { get; set; }
    public string PatientId { get; set; }
    public int[] ProcessedEcgChannel1 { get; set; }
    public int[] ProcessedEcgChannel2 { get; set; }
    public int[] ProcessedEcgChannel3 { get; set; }
};

//public class EcgProcessedMeasurement
//{
//    // from python processing to c# observer and maui app. Sent every 5 seconds
//    // saved in database
//    public Guid ProcessedMeasurementId { get; set; }
//    public string PatientId { get; set; }
//    public int[] ProcessedEcgChannel1 { get; set; }
//    public int[] ProcessedEcgChannel2 { get; set; }
//    public int[] ProcessedEcgChannel3 { get; set; }
//};

//public class ECGAlarmValues
//{
//    // from maui to c# observer and maui app. Sent every 5 seconds
//    // saved in database
//    public Guid Id { get; set; }
//    public Guid ProcessedMeasurementId { get; set; }
//    public string PatientId { get; set; }
//    public int CSI30 { get; set; }
//    public int CSI50 { get; set; }
//    public int CSI100 { get; set; }
//    public int ModCSI30 { get; set; }
//    public int ModCSI50 { get; set; }
//    public int ModCSI100 { get; set; }
//    public int PatientCSIThreshold { get; set; }
//    public int PatientModCSIThreshold { get; set; }
//    public DateTime AlarmTimestamp { get; set; }
//}



//topics:
// ecg_data_group1/processed_measurements
// ecg_data_group1/measurements
// ecg_data_group1/alarm_values




