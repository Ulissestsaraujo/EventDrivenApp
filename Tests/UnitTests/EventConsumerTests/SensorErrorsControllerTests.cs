using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventConsumer.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Data;
using Shared.Models;
using Xunit;

namespace UnitTests.EventConsumerTests
{
    public class SensorErrorsControllerTests
    {
        private readonly Mock<ILogger<SensorErrorsController>> _loggerMock;
        private readonly AppDbContext _dbContext;
        private readonly SensorErrorsController _controller;

        public SensorErrorsControllerTests()
        {
            _loggerMock = new Mock<ILogger<SensorErrorsController>>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new AppDbContext(options);
            SeedTestData();
            _controller = new SensorErrorsController(_dbContext, _loggerMock.Object);
        }

        private void SeedTestData()
        {
            var testData = new List<SensorError>
            {
                new SensorError
                {
                    SensorId = "env-001",
                    SensorType = SensorType.Environmental,
                    ErrorTimestamp = DateTime.UtcNow.AddHours(-2),
                    ErrorMessage = "Temperature reading out of range",
                    ErrorCount = 5,
                },
                new SensorError
                {
                    SensorId = "air-001",
                    SensorType = SensorType.AirQuality,
                    ErrorTimestamp = DateTime.UtcNow.AddHours(-1),
                    ErrorMessage = "CO2 sensor malfunction",
                    ErrorCount = 3,
                },
                new SensorError
                {
                    SensorId = "water-001",
                    SensorType = SensorType.Water,
                    ErrorTimestamp = DateTime.UtcNow.AddHours(-3),
                    ErrorMessage = "Invalid pH reading",
                    ErrorCount = 7,
                },
                new SensorError
                {
                    SensorId = "motion-001",
                    SensorType = SensorType.Motion,
                    ErrorTimestamp = DateTime.UtcNow.AddMinutes(-30),
                    ErrorMessage = "Accelerometer calibration error",
                    ErrorCount = 1,
                },
            };

            _dbContext.SensorErrors.AddRange(testData);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetSensorErrors_ReturnsTop3OrderedByErrorCount()
        {
            var result = await _controller.GetSensorErrors();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedErrors = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);

            Assert.Equal(3, returnedErrors.Count());

            var errorsList = returnedErrors.ToList();

            var firstItem = errorsList[0];
            var errorCountProp = firstItem.GetType().GetProperty("errorCount");
            var sensorIdProp = firstItem.GetType().GetProperty("sensorId");

            Assert.NotNull(errorCountProp);
            Assert.NotNull(sensorIdProp);

            int firstErrorCount = (int)(errorCountProp?.GetValue(firstItem) ?? 0);
            string? firstSensorId = (string?)(sensorIdProp?.GetValue(firstItem));

            Assert.Equal(7, firstErrorCount);
            Assert.Equal("water-001", firstSensorId);

            for (int i = 0; i < errorsList.Count - 1; i++)
            {
                int currentCount = (int)(errorCountProp?.GetValue(errorsList[i]) ?? 0);
                int nextCount = (int)(errorCountProp?.GetValue(errorsList[i + 1]) ?? 0);
                Assert.True(currentCount >= nextCount);
            }
        }

        [Fact]
        public async Task GetSensorErrors_WithNoData_ReturnsEmptyList()
        {
            _dbContext.SensorErrors.RemoveRange(_dbContext.SensorErrors);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.GetSensorErrors();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedErrors = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Empty(returnedErrors);
        }

        [Fact]
        public async Task GetSensorErrors_WithDatabaseError_Returns500()
        {
            await _dbContext.DisposeAsync();

            var result = await _controller.GetSensorErrors();

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            _loggerMock.Verify(
                x =>
                    x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => true),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetSensorErrors_ReturnsCorrectErrorProperties()
        {
            var result = await _controller.GetSensorErrors();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedErrors = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);

            var firstError = returnedErrors.First();
            var type = firstError.GetType();

            Assert.NotNull(type.GetProperty("sensorId"));
            Assert.NotNull(type.GetProperty("sensorType"));
            Assert.NotNull(type.GetProperty("errorCount"));
            Assert.NotNull(type.GetProperty("lastErrorTimestamp"));
            Assert.NotNull(type.GetProperty("lastErrorMessage"));

            var sensorIdProp = type.GetProperty("sensorId");
            var sensorTypeProp = type.GetProperty("sensorType");
            var errorCountProp = type.GetProperty("errorCount");
            var timestampProp = type.GetProperty("lastErrorTimestamp");
            var messageProp = type.GetProperty("lastErrorMessage");

            Assert.NotNull(sensorIdProp);
            Assert.NotNull(sensorTypeProp);
            Assert.NotNull(errorCountProp);
            Assert.NotNull(timestampProp);
            Assert.NotNull(messageProp);

            Assert.Equal("water-001", sensorIdProp?.GetValue(firstError));
            Assert.Equal(SensorType.Water, sensorTypeProp?.GetValue(firstError));
            Assert.Equal(7, errorCountProp?.GetValue(firstError));
            Assert.NotNull(timestampProp?.GetValue(firstError));
            Assert.Equal("Invalid pH reading", messageProp?.GetValue(firstError));
        }
    }
}
