using System;
using System.Linq;
using System.Threading.Tasks;
using EventProducer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Data;
using Shared.Messages;
using Shared.Models;
using Xunit;

namespace UnitTests.EventProducerTests
{
    public class WorkerSaveTests
    {
        private readonly Mock<ILogger<Worker>> _loggerMock;
        private readonly Mock<IBus> _busMock;
        private readonly AppDbContext _dbContext;
        private readonly Worker _worker;

        public WorkerSaveTests()
        {
            _loggerMock = new Mock<ILogger<Worker>>();
            _busMock = new Mock<IBus>();

            // Set up in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new AppDbContext(options);

            // Set up a service provider that returns the db context
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(AppDbContext))).Returns(_dbContext);

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);

            _worker = new Worker(_loggerMock.Object, _busMock.Object, serviceScopeFactory.Object);
        }

        [Fact]
        public async Task SaveSensorDataAsync_WithEnvironmentalData_ShouldSaveToDatabase()
        {
            // Arrange
            var sensorData = new SensorData
            {
                SensorId = "env-001",
                SensorType = SensorType.Environmental,
                Temperature = 25.0,
                Humidity = 60.0,
                Pressure = 1010.0,
                Timestamp = DateTime.UtcNow,
                Processed = false,
            };

            // Act
            await _worker.SaveSensorDataAsync(sensorData);

            // Assert
            var savedData = await _dbContext.SensorData.ToListAsync();
            Assert.Single(savedData);

            var savedItem = savedData[0];
            Assert.Equal(sensorData.SensorId, savedItem.SensorId);
            Assert.Equal(sensorData.SensorType, savedItem.SensorType);
            Assert.Equal(sensorData.Temperature, savedItem.Temperature);
            Assert.Equal(sensorData.Humidity, savedItem.Humidity);
            Assert.Equal(sensorData.Pressure, savedItem.Pressure);
            Assert.Equal(sensorData.Timestamp, savedItem.Timestamp);
            Assert.Equal(sensorData.Processed, savedItem.Processed);
        }

        [Fact]
        public async Task SaveSensorDataAsync_WithAirQualityData_ShouldSaveToDatabase()
        {
            // Arrange
            var sensorData = new SensorData
            {
                SensorId = "air-001",
                SensorType = SensorType.AirQuality,
                CO2 = 850.5,
                VOC = 120.3,
                PM25 = 15.2,
                PM10 = 30.5,
                Timestamp = DateTime.UtcNow,
                Processed = false,
            };

            // Act
            await _worker.SaveSensorDataAsync(sensorData);

            // Assert
            var savedData = await _dbContext.SensorData.ToListAsync();
            Assert.Single(savedData);

            var savedItem = savedData[0];
            Assert.Equal(sensorData.SensorId, savedItem.SensorId);
            Assert.Equal(sensorData.SensorType, savedItem.SensorType);
            Assert.Equal(sensorData.CO2, savedItem.CO2);
            Assert.Equal(sensorData.VOC, savedItem.VOC);
            Assert.Equal(sensorData.PM25, savedItem.PM25);
            Assert.Equal(sensorData.PM10, savedItem.PM10);
            Assert.Equal(sensorData.Timestamp, savedItem.Timestamp);
            Assert.Equal(sensorData.Processed, savedItem.Processed);
        }

        [Fact]
        public async Task SaveSensorDataAsync_WithMultipleRecords_ShouldSaveAllRecords()
        {
            // Arrange
            var sensorData1 = new SensorData
            {
                SensorId = "env-001",
                SensorType = SensorType.Environmental,
                Temperature = 25.0,
                Humidity = 60.0,
                Pressure = 1010.0,
                Timestamp = DateTime.UtcNow.AddMinutes(-10),
                Processed = false,
            };

            var sensorData2 = new SensorData
            {
                SensorId = "water-001",
                SensorType = SensorType.Water,
                PH = 7.2,
                Turbidity = 5.3,
                DissolvedOxygen = 8.1,
                Conductivity = 450.2,
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                Processed = false,
            };

            // Act
            await _worker.SaveSensorDataAsync(sensorData1);
            await _worker.SaveSensorDataAsync(sensorData2);

            // Assert
            var savedData = await _dbContext.SensorData.ToListAsync();
            Assert.Equal(2, savedData.Count);

            // Verify first sensor data
            var saved1 = savedData.FirstOrDefault(d => d.SensorId == "env-001");
            Assert.NotNull(saved1);
            Assert.Equal(SensorType.Environmental, saved1.SensorType);
            Assert.Equal(sensorData1.Temperature, saved1.Temperature);

            // Verify second sensor data
            var saved2 = savedData.FirstOrDefault(d => d.SensorId == "water-001");
            Assert.NotNull(saved2);
            Assert.Equal(SensorType.Water, saved2.SensorType);
            Assert.Equal(sensorData2.PH, saved2.PH);
        }

        [Fact]
        public void MapToMessage_ShouldTransformSensorDataToMessage()
        {
            // Arrange
            var sensorData = new SensorData
            {
                SensorId = "light-001",
                SensorType = SensorType.Light,
                Illuminance = 12500.5,
                UVIndex = 5.2,
                ColorTemperature = 5500.0,
                Timestamp = DateTime.UtcNow,
                Processed = false,
            };

            // Act - Use reflection to call the internal method
            var message =
                _worker
                    .GetType()
                    .GetMethod(
                        "MapToMessage",
                        System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Instance
                    )
                    .Invoke(_worker, new object[] { sensorData }) as SensorDataMessage;

            // Assert
            Assert.NotNull(message);
            Assert.Equal(sensorData.SensorId, message.SensorId);
            Assert.Equal(sensorData.SensorType, message.SensorType);
            Assert.Equal(sensorData.Illuminance, message.Illuminance);
            Assert.Equal(sensorData.UVIndex, message.UVIndex);
            Assert.Equal(sensorData.ColorTemperature, message.ColorTemperature);
            Assert.Equal(sensorData.Timestamp, message.Timestamp);
        }
    }
}
