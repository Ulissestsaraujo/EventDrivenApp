using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventProducer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Data;
using Shared.Models;
using Xunit;

namespace UnitTests.EventProducerTests
{
    public class WorkerTests
    {
        private readonly Mock<ILogger<Worker>> _loggerMock;
        private readonly Mock<IBus> _busMock;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly AppDbContext _dbContext;
        private readonly Worker _worker;

        public WorkerTests()
        {
            _loggerMock = new Mock<ILogger<Worker>>();
            _busMock = new Mock<IBus>();
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();

            // Set up in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            _dbContext = new AppDbContext(options);

            // Configure scope factory to return DbContext
            var mockScope = new Mock<IServiceScope>();
            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(x => x.GetService(typeof(AppDbContext))).Returns(_dbContext);
            mockScope.Setup(x => x.ServiceProvider).Returns(mockProvider.Object);
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            _worker = new Worker(_loggerMock.Object, _busMock.Object, _scopeFactoryMock.Object);
        }

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
            Assert.False(result.Processed);
            Assert.True((DateTime.UtcNow - result.Timestamp).TotalSeconds < 1); // Generated timestamp should be recent

            // Verify that the SensorType is valid
            Assert.True(Enum.IsDefined(typeof(SensorType), result.SensorType));

            // Check for specific fields based on sensor type
            switch (result.SensorType)
            {
                case SensorType.Environmental:
                    Assert.NotNull(result.Temperature);
                    Assert.NotNull(result.Humidity);
                    Assert.NotNull(result.Pressure);
                    Assert.InRange(result.Temperature.Value, -10, 40);
                    Assert.InRange(result.Humidity.Value, 0, 100);
                    Assert.InRange(result.Pressure.Value, 970, 1050);
                    break;

                case SensorType.AirQuality:
                    Assert.NotNull(result.CO2);
                    Assert.NotNull(result.PM25);
                    Assert.InRange(result.CO2.Value, 300, 5000);
                    Assert.InRange(result.PM25.Value, 0, 500);
                    break;

                case SensorType.Water:
                    Assert.NotNull(result.PH);
                    Assert.NotNull(result.DissolvedOxygen);
                    Assert.InRange(result.PH.Value, 0, 14);
                    Assert.InRange(result.DissolvedOxygen.Value, 0, 20);
                    break;

                case SensorType.Energy:
                    Assert.NotNull(result.Voltage);
                    Assert.NotNull(result.PowerConsumption);
                    Assert.InRange(result.Voltage.Value, 0, 250);
                    Assert.InRange(result.PowerConsumption.Value, 0, 10000);
                    break;

                case SensorType.Motion:
                    Assert.NotNull(result.Vibration);
                    Assert.NotNull(result.AccelerationX);
                    Assert.InRange(result.Vibration.Value, 0, 100);
                    Assert.InRange(result.AccelerationX.Value, -20, 20);
                    break;

                case SensorType.Light:
                    Assert.NotNull(result.Illuminance);
                    Assert.NotNull(result.UVIndex);
                    Assert.InRange(result.Illuminance.Value, 0, 100000);
                    Assert.InRange(result.UVIndex.Value, 0, 12);
                    break;

                default:
                    throw new Exception($"Unexpected sensor type: {result.SensorType}");
            }
        }

        [Fact]
        public void GenerateSensorData_ShouldUsePredefinedSensorIds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Worker>>();
            var busMock = new Mock<IBus>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var worker = new Worker(loggerMock.Object, busMock.Object, scopeFactoryMock.Object);

            // All expected sensor types and their IDs
            var expectedSensorIds = new Dictionary<SensorType, string[]>
            {
                { SensorType.Environmental, new[] { "env-001", "env-002", "env-003" } },
                { SensorType.AirQuality, new[] { "air-001", "air-002" } },
                { SensorType.Water, new[] { "water-001", "water-002" } },
                { SensorType.Energy, new[] { "energy-001", "energy-002", "energy-003" } },
                { SensorType.Motion, new[] { "motion-001", "motion-002" } },
                { SensorType.Light, new[] { "light-001", "light-002" } },
            };

            // Act - Generate multiple data points (enough to likely cover all sensor types)
            var results = Enumerable
                .Range(0, 100)
                .Select(_ => worker.GenerateSensorData())
                .ToList();

            // Assert
            // Check that all generated data has valid sensor IDs for its type
            foreach (var result in results)
            {
                var validIdsForType = expectedSensorIds[result.SensorType];
                Assert.Contains(result.SensorId, validIdsForType);
            }

            // Check that we got at least one sensor of each type
            foreach (var sensorType in Enum.GetValues(typeof(SensorType)).Cast<SensorType>())
            {
                Assert.Contains(results, r => r.SensorType == sensorType);
            }
        }

        [Theory]
        [InlineData(SensorType.Environmental, -10, 30, "Temperature")] // Temperature range
        [InlineData(SensorType.Environmental, 0, 100, "Humidity")] // Humidity range
        [InlineData(SensorType.AirQuality, 400, 1900, "CO2")] // CO2 range
        [InlineData(SensorType.Water, 3, 10, "PH")] // pH range
        [InlineData(SensorType.Energy, 220, 240, "Voltage")] // Voltage range
        public void GenerateSensorData_ShouldGenerateValuesWithinExpectedRanges(
            SensorType sensorType,
            double minValue,
            double maxValue,
            string propertyToCheck
        )
        {
            // Act
            var results = new List<SensorData>();
            for (int i = 0; i < 100; i++) // Generate multiple samples
            {
                var data = _worker.GenerateSensorData();
                if (data.SensorType == sensorType)
                {
                    results.Add(data);
                }
            }

            // Assert
            Assert.NotEmpty(results);
            foreach (var data in results)
            {
                var value =
                    typeof(SensorData).GetProperty(propertyToCheck)?.GetValue(data) as double?;

                Assert.NotNull(value);
                Assert.InRange(value.Value, minValue, maxValue);
            }
        }

        [Fact]
        public void GenerateSensorData_ShouldUseCorrectSensorIds()
        {
            // Arrange
            var expectedSensorIdPatterns = new Dictionary<SensorType, string>
            {
                { SensorType.Environmental, "^env-\\d{3}$" },
                { SensorType.AirQuality, "^air-\\d{3}$" },
                { SensorType.Water, "^water-\\d{3}$" },
                { SensorType.Energy, "^energy-\\d{3}$" },
                { SensorType.Motion, "^motion-\\d{3}$" },
                { SensorType.Light, "^light-\\d{3}$" },
            };

            // Act
            var results = new List<SensorData>();
            for (int i = 0; i < 100; i++) // Generate enough samples to likely get all types
            {
                results.Add(_worker.GenerateSensorData());
            }

            // Assert
            foreach (var data in results)
            {
                Assert.Matches(expectedSensorIdPatterns[data.SensorType], data.SensorId);
            }
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandlePublishingErrors()
        {
            // Arrange
            _busMock
                .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Simulated publish failure"));

            var cts = new CancellationTokenSource();

            // Act
            var workerTask = _worker.StartAsync(cts.Token);
            await Task.Delay(2000); // Let it run briefly
            cts.Cancel();
            await workerTask;

            // Assert
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => true),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleDatabaseErrors()
        {
            // Arrange
            var mockScope = new Mock<IServiceScope>();
            var mockProvider = new Mock<IServiceProvider>();
            mockProvider
                .Setup(x => x.GetService(typeof(AppDbContext)))
                .Throws(new Exception("Simulated database error"));
            mockScope.Setup(x => x.ServiceProvider).Returns(mockProvider.Object);
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            var cts = new CancellationTokenSource();

            // Act
            var workerTask = _worker.StartAsync(cts.Token);
            await Task.Delay(2000); // Let it run briefly
            cts.Cancel();
            await workerTask;

            // Assert
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => true),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task SaveSensorDataAsync_ShouldPersistDataCorrectly()
        {
            // Arrange
            var sensorData = new SensorData
            {
                SensorId = "test-001",
                SensorType = SensorType.Environmental,
                Temperature = 22.5,
                Humidity = 45.0,
                Pressure = 1013.25,
                Timestamp = DateTime.UtcNow,
            };

            // Act
            await _worker.SaveSensorDataAsync(sensorData);

            // Assert
            var savedData = await _dbContext.SensorData.FirstOrDefaultAsync();
            Assert.NotNull(savedData);
            Assert.Equal(sensorData.SensorId, savedData.SensorId);
            Assert.Equal(sensorData.Temperature, savedData.Temperature);
            Assert.Equal(sensorData.Humidity, savedData.Humidity);
            Assert.Equal(sensorData.Pressure, savedData.Pressure);
        }
    }
}
