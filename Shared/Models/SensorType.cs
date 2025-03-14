namespace Shared.Models;

public enum SensorType
{
    Environmental, // Temperature, humidity, pressure
    AirQuality, // CO2, VOC, PM2.5, PM10
    Water, // pH, turbidity, dissolved oxygen, conductivity
    Energy, // Voltage, current, power consumption
    Motion, // Acceleration, orientation, vibration
    Light, // Illuminance, UV index, color temperature
}
