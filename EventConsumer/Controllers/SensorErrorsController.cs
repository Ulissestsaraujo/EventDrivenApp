using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Data;

namespace EventConsumer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorErrorsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SensorErrorsController> _logger;

    public SensorErrorsController(AppDbContext dbContext, ILogger<SensorErrorsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSensorErrors()
    {
        try
        {
            var errors = await _dbContext
                .SensorErrors
                .OrderByDescending(e => e.ErrorCount)
                .ThenByDescending(e => e.ErrorTimestamp)
                .Take(3)
                .ToListAsync();

            var result = errors
                .Select(e => new
                {
                    sensorId = e.SensorId,
                    sensorType = e.SensorType,
                    errorCount = e.ErrorCount,
                    lastErrorTimestamp = e.ErrorTimestamp,
                    lastErrorMessage = e.ErrorMessage,
                })
                .ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sensor errors");
            return StatusCode(500, "An error occurred while retrieving sensor errors");
        }
    }
}
