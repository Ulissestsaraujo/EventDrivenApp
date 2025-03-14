using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Models;

namespace EventConsumer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorDataController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SensorDataController> _logger;

    public SensorDataController(AppDbContext dbContext, ILogger<SensorDataController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSensorData()
    {
        try
        {
            var sensorData = await _dbContext
                .SensorData.OrderByDescending(x => x.Timestamp)
                .Take(100)
                .ToListAsync();

            return Ok(sensorData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sensor data");
            return StatusCode(500, "An error occurred while retrieving sensor data");
        }
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestSensorData()
    {
        try
        {
            var latestData = await _dbContext
                .SensorData.OrderByDescending(x => x.Timestamp)
                .Take(10)
                .ToListAsync();

            return Ok(latestData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest sensor data");
            return StatusCode(500, "An error occurred while retrieving latest sensor data");
        }
    }

    [HttpGet("bySensor/{sensorId}")]
    public async Task<IActionResult> GetSensorDataBySensorId(string sensorId)
    {
        try
        {
            var sensorData = await _dbContext
                .SensorData.Where(x => x.SensorId == sensorId)
                .OrderByDescending(x => x.Timestamp)
                .Take(50)
                .ToListAsync();

            if (!sensorData.Any())
            {
                return NotFound($"No data found for sensor ID: {sensorId}");
            }

            return Ok(sensorData);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving sensor data for sensor ID: {SensorId}",
                sensorId
            );
            return StatusCode(
                500,
                $"An error occurred while retrieving data for sensor ID: {sensorId}"
            );
        }
    }

    [HttpGet("byType/{sensorType}")]
    public async Task<IActionResult> GetSensorDataBySensorType(SensorType sensorType)
    {
        try
        {
            var sensorData = await _dbContext
                .SensorData.Where(x => x.SensorType == sensorType)
                .OrderByDescending(x => x.Timestamp)
                .Take(50)
                .ToListAsync();

            if (!sensorData.Any())
            {
                return NotFound($"No data found for sensor type: {sensorType}");
            }

            return Ok(sensorData);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving sensor data for sensor type: {SensorType}",
                sensorType
            );
            return StatusCode(
                500,
                $"An error occurred while retrieving sensor data for sensor type: {sensorType}"
            );
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSensorDataSummary()
    {
        try
        {
            var summary = await _dbContext
                .SensorData.GroupBy(x => new { x.SensorId, x.SensorType })
                .Select(g => new
                {
                    SensorId = g.Key.SensorId,
                    SensorType = g.Key.SensorType,
                    LatestTimestamp = g.Max(x => x.Timestamp),

                    // Environmental data
                    LatestTemperature = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.Temperature)
                        .FirstOrDefault(),
                    LatestHumidity = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.Humidity)
                        .FirstOrDefault(),
                    LatestPressure = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.Pressure)
                        .FirstOrDefault(),

                    // Air quality data
                    LatestCO2 = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.CO2)
                        .FirstOrDefault(),
                    LatestVOC = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.VOC)
                        .FirstOrDefault(),
                    LatestPM25 = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.PM25)
                        .FirstOrDefault(),
                    LatestPM10 = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.PM10)
                        .FirstOrDefault(),

                    // Water data
                    LatestPH = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.PH)
                        .FirstOrDefault(),
                    LatestTurbidity = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.Turbidity)
                        .FirstOrDefault(),
                    LatestDissolvedOxygen = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.DissolvedOxygen)
                        .FirstOrDefault(),
                    LatestConductivity = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.Conductivity)
                        .FirstOrDefault(),

                    // Energy data
                    LatestVoltage = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.Voltage)
                        .FirstOrDefault(),
                    LatestCurrent = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.Current)
                        .FirstOrDefault(),
                    LatestPowerConsumption = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.PowerConsumption)
                        .FirstOrDefault(),

                    // Motion data
                    LatestAccelerationX = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.AccelerationX)
                        .FirstOrDefault(),
                    LatestAccelerationY = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.AccelerationY)
                        .FirstOrDefault(),
                    LatestAccelerationZ = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.AccelerationZ)
                        .FirstOrDefault(),
                    LatestVibration = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.Vibration)
                        .FirstOrDefault(),

                    // Light data
                    LatestIlluminance = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.Illuminance)
                        .FirstOrDefault(),
                    LatestUVIndex = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.UVIndex)
                        .FirstOrDefault(),
                    LatestColorTemperature = g.OrderByDescending(x => x.Timestamp)
                        .Select(x => x.ColorTemperature)
                        .FirstOrDefault(),
                })
                .ToListAsync();

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sensor data summary");
            return StatusCode(500, "An error occurred while retrieving sensor data summary");
        }
    }
}
