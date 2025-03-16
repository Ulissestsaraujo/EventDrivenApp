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
    public async Task<IActionResult> GetSensorDataSummary(
        [FromQuery] SensorType? sensorType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 6
    )
    {
        try
        {
            IQueryable<SensorData> query = _dbContext.SensorData;

            if (sensorType.HasValue)
            {
                query = query.Where(x => x.SensorType == sensorType.Value);
            }

            var latestDataBySensor = await query
                .GroupBy(x => new { x.SensorId, x.SensorType })
                .Select(g => new
                {
                    SensorId = g.Key.SensorId,
                    SensorType = g.Key.SensorType,
                    LatestEntry = g.OrderByDescending(x => x.Timestamp).FirstOrDefault(),
                })
                .ToListAsync();

            var summary = latestDataBySensor
                .Select(x => new
                {
                    SensorId = x.SensorId,
                    SensorType = x.SensorType,
                    LatestTimestamp = x.LatestEntry.Timestamp,

                    // Environmental data
                    LatestTemperature = x.LatestEntry.Temperature,
                    LatestHumidity = x.LatestEntry.Humidity,
                    LatestPressure = x.LatestEntry.Pressure,

                    // Air quality data
                    LatestCO2 = x.LatestEntry.CO2,
                    LatestVOC = x.LatestEntry.VOC,
                    LatestPM25 = x.LatestEntry.PM25,
                    LatestPM10 = x.LatestEntry.PM10,

                    // Water data
                    LatestPH = x.LatestEntry.PH,
                    LatestTurbidity = x.LatestEntry.Turbidity,
                    LatestDissolvedOxygen = x.LatestEntry.DissolvedOxygen,
                    LatestConductivity = x.LatestEntry.Conductivity,

                    // Energy data
                    LatestVoltage = x.LatestEntry.Voltage,
                    LatestCurrent = x.LatestEntry.Current,
                    LatestPowerConsumption = x.LatestEntry.PowerConsumption,

                    // Motion data
                    LatestAccelerationX = x.LatestEntry.AccelerationX,
                    LatestAccelerationY = x.LatestEntry.AccelerationY,
                    LatestAccelerationZ = x.LatestEntry.AccelerationZ,
                    LatestVibration = x.LatestEntry.Vibration,

                    // Light data
                    LatestIlluminance = x.LatestEntry.Illuminance,
                    LatestUVIndex = x.LatestEntry.UVIndex,
                    LatestColorTemperature = x.LatestEntry.ColorTemperature,
                })
                .ToList();

            var totalCount = summary.Count;
            var pageCount = (int)Math.Ceiling(totalCount / (double)pageSize);
            var paginatedData = summary.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(
                new
                {
                    TotalCount = totalCount,
                    TotalPages = pageCount,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Data = paginatedData,
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sensor data summary");
            return StatusCode(500, "An error occurred while retrieving sensor data summary");
        }
    }
}
