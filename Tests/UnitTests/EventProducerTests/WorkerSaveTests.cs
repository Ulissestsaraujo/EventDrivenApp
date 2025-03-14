using System;
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
    public class WorkerSaveTests
    {
        private readonly Mock<ILogger<Worker>> _loggerMock;
        private readonly Mock<IBus> _busMock;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly AppDbContext _dbContext;
        private readonly Worker _worker;

        public WorkerSaveTests()
        {
            _loggerMock = new Mock<ILogger<Worker>>();
            _busMock = new Mock<IBus>();
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            // Set up in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new AppDbContext(options);

            // Configure service provider to return the DbContext
            _serviceProviderMock
                .Setup(sp => sp.GetService(typeof(AppDbContext)))
                .Returns(_dbContext);

            // Configure service scope to return the service provider
            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);

            // Configure scope factory to return the service scope
            _scopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(_serviceScopeMock.Object);

            _worker = new Worker(_loggerMock.Object, _busMock.Object, _scopeFactoryMock.Object);
        }

        [Fact]
        public async Task SaveSensorDataAsync_ShouldSaveDataToDatabase()
        {
            // Arrange
            var sensorData = new SensorData
            {
                SensorId = "test-sensor",
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
            Assert.Equal(sensorData.Temperature, savedItem.Temperature);
            Assert.Equal(sensorData.Humidity, savedItem.Humidity);
            Assert.Equal(sensorData.Pressure, savedItem.Pressure);
            Assert.Equal(sensorData.Timestamp, savedItem.Timestamp);
            Assert.Equal(sensorData.Processed, savedItem.Processed);
        }

        [Fact]
        public async Task SaveSensorDataAsync_WithMultipleRecords_ShouldSaveAllRecords()
        {
            // Arrange
            var sensorData1 = new SensorData
            {
                SensorId = "test-sensor-1",
                Temperature = 25.0,
                Humidity = 60.0,
                Pressure = 1010.0,
                Timestamp = DateTime.UtcNow.AddMinutes(-10),
                Processed = false,
            };

            var sensorData2 = new SensorData
            {
                SensorId = "test-sensor-2",
                Temperature = 26.0,
                Humidity = 62.0,
                Pressure = 1011.0,
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
            var saved1 = savedData.FirstOrDefault(d => d.SensorId == "test-sensor-1");
            Assert.NotNull(saved1);
            Assert.Equal(sensorData1.Temperature, saved1.Temperature);

            // Verify second sensor data
            var saved2 = savedData.FirstOrDefault(d => d.SensorId == "test-sensor-2");
            Assert.NotNull(saved2);
            Assert.Equal(sensorData2.Temperature, saved2.Temperature);
        }
    }
}
