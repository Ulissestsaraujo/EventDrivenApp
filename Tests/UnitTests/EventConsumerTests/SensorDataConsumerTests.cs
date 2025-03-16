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

            _consumer = new SensorDataConsumer(_loggerMock.Object, _scopeFactoryMock.Object);
            _consumeContextMock = new Mock<ConsumeContext<SensorDataMessage>>();
        }

        [Fact]
        public async Task Consume_EnvironmentalSensorData_ShouldProcessMessageAndSaveToDatabase()
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

            // Verify logger was called
            _loggerMock.Verify(
                x =>
                    x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (v, t) => v.ToString().Contains("Successfully processed")
                        ),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task Consume_AirQualitySensorData_ShouldProcessMessageAndSaveToDatabase()
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
        }

        [Fact]
        public async Task Consume_WithInvalidMessage_ShouldThrowException()
        {
            // Arrange
            // Create a null message to simulate an error condition
            SensorDataMessage message = null;

            var contextMock = new Mock<ConsumeContext<SensorDataMessage>>();
            contextMock.Setup(m => m.Message).Returns(message);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(
                () => _consumer.Consume(contextMock.Object)
            );

            // Verify error was logged - we expect two error logs:
            // 1. "Message cannot be null"
            // 2. The exception log in the catch block
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => true),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Exactly(2)
            );

            // Verify nothing was saved
            var savedData = await _dbContext.SensorData.ToListAsync();
            Assert.Empty(savedData);
        }

        [Fact]
        public async Task Consume_ValidEnvironmentalData_ShouldSaveToDatabase()
        {
            // Arrange
            var message = new SensorDataMessage
            {
                SensorId = "environmental-001",
                SensorType = SensorType.Environmental,
                Temperature = 22.5,
                Humidity = 45.0,
                Pressure = 1013.25,
                Timestamp = DateTime.UtcNow,
            };
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            // Act
            await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            var savedData = await _dbContext.SensorData.FirstOrDefaultAsync();
            Assert.NotNull(savedData);
            Assert.Equal(message.SensorId, savedData.SensorId);
            Assert.Equal(message.Temperature, savedData.Temperature);
            Assert.Equal(message.Humidity, savedData.Humidity);
            Assert.Equal(message.Pressure, savedData.Pressure);
        }

        [Fact]
        public async Task Consume_ValidAirQualityData_ShouldSaveToDatabase()
        {
            // Arrange
            var message = new SensorDataMessage
            {
                SensorId = "airquality-001",
                SensorType = SensorType.AirQuality,
                CO2 = 450.0,
                PM25 = 10.5,
                VOC = 100.0,
                Timestamp = DateTime.UtcNow,
            };
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            // Act
            await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            var savedData = await _dbContext.SensorData.FirstOrDefaultAsync();
            Assert.NotNull(savedData);
            Assert.Equal(message.SensorId, savedData.SensorId);
            Assert.Equal(message.CO2, savedData.CO2);
            Assert.Equal(message.PM25, savedData.PM25);
            Assert.Equal(message.VOC, savedData.VOC);
        }

        [Fact]
        public async Task Consume_InvalidData_ShouldLogErrorAndNotSave()
        {
            // Arrange
            var message = new SensorDataMessage
            {
                SensorId = "environmental-001",
                SensorType = SensorType.Environmental,
                Temperature = -273.16, // Invalid temperature (below absolute zero)
                Humidity = 150.0, // Invalid humidity (over 100%)
                Timestamp = DateTime.UtcNow,
            };
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            // Act
            await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            var savedData = await _dbContext.SensorData.FirstOrDefaultAsync();
            Assert.Null(savedData);
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
        public async Task Consume_DuplicateData_ShouldHandleGracefully()
        {
            // Arrange
            var message = new SensorDataMessage
            {
                SensorId = "environmental-001",
                SensorType = SensorType.Environmental,
                Temperature = 22.5,
                Humidity = 45.0,
                Timestamp = DateTime.UtcNow,
            };
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            // Act - Save the same data twice
            await _consumer.Consume(_consumeContextMock.Object);
            await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            var savedDataCount = await _dbContext.SensorData.CountAsync();
            Assert.Equal(2, savedDataCount); // Should save both records as time-series data
        }

        [Fact]
        public async Task Consume_DatabaseError_ShouldHandleGracefully()
        {
            // Arrange
            var message = new SensorDataMessage
            {
                SensorId = "environmental-001",
                SensorType = SensorType.Environmental,
                Temperature = 22.5,
                Humidity = 45.0,
                Timestamp = DateTime.UtcNow,
            };
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            // Force database error by disposing context
            await _dbContext.DisposeAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await _consumer.Consume(_consumeContextMock.Object)
            );
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
        public async Task Consume_MissingRequiredFields_ShouldLogErrorAndNotSave()
        {
            // Arrange
            var message = new SensorDataMessage
            {
                // Missing SensorId and SensorType
                Temperature = 22.5,
                Timestamp = DateTime.UtcNow,
            };
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            // Act
            await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            var savedData = await _dbContext.SensorData.FirstOrDefaultAsync();
            Assert.Null(savedData);
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
    }
}
