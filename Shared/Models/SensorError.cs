using System;

namespace Shared.Models;

public class SensorError
{
    public int Id { get; set; }
    public required string SensorId { get; set; }
    public SensorType SensorType { get; set; }
    public DateTime ErrorTimestamp { get; set; }
    public required string ErrorMessage { get; set; }
    public int ErrorCount { get; set; }
}
