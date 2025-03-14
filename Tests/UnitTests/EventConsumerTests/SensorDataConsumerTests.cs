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
        }

        [Fact]
        public async Task Consume_ShouldProcessMessageAndSaveToDatabase()
        {
            // Arrange
            var message = new SensorDataMessage
            {
                SensorId = "sensor-test",
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

            // Verify error was logged
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => true),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Once
            );

            // Verify nothing was saved
            var savedData = await _dbContext.SensorData.ToListAsync();
            Assert.Empty(savedData);
        }
    }
}
