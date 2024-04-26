using ClinicalEpilepsyApp.Domain.DBModels;
using ClinicalEpilepsyApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EpilepsyWebAPI.Controllers;

[ApiController]
public class EcgProcessedMeasurementsController : ControllerBase
{
    private readonly EcgProcessedMeasurementRepository _repository;

    public EcgProcessedMeasurementsController(EcgProcessedMeasurementRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("measurements")]
    public async Task<ActionResult<IEnumerable<EcgProcessedMeasurement>>> GetEcgProcessedMeasurements()
    {
        var measurements = await _repository.GetAllProcessedMeasurementsAsync();
        return Ok(measurements);
    }

    [HttpGet("measurements/{id}")]
    public async Task<ActionResult<EcgProcessedMeasurement>> GetEcgProcessedMeasurement(Guid id)
    {
        var measurement = await _repository.GetProcessedMeasurementByIdAsync(id);
        if (measurement == null)
        {
            return NotFound();
        }
        return Ok(measurement);
    }

    [HttpPost("measurements")]
    public async Task<ActionResult<EcgProcessedMeasurement>> AddEcgProcessedMeasurement([FromBody] EcgProcessedMeasurement measurement)
    {
        await _repository.AddProcessedMeasurementAsync(measurement);
        return Ok(measurement);
    }
}