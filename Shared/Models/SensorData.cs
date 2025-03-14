using System;

namespace Shared.Models;

public class SensorData
{
    public int Id { get; set; }
    public string SensorId { get; set; } = null!;
    public SensorType SensorType { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Processed { get; set; }

    // Environmental sensor data
    public double? Temperature { get; set; }
    public double? Humidity { get; set; }
    public double? Pressure { get; set; }

    // Air quality sensor data
    public double? CO2 { get; set; }
    public double? VOC { get; set; }
    public double? PM25 { get; set; }
    public double? PM10 { get; set; }

    // Water sensor data
    public double? PH { get; set; }
    public double? Turbidity { get; set; }
    public double? DissolvedOxygen { get; set; }
    public double? Conductivity { get; set; }

    // Energy sensor data
    public double? Voltage { get; set; }
    public double? Current { get; set; }
    public double? PowerConsumption { get; set; }

    // Motion sensor data
    public double? AccelerationX { get; set; }
    public double? AccelerationY { get; set; }
    public double? AccelerationZ { get; set; }
    public double? Vibration { get; set; }

    // Light sensor data
    public double? Illuminance { get; set; }
    public double? UVIndex { get; set; }
    public double? ColorTemperature { get; set; }
}
