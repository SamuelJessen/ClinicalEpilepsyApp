using ClinicalEpilepsyApp.Domain.DBModels;
using ClinicalEpilepsyApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EpilepsyWebAPI.Controllers;

[ApiController]
public class PatientsController : ControllerBase
{
    private readonly ILogger<PatientsController> _logger;
    private readonly PatientRepository _patientRepository;

    public PatientsController(ILogger<PatientsController> logger, PatientRepository patientRepository)
    {
        _logger = logger;
        _patientRepository = patientRepository;
    }

    [HttpGet("patients")]
    public async Task<IActionResult> GetAllPatients()
    {
        try
        {
            var patients = await _patientRepository.GetAllPatientsAsync();
            return Ok(patients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all patients.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("patients/{id}")]
    public async Task<IActionResult> GetPatientById(string id)
    {
        try
        {
            var patient = await _patientRepository.GetPatientByIdAsync(id);
            return Ok(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while fetching patient with ID: {id}");
            return NotFound();
        }
    }

    [HttpPost("patients")]
    public async Task<IActionResult> AddPatient([FromBody] Patient patient)
    {
        try
        {
            await _patientRepository.AddPatientAsync(patient);
            return Ok(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding patient.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("patients/{id}")]
    public async Task<IActionResult> UpdatePatient(string id, [FromBody] Patient patient)
    {
        try
        {
            if (id != patient.Id)
            {
                return BadRequest("Patient ID mismatch");
            }

            await _patientRepository.UpdatePatientAsync(patient);
            return Ok(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while updating patient with ID: {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("patients/{id}")]
    public async Task<IActionResult> DeletePatient(string id)
    {
        try
        {
            var existingPatient = await _patientRepository.GetPatientByIdAsync(id);

            if (existingPatient == null)
            {
                return NotFound("Patient not found");
            }

            await _patientRepository.DeletePatientAsync(id);
            return Ok("Patient deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while deleting patient with ID: {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("patients/login")]
    public async Task<IActionResult> Login([FromBody] PatientLoginRequest loginRequest)
    {
        try
        {
            var patient = await _patientRepository.GetPatientByIdAsync(loginRequest.Id);

            if (patient == null)
            {
                return NotFound("Patient not found");
            }

            if (patient.Password != loginRequest.Password)
            {
                return BadRequest("Invalid password");
            }

            return Ok(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("patients/{id}/thresholds")]
    public async Task<IActionResult> UpdateThresholds(string id, [FromBody] ThresholdUpdateRequest thresholds)
    {
        try
        {
            var patient = await _patientRepository.GetPatientByIdAsync(id);

            if (patient == null)
            {
                return NotFound("Patient not found");
            }

            // Update the thresholds
            patient.CSIThreshold30 = thresholds.CSIThreshold30;
            patient.CSIThreshold50 = thresholds.CSIThreshold50;
            patient.CSIThreshold100 = thresholds.CSIThreshold100;
            patient.ModCSIThreshold100 = thresholds.ModCSIThreshold100;

            await _patientRepository.UpdatePatientAsync(patient);

            return Ok(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while updating thresholds for patient with ID: {id}");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class PatientLoginRequest
{
    public string Id { get; set; }
    public string Password { get; set; }
}

public class ThresholdUpdateRequest
{
    public int CSIThreshold30 { get; set; }
    public int CSIThreshold50 { get; set; }
    public int CSIThreshold100 { get; set; }
    public int ModCSIThreshold100 { get; set; }
}