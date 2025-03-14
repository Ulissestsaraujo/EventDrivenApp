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

        _logger.LogInformation(
            "Received {SensorType} data from sensor {SensorId}",
            message.SensorType,
            message.SensorId
        );

        try
        {
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
}
