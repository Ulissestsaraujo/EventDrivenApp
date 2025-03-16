using System;
using System.Linq;
using System.Threading.Tasks;
using EventConsumer.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Data;
using Shared.Messages;
using Shared.Models;
using Xunit;

namespace UnitTests.EventConsumerTests
{
    public class SensorDataConsumerTests
    {
        private readonly Mock<ILogger<SensorDataConsumer>> _loggerMock;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly AppDbContext _dbContext;
        private readonly SensorDataConsumer _consumer;
        private readonly Mock<ConsumeContext<SensorDataMessage>> _consumeContextMock;

        public SensorDataConsumerTests()
        {
            _loggerMock = new Mock<ILogger<SensorDataConsumer>>();
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new AppDbContext(options);

            _serviceProviderMock
                .Setup(sp => sp.GetService(typeof(AppDbContext)))
                .Returns(_dbContext);

            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);

            _scopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(_serviceScopeMock.Object);

            _consumer = new SensorDataConsumer(_loggerMock.Object, _scopeFactoryMock.Object);
            _consumeContextMock = new Mock<ConsumeContext<SensorDataMessage>>();
        }

        [Fact]
        public async Task Consume_EnvironmentalSensorData_ShouldProcessAndSaveToDatabase()
        {
            // Arrange
            var message = new SensorDataMessage
            {
                SensorId = "env-001",
                SensorType = SensorType.Environmental,
                Temperature = 22.5,
                Humidity = 45.0,
                Pressure = 1013.25,
                Timestamp = DateTime.UtcNow,
            };

            var contextMock = new Mock<ConsumeContext<SensorDataMessage>>();
            contextMock.Setup(m => m.Message).Returns(message);

            // Act
            await _consumer.Consume(contextMock.Object);

            // Assert
            var savedData = await _dbContext.SensorData.ToListAsync();
            Assert.Single(savedData);

            var savedItem = savedData.First();
            Assert.Equal(message.SensorId, savedItem.SensorId);
            Assert.Equal(message.SensorType, savedItem.SensorType);
            Assert.Equal(message.Temperature, savedItem.Temperature);
            Assert.Equal(message.Humidity, savedItem.Humidity);
            Assert.Equal(message.Pressure, savedItem.Pressure);
            Assert.Equal(message.Timestamp, savedItem.Timestamp);
            Assert.True(savedItem.Processed);

            // Verify logging occurs
            _loggerMock.Verify(
                x =>
                    x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v != null),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task Consume_AirQualitySensorData_ShouldProcessAndSaveToDatabase()
        {
            // Arrange
            var message = new SensorDataMessage
            {
                SensorId = "air-001",
                SensorType = SensorType.AirQuality,
                CO2 = 850.5,
                VOC = 120.3,
                PM25 = 15.2,
                PM10 = 30.5,
                Timestamp = DateTime.UtcNow,
            };

            var contextMock = new Mock<ConsumeContext<SensorDataMessage>>();
            contextMock.Setup(m => m.Message).Returns(message);

            // Act
            await _consumer.Consume(contextMock.Object);

            // Assert
            var savedData = await _dbContext.SensorData.ToListAsync();
            Assert.Single(savedData);

            var savedItem = savedData.First();
            Assert.Equal(message.SensorId, savedItem.SensorId);
            Assert.Equal(message.SensorType, savedItem.SensorType);
            Assert.Equal(message.CO2, savedItem.CO2);
            Assert.Equal(message.VOC, savedItem.VOC);
            Assert.Equal(message.PM25, savedItem.PM25);
            Assert.Equal(message.PM10, savedItem.PM10);
            Assert.Equal(message.Timestamp, savedItem.Timestamp);
            Assert.True(savedItem.Processed);

            _loggerMock.Verify(
                x =>
                    x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v != null),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task Consume_OtherSensorTypes_ShouldHandleCorrectly()
        {
            var waterMessage = new SensorDataMessage
            {
                SensorId = "water-001",
                SensorType = SensorType.Water,
                PH = 7.2,
                Turbidity = 0.8,
                DissolvedOxygen = 8.5,
                Conductivity = 310.0,
                Timestamp = DateTime.UtcNow,
            };
            _consumeContextMock.Setup(x => x.Message).Returns(waterMessage);
            await _consumer.Consume(_consumeContextMock.Object);

            var waterData = await _dbContext.SensorData.FirstOrDefaultAsync(d =>
                d.SensorId == "water-001"
            );
            Assert.NotNull(waterData);
            Assert.Equal(waterMessage.PH, waterData.PH);
            Assert.Equal(waterMessage.Turbidity, waterData.Turbidity);

            _dbContext.SensorData.RemoveRange(_dbContext.SensorData);
            await _dbContext.SaveChangesAsync();

            var energyMessage = new SensorDataMessage
            {
                SensorId = "energy-001",
                SensorType = SensorType.Energy,
                Voltage = 230.5,
                Current = 2.7,
                PowerConsumption = 621.35,
                Timestamp = DateTime.UtcNow,
            };
            _consumeContextMock.Setup(x => x.Message).Returns(energyMessage);
            await _consumer.Consume(_consumeContextMock.Object);

            var energyData = await _dbContext.SensorData.FirstOrDefaultAsync(d =>
                d.SensorId == "energy-001"
            );
            Assert.NotNull(energyData);
            Assert.Equal(energyMessage.Voltage, energyData.Voltage);
            Assert.Equal(energyMessage.PowerConsumption, energyData.PowerConsumption);
        }

        [Fact]
        public async Task Consume_WithInvalidMessage_ShouldThrowException()
        {
            SensorDataMessage? message = null;

            var contextMock = new Mock<ConsumeContext<SensorDataMessage>>();
            contextMock.Setup(m => m.Message).Returns(message!);

            await Assert.ThrowsAsync<NullReferenceException>(
                () => _consumer.Consume(contextMock.Object)
            );

            _loggerMock.Verify(
                x =>
                    x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v != null),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.AtLeastOnce
            );

            var savedData = await _dbContext.SensorData.ToListAsync();
            Assert.Empty(savedData);
        }

        [Fact]
        public async Task Consume_InvalidData_ShouldLogErrorAndNotSave()
        {
            var message = new SensorDataMessage
            {
                SensorId = "environmental-001",
                SensorType = SensorType.Environmental,
                Temperature = -273.16,
                Humidity = 150.0,
                Timestamp = DateTime.UtcNow,
            };
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            await _consumer.Consume(_consumeContextMock.Object);

            var savedData = await _dbContext.SensorData.FirstOrDefaultAsync();
            Assert.Null(savedData);
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v != null),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task Consume_DuplicateData_ShouldHandleGracefully()
        {
            var message = new SensorDataMessage
            {
                SensorId = "environmental-001",
                SensorType = SensorType.Environmental,
                Temperature = 22.5,
                Humidity = 45.0,
                Timestamp = DateTime.UtcNow,
            };
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            await _consumer.Consume(_consumeContextMock.Object);
            await _consumer.Consume(_consumeContextMock.Object);

            var savedDataCount = await _dbContext.SensorData.CountAsync();
            Assert.Equal(2, savedDataCount);
        }

        [Fact]
        public async Task Consume_DatabaseError_ShouldHandleGracefully()
        {
            var message = new SensorDataMessage
            {
                SensorId = "environmental-001",
                SensorType = SensorType.Environmental,
                Temperature = 22.5,
                Humidity = 45.0,
                Timestamp = DateTime.UtcNow,
            };
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            await _dbContext.DisposeAsync();

            await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await _consumer.Consume(_consumeContextMock.Object)
            );
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v != null),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task Consume_MissingRequiredFields_ShouldLogErrorAndNotSave()
        {
            var message = new SensorDataMessage { Temperature = 22.5, Timestamp = DateTime.UtcNow };
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            await _consumer.Consume(_consumeContextMock.Object);

            var savedData = await _dbContext.SensorData.FirstOrDefaultAsync();
            Assert.Null(savedData);
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v != null),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.AtLeastOnce
            );
        }
    }
}
