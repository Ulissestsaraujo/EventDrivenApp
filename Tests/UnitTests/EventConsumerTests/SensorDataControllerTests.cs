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
    public class SensorDataControllerTests
    {
        private readonly AppDbContext _dbContext;
        private readonly SensorDataController _controller;
        private readonly Mock<ILogger<SensorDataController>> _loggerMock;

        public SensorDataControllerTests()
        {
            // Set up in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new AppDbContext(options);
            _loggerMock = new Mock<ILogger<SensorDataController>>();
            _controller = new SensorDataController(_dbContext, _loggerMock.Object);

            // Seed the database with test data
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var testData = new List<SensorData>
            {
                new SensorData
                {
                    SensorId = "sensor-001",
                    Temperature = 22.5,
                    Humidity = 45.2,
                    Pressure = 1013.2,
                    Timestamp = DateTime.UtcNow.AddMinutes(-30),
                    Processed = true,
                },
                new SensorData
                {
                    SensorId = "sensor-001",
                    Temperature = 23.1,
                    Humidity = 46.5,
                    Pressure = 1012.8,
                    Timestamp = DateTime.UtcNow.AddMinutes(-20),
                    Processed = true,
                },
                new SensorData
                {
                    SensorId = "sensor-002",
                    Temperature = 18.9,
                    Humidity = 65.3,
                    Pressure = 1014.1,
                    Timestamp = DateTime.UtcNow.AddMinutes(-15),
                    Processed = true,
                },
                new SensorData
                {
                    SensorId = "sensor-003",
                    Temperature = 25.7,
                    Humidity = 30.2,
                    Pressure = 1010.5,
                    Timestamp = DateTime.UtcNow.AddMinutes(-10),
                    Processed = true,
                },
                new SensorData
                {
                    SensorId = "sensor-002",
                    Temperature = 19.2,
                    Humidity = 64.8,
                    Pressure = 1014.3,
                    Timestamp = DateTime.UtcNow.AddMinutes(-5),
                    Processed = true,
                },
            };

            _dbContext.SensorData.AddRange(testData);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetLatestSensorData_ShouldReturn10LatestEntries()
        {
            // Act
            var result = await _controller.GetLatestSensorData();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<SensorData>>(okResult.Value);
            Assert.Equal(5, returnValue.Count); // We only have 5 total entries

            // Verify ordering - should be most recent first
            var sortedData = returnValue.OrderByDescending(d => d.Timestamp).ToList();
            for (int i = 0; i < returnValue.Count; i++)
            {
                Assert.Equal(sortedData[i].Id, returnValue[i].Id);
            }
        }

        [Fact]
        public async Task GetSensorDataBySensorId_WithValidId_ShouldReturnFilteredData()
        {
            // Arrange
            string sensorId = "sensor-001";

            // Act
            var result = await _controller.GetSensorDataBySensorId(sensorId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<SensorData>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.All(returnValue, item => Assert.Equal(sensorId, item.SensorId));
        }

        [Fact]
        public async Task GetSensorDataBySensorId_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            string sensorId = "sensor-999"; // Non-existent ID

            // Act
            var result = await _controller.GetSensorDataBySensorId(sensorId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetSensorDataSummary_ShouldReturnSummaryForEachSensor()
        {
            // Act
            var result = await _controller.GetSensorDataSummary();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Equal(3, returnValue.Count()); // We have 3 distinct sensors

            // Verify each sensor has a summary
            var resultList = returnValue.ToList();
            var sensorIds = new[] { "sensor-001", "sensor-002", "sensor-003" };

            foreach (var sensorId in sensorIds)
            {
                var hasSensor = resultList.Any(s =>
                {
                    var dict = s.GetType()
                        .GetProperties()
                        .ToDictionary(p => p.Name, p => p.GetValue(s));
                    return dict["SensorId"].ToString() == sensorId;
                });

                Assert.True(hasSensor, $"Summary for sensor {sensorId} not found");
            }
        }
    }
}
