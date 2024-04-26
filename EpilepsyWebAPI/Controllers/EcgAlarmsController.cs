using ClinicalEpilepsyApp.Domain.DBModels;
using ClinicalEpilepsyApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EpilepsyWebAPI.Controllers;

[ApiController]
public class EcgAlarmsController : ControllerBase
{
    private readonly EcgAlarmRepository _repository;

    public EcgAlarmsController(EcgAlarmRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("alarms")]
    public async Task<ActionResult<IEnumerable<EcgAlarm>>> GetEcgAlarms()
    {
        var alarms = await _repository.GetAllAlarmsAsync();
        return Ok(alarms);
    }

    [HttpGet("alarms/forMeasurement/{id}")]
    public async Task<ActionResult<IEnumerable<EcgAlarm>>> GetAlarmsForMeasurement(Guid id)
    {
        var alarms = await _repository.GetAlarmsForMeasurementByMeasurementIdAsync(id);
        if (alarms == null)
        {
            return NotFound();
        }
        return Ok(alarms);
    }

    [HttpPost("alarms")]
    public async Task<ActionResult<EcgAlarm>> PostEcgAlarm(EcgAlarm alarm)
    {
        await _repository.AddAlarmAsync(alarm);
        return Created();
    }
}