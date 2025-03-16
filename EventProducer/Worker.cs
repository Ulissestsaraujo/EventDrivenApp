using System.Runtime.CompilerServices;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Messages;
using Shared.Models;

[assembly: InternalsVisibleTo("UnitTests")]

namespace EventProducer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IBus _bus;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Random _random = new();

    private readonly Dictionary<SensorType, string[]> _sensorIds = new()
    {
        { SensorType.Environmental, new[] { "env-001", "env-002", "env-003" } },
        { SensorType.AirQuality, new[] { "air-001", "air-002" } },
        { SensorType.Water, new[] { "water-001", "water-002" } },
        { SensorType.Energy, new[] { "energy-001", "energy-002", "energy-003" } },
        { SensorType.Motion, new[] { "motion-001", "motion-002" } },
        { SensorType.Light, new[] { "light-001", "light-002" } },
    };

    public Worker(ILogger<Worker> logger, IBus bus, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _bus = bus;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await dbContext.Database.EnsureCreatedAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error initializing database");
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var sensorData = GenerateSensorData();

                    await SaveSensorDataAsync(sensorData);

                    await _bus.Publish(MapToMessage(sensorData), stoppingToken);

                    _logger.LogInformation(
                        "Published {SensorType} data for sensor {SensorId} at {Time}",
                        sensorData.SensorType,
                        sensorData.SensorId,
                        DateTimeOffset.Now
                    );

                    await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing sensor data");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during worker initialization");
            throw;
        }
    }

    internal SensorData GenerateSensorData()
    {
        var sensorTypes = Enum.GetValues<SensorType>();
        var sensorType = sensorTypes[_random.Next(0, sensorTypes.Length)];

        var sensorIds = _sensorIds[sensorType];
        var sensorId = sensorIds[_random.Next(0, sensorIds.Length)];

        var sensorData = new SensorData
        {
            SensorId = sensorId,
            SensorType = sensorType,
            Timestamp = DateTime.UtcNow,
            Processed = false,
        };

        switch (sensorType)
        {
            case SensorType.Environmental:
                sensorData.Temperature = Math.Round(_random.NextDouble() * 40 - 10, 2); // -10 to 30 °C
                sensorData.Humidity = Math.Round(_random.NextDouble() * 100, 2); // 0 to 100 %
                sensorData.Pressure = Math.Round(_random.NextDouble() * 50 + 970, 2); // 970 to 1020 hPa
                break;

            case SensorType.AirQuality:
                sensorData.CO2 = Math.Round(_random.NextDouble() * 1500 + 400, 2); // 400 to 1900 ppm
                sensorData.VOC = Math.Round(_random.NextDouble() * 1000, 2); // 0 to 1000 ppb
                sensorData.PM25 = Math.Round(_random.NextDouble() * 50, 2); // 0 to 50 µg/m³
                sensorData.PM10 = Math.Round(_random.NextDouble() * 100, 2); // 0 to 100 µg/m³
                break;

            case SensorType.Water:
                sensorData.PH = Math.Round(_random.NextDouble() * 7 + 3, 2); // 3 to 10 pH
                sensorData.Turbidity = Math.Round(_random.NextDouble() * 10, 2); // 0 to 10 NTU
                sensorData.DissolvedOxygen = Math.Round(_random.NextDouble() * 15, 2); // 0 to 15 mg/L
                sensorData.Conductivity = Math.Round(_random.NextDouble() * 1000, 2); // 0 to 1000 µS/cm
                break;

            case SensorType.Energy:
                sensorData.Voltage = Math.Round(_random.NextDouble() * 20 + 220, 2); // 220 to 240 V
                sensorData.Current = Math.Round(_random.NextDouble() * 15, 2); // 0 to 15 A
                sensorData.PowerConsumption = Math.Round(_random.NextDouble() * 3000, 2); // 0 to 3000 W
                break;

            case SensorType.Motion:
                sensorData.AccelerationX = Math.Round(_random.NextDouble() * 20 - 10, 2); // -10 to 10 m/s²
                sensorData.AccelerationY = Math.Round(_random.NextDouble() * 20 - 10, 2); // -10 to 10 m/s²
                sensorData.AccelerationZ = Math.Round(_random.NextDouble() * 20 - 10, 2); // -10 to 10 m/s²
                sensorData.Vibration = Math.Round(_random.NextDouble() * 100, 2); // 0 to 100 Hz
                break;

            case SensorType.Light:
                sensorData.Illuminance = Math.Round(_random.NextDouble() * 10000, 2); // 0 to 10000 lux
                sensorData.UVIndex = Math.Round(_random.NextDouble() * 11, 2); // 0 to 11 UV index
                sensorData.ColorTemperature = Math.Round(_random.NextDouble() * 5000 + 2000, 2); // 2000 to 7000 K
                break;
        }

        return sensorData;
    }

    internal SensorDataMessage MapToMessage(SensorData sensorData)
    {
        return new SensorDataMessage
        {
            SensorId = sensorData.SensorId,
            SensorType = sensorData.SensorType,
            Timestamp = sensorData.Timestamp,

            // Environmental data
            Temperature = sensorData.Temperature,
            Humidity = sensorData.Humidity,
            Pressure = sensorData.Pressure,

            // Air quality data
            CO2 = sensorData.CO2,
            VOC = sensorData.VOC,
            PM25 = sensorData.PM25,
            PM10 = sensorData.PM10,

            // Water data
            PH = sensorData.PH,
            Turbidity = sensorData.Turbidity,
            DissolvedOxygen = sensorData.DissolvedOxygen,
            Conductivity = sensorData.Conductivity,

            // Energy data
            Voltage = sensorData.Voltage,
            Current = sensorData.Current,
            PowerConsumption = sensorData.PowerConsumption,

            // Motion data
            AccelerationX = sensorData.AccelerationX,
            AccelerationY = sensorData.AccelerationY,
            AccelerationZ = sensorData.AccelerationZ,
            Vibration = sensorData.Vibration,

            // Light data
            Illuminance = sensorData.Illuminance,
            UVIndex = sensorData.UVIndex,
            ColorTemperature = sensorData.ColorTemperature,
        };
    }

    internal async Task SaveSensorDataAsync(SensorData sensorData)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        dbContext.SensorData.Add(sensorData);
        await dbContext.SaveChangesAsync();
    }
}
