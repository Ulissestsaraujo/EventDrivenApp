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
            if (message == null)
            {
                _logger.LogError("Message cannot be null");
                throw new NullReferenceException("Message cannot be null");
            }

            if (string.IsNullOrEmpty(message.SensorId))
            {
                _logger.LogError("Missing required field: SensorId");
                return;
            }

            _logger.LogInformation(
                "Received {SensorType} data from sensor {SensorId}",
                message.SensorType,
                message.SensorId
            );

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureCreatedAsync();

            var validationResult = IsValidSensorData(message);
            if (!validationResult.IsValid)
            {
                _logger.LogError(
                    "Invalid sensor data values for sensor {SensorId}: {ErrorMessage}",
                    message.SensorId,
                    validationResult.ErrorMessage
                );

                // Record the error
                await RecordSensorErrorAsync(
                    dbContext,
                    message.SensorId,
                    message.SensorType,
                    validationResult.ErrorMessage
                );

                return;
            }

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

    private async Task RecordSensorErrorAsync(
        AppDbContext dbContext,
        string sensorId,
        SensorType sensorType,
        string errorMessage
    )
    {
        try
        {
            // Check if there's an existing error record for this sensor
            var existingError = await dbContext.SensorErrors.FirstOrDefaultAsync(e =>
                e.SensorId == sensorId && e.SensorType == sensorType
            );

            if (existingError != null)
            {
                // Update existing error record
                existingError.ErrorCount++;
                existingError.ErrorTimestamp = DateTime.UtcNow;
                existingError.ErrorMessage = errorMessage;
            }
            else
            {
                // Create new error record
                var sensorError = new SensorError
                {
                    SensorId = sensorId,
                    SensorType = sensorType,
                    ErrorTimestamp = DateTime.UtcNow,
                    ErrorMessage = errorMessage,
                    ErrorCount = 1,
                };
                dbContext.SensorErrors.Add(sensorError);
            }

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording sensor error");
        }
    }

    private (bool IsValid, string ErrorMessage) IsValidSensorData(SensorDataMessage message)
    {
        switch (message.SensorType)
        {
            case SensorType.Environmental:
                // Check if temperature is within reasonable range (-100 to 100 Celsius)
                if (
                    message.Temperature.HasValue
                    && (message.Temperature < -100 || message.Temperature > 100)
                )
                    return (false, $"Temperature out of range: {message.Temperature}°C");
                // Check if humidity is within valid range (0-100%)
                if (message.Humidity.HasValue && (message.Humidity < 0 || message.Humidity > 100))
                    return (false, $"Humidity out of range: {message.Humidity}%");
                break;

            case SensorType.AirQuality:
                // CO2 levels should be positive
                if (message.CO2.HasValue && message.CO2 < 0)
                    return (false, $"CO2 level cannot be negative: {message.CO2}");
                // PM2.5 and PM10 should be positive
                if (message.PM25.HasValue && message.PM25 < 0)
                    return (false, $"PM2.5 level cannot be negative: {message.PM25}");
                if (message.PM10.HasValue && message.PM10 < 0)
                    return (false, $"PM10 level cannot be negative: {message.PM10}");
                break;

            case SensorType.Water:
                // pH should be between 0 and 14
                if (message.PH.HasValue && (message.PH < 0 || message.PH > 14))
                    return (false, $"pH out of valid range (0-14): {message.PH}");
                break;

            case SensorType.Energy:
                // Voltage and power consumption should be positive
                if (message.Voltage.HasValue && message.Voltage < 0)
                    return (false, $"Voltage cannot be negative: {message.Voltage}V");
                if (message.PowerConsumption.HasValue && message.PowerConsumption < 0)
                    return (
                        false,
                        $"Power consumption cannot be negative: {message.PowerConsumption}W"
                    );
                break;

            case SensorType.Motion:
                // Extreme acceleration values (over 50 m/s² is unusual)
                if (message.AccelerationX.HasValue && Math.Abs(message.AccelerationX.Value) > 50)
                    return (false, $"X-axis acceleration too extreme: {message.AccelerationX}m/s²");
                if (message.AccelerationY.HasValue && Math.Abs(message.AccelerationY.Value) > 50)
                    return (false, $"Y-axis acceleration too extreme: {message.AccelerationY}m/s²");
                if (message.AccelerationZ.HasValue && Math.Abs(message.AccelerationZ.Value) > 50)
                    return (false, $"Z-axis acceleration too extreme: {message.AccelerationZ}m/s²");
                break;

            case SensorType.Light:
                // UV Index is normally 0-11
                if (message.UVIndex.HasValue && message.UVIndex > 11)
                    return (false, $"UV Index out of range (0-11): {message.UVIndex}");
                break;
        }

        return (true, string.Empty);
    }
}
