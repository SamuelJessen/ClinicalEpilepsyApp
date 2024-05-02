namespace ClinicalEpilepsyApp.Domain.DBModels;

public class Patient
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int CSIThreshold { get; set; }
    public int ModCSIThreshold { get; set; }
    public string Password { get; set; }
}