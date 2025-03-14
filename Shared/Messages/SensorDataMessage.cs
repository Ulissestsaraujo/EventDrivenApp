using System;
using Shared.Models;

namespace Shared.Messages;

public record SensorDataMessage
{
    public string SensorId { get; init; } = null!;
    public SensorType SensorType { get; init; }
    public DateTime Timestamp { get; init; }

    // Environmental sensor data
    public double? Temperature { get; init; }
    public double? Humidity { get; init; }
    public double? Pressure { get; init; }

    // Air quality sensor data
    public double? CO2 { get; init; }
    public double? VOC { get; init; }
    public double? PM25 { get; init; }
    public double? PM10 { get; init; }

    // Water sensor data
    public double? PH { get; init; }
    public double? Turbidity { get; init; }
    public double? DissolvedOxygen { get; init; }
    public double? Conductivity { get; init; }

    // Energy sensor data
    public double? Voltage { get; init; }
    public double? Current { get; init; }
    public double? PowerConsumption { get; init; }

    // Motion sensor data
    public double? AccelerationX { get; init; }
    public double? AccelerationY { get; init; }
    public double? AccelerationZ { get; init; }
    public double? Vibration { get; init; }

    // Light sensor data
    public double? Illuminance { get; init; }
    public double? UVIndex { get; init; }
    public double? ColorTemperature { get; init; }
}
