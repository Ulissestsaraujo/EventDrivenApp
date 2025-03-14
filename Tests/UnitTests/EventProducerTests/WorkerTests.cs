using EventProducer;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Models;
using Xunit;

namespace UnitTests.EventProducerTests
{
    public class WorkerTests
    {
        [Fact]
        public void GenerateSensorData_ShouldReturnValidData()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Worker>>();
            var busMock = new Mock<IBus>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var worker = new Worker(loggerMock.Object, busMock.Object, scopeFactoryMock.Object);

            // Act
            var result = worker.GenerateSensorData();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SensorId);
            Assert.InRange(result.Temperature, -10, 30);
            Assert.InRange(result.Humidity, 0, 100);
            Assert.InRange(result.Pressure, 970, 1020);
            Assert.False(result.Processed);
            Assert.True((DateTime.UtcNow - result.Timestamp).TotalSeconds < 1); // Generated timestamp should be recent
        }

        [Fact]
        public void GenerateSensorData_ShouldUsePredefinedSensorIds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Worker>>();
            var busMock = new Mock<IBus>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var worker = new Worker(loggerMock.Object, busMock.Object, scopeFactoryMock.Object);

            // Act - Generate multiple data points
            var validSensorIds = new[] { "sensor-001", "sensor-002", "sensor-003" };
            var results = Enumerable.Range(0, 50).Select(_ => worker.GenerateSensorData()).ToList();

            // Assert
            foreach (var result in results)
            {
                Assert.Contains(result.SensorId, validSensorIds);
            }

            // At least one of each sensor ID should appear in a large enough sample
            Assert.Contains(results, r => r.SensorId == "sensor-001");
            Assert.Contains(results, r => r.SensorId == "sensor-002");
            Assert.Contains(results, r => r.SensorId == "sensor-003");
        }
    }
}
