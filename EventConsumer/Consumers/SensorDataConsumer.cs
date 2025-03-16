using System.Runtime.CompilerServices;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Messages;
using Shared.Models;

[assembly: InternalsVisibleTo("UnitTests")]

namespace EventConsumer.Consumers;

public class SensorDataConsumer : IConsumer<SensorDataMessage>
{
    private readonly ILogger<SensorDataConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public SensorDataConsumer(ILogger<SensorDataConsumer> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task Consume(ConsumeContext<SensorDataMessage> context)
    {
        var message = context.Message;

        try
        {
            // Validate the message
            if (message == null)
            {
                _logger.LogError("Message cannot be null");
                throw new NullReferenceException("Message cannot be null");
            }

            // Basic validation of required fields
            if (string.IsNullOrEmpty(message.SensorId))
            {
                _logger.LogError("Missing required field: SensorId");
                return; // Don't save to database if required fields are missing
            }

            _logger.LogInformation(
                "Received {SensorType} data from sensor {SensorId}",
                message.SensorType,
                message.SensorId
            );

            // Validate sensor data values based on type
            if (!IsValidSensorData(message))
            {
                _logger.LogError(
                    "Invalid sensor data values for sensor {SensorId}",
                    message.SensorId
                );
                return; // Don't save to database if data is invalid
            }

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Ensure database is created
            await dbContext.Database.EnsureCreatedAsync();

            // Process and store the sensor data
            var sensorData = new SensorData
            {
                SensorId = message.SensorId,
                SensorType = message.SensorType,
                Timestamp = message.Timestamp,
                Processed = true,

                // Environmental data
                Temperature = message.Temperature,
                Humidity = message.Humidity,
                Pressure = message.Pressure,

                // Air quality data
                CO2 = message.CO2,
                VOC = message.VOC,
                PM25 = message.PM25,
                PM10 = message.PM10,

                // Water data
                PH = message.PH,
                Turbidity = message.Turbidity,
                DissolvedOxygen = message.DissolvedOxygen,
                Conductivity = message.Conductivity,

                // Energy data
                Voltage = message.Voltage,
                Current = message.Current,
                PowerConsumption = message.PowerConsumption,

                // Motion data
                AccelerationX = message.AccelerationX,
                AccelerationY = message.AccelerationY,
                AccelerationZ = message.AccelerationZ,
                Vibration = message.Vibration,

                // Light data
                Illuminance = message.Illuminance,
                UVIndex = message.UVIndex,
                ColorTemperature = message.ColorTemperature,
            };

            dbContext.SensorData.Add(sensorData);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully processed and stored {SensorType} data from {SensorId}",
                message.SensorType,
                message.SensorId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sensor data message");
            throw;
        }
    }

    private bool IsValidSensorData(SensorDataMessage message)
    {
        switch (message.SensorType)
        {
            case SensorType.Environmental:
                // Check if temperature is within reasonable range (-100 to 100 Celsius)
                if (
                    message.Temperature.HasValue
                    && (message.Temperature < -100 || message.Temperature > 100)
                )
                    return false;
                // Check if humidity is within valid range (0-100%)
                if (message.Humidity.HasValue && (message.Humidity < 0 || message.Humidity > 100))
                    return false;
                break;

            case SensorType.AirQuality:
                // CO2 levels should be positive
                if (message.CO2.HasValue && message.CO2 < 0)
                    return false;
                // PM2.5 and PM10 should be positive
                if (message.PM25.HasValue && message.PM25 < 0)
                    return false;
                if (message.PM10.HasValue && message.PM10 < 0)
                    return false;
                break;

            case SensorType.Water:
                // pH should be between 0 and 14
                if (message.PH.HasValue && (message.PH < 0 || message.PH > 14))
                    return false;
                break;

            case SensorType.Energy:
                // Voltage and power consumption should be positive
                if (message.Voltage.HasValue && message.Voltage < 0)
                    return false;
                if (message.PowerConsumption.HasValue && message.PowerConsumption < 0)
                    return false;
                break;

            // Additional validations for other sensor types could be added here
        }

        return true;
    }
}
