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
        private readonly Mock<ILogger<SensorDataController>> _loggerMock;
        private readonly AppDbContext _dbContext;
        private readonly SensorDataController _controller;

        public SensorDataControllerTests()
        {
            _loggerMock = new Mock<ILogger<SensorDataController>>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new AppDbContext(options);
            SeedTestData();
            _controller = new SensorDataController(_dbContext, _loggerMock.Object);
        }

        private void SeedTestData()
        {
            var environmentalData = new List<SensorData>
            {
                new SensorData
                {
                    SensorId = "env-001",
                    SensorType = SensorType.Environmental,
                    Temperature = 22.5,
                    Humidity = 65.0,
                    Pressure = 1013.25,
                    Timestamp = DateTime.UtcNow.AddHours(-1),
                    Processed = true,
                },
                new SensorData
                {
                    SensorId = "env-001",
                    SensorType = SensorType.Environmental,
                    Temperature = 23.1,
                    Humidity = 63.2,
                    Pressure = 1012.8,
                    Timestamp = DateTime.UtcNow.AddHours(-2),
                    Processed = true,
                },
            };

            var airQualityData = new List<SensorData>
            {
                new SensorData
                {
                    SensorId = "air-001",
                    SensorType = SensorType.AirQuality,
                    CO2 = 450.0,
                    VOC = 120.5,
                    PM25 = 12.3,
                    PM10 = 25.7,
                    Timestamp = DateTime.UtcNow.AddHours(-3),
                    Processed = true,
                },
            };

            var waterData = new List<SensorData>
            {
                new SensorData
                {
                    SensorId = "water-001",
                    SensorType = SensorType.Water,
                    PH = 7.2,
                    Turbidity = 0.8,
                    DissolvedOxygen = 8.5,
                    Conductivity = 310.0,
                    Timestamp = DateTime.UtcNow.AddHours(-4),
                    Processed = true,
                },
            };

            _dbContext.SensorData.AddRange(environmentalData);
            _dbContext.SensorData.AddRange(airQualityData);
            _dbContext.SensorData.AddRange(waterData);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetAllSensorData_ReturnsAllData()
        {
            var result = await _controller.GetAllSensorData();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = Assert.IsAssignableFrom<List<SensorData>>(okResult.Value);

            Assert.Equal(4, returnedData.Count);

            for (int i = 0; i < returnedData.Count - 1; i++)
            {
                Assert.True(returnedData[i].Timestamp >= returnedData[i + 1].Timestamp);
            }
        }

        [Fact]
        public async Task GetLatestSensorData_ReturnsLatestData()
        {
            var result = await _controller.GetLatestSensorData();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = Assert.IsAssignableFrom<List<SensorData>>(okResult.Value);

            Assert.True(returnedData.Count <= 10);

            for (int i = 0; i < returnedData.Count - 1; i++)
            {
                Assert.True(returnedData[i].Timestamp >= returnedData[i + 1].Timestamp);
            }
        }

        [Fact]
        public async Task GetSensorDataBySensorId_ValidId_ReturnsData()
        {
            var result = await _controller.GetSensorDataBySensorId("env-001");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = Assert.IsAssignableFrom<List<SensorData>>(okResult.Value);

            Assert.Equal(2, returnedData.Count);
            Assert.All(returnedData, item => Assert.Equal("env-001", item.SensorId));

            for (int i = 0; i < returnedData.Count - 1; i++)
            {
                Assert.True(returnedData[i].Timestamp >= returnedData[i + 1].Timestamp);
            }
        }

        [Fact]
        public async Task GetSensorDataBySensorId_InvalidId_ReturnsNotFound()
        {
            var result = await _controller.GetSensorDataBySensorId("invalid-sensor");

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No data found for sensor ID: invalid-sensor", notFoundResult.Value);
        }

        [Fact]
        public async Task GetSensorDataBySensorType_ValidType_ReturnsData()
        {
            var result = await _controller.GetSensorDataBySensorType(SensorType.Environmental);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = Assert.IsAssignableFrom<List<SensorData>>(okResult.Value);

            Assert.Equal(2, returnedData.Count);
            Assert.All(
                returnedData,
                item => Assert.Equal(SensorType.Environmental, item.SensorType)
            );
        }

        [Fact]
        public async Task GetSensorDataBySensorType_NoDataForType_ReturnsNotFound()
        {
            var result = await _controller.GetSensorDataBySensorType(SensorType.Motion);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No data found for sensor type: Motion", notFoundResult.Value);
        }

        [Fact]
        public async Task GetSensorDataSummary_NoFilter_ReturnsPaginatedSummary()
        {
            var result = await _controller.GetSensorDataSummary(null, 1, 2);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var resultObj = okResult.Value;
            var resultType = resultObj.GetType();

            var totalCountProp = resultType.GetProperty("TotalCount");
            var totalPagesProp = resultType.GetProperty("TotalPages");
            var currentPageProp = resultType.GetProperty("CurrentPage");
            var pageSizeProp = resultType.GetProperty("PageSize");
            var dataProp = resultType.GetProperty("Data");

            Assert.NotNull(totalCountProp);
            Assert.NotNull(totalPagesProp);
            Assert.NotNull(currentPageProp);
            Assert.NotNull(pageSizeProp);
            Assert.NotNull(dataProp);

            Assert.Equal(3, totalCountProp.GetValue(resultObj));
            Assert.Equal(2, totalPagesProp.GetValue(resultObj));
            Assert.Equal(1, currentPageProp.GetValue(resultObj));
            Assert.Equal(2, pageSizeProp.GetValue(resultObj));

            var data = dataProp.GetValue(resultObj) as System.Collections.IEnumerable;
            Assert.NotNull(data);

            int count = 0;
            foreach (var item in data)
            {
                count++;
            }
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetSensorDataSummary_WithTypeFilter_ReturnsFilteredData()
        {
            var result = await _controller.GetSensorDataSummary(SensorType.Environmental);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var resultObj = okResult.Value;
            var resultType = resultObj.GetType();

            var totalCountProp = resultType.GetProperty("TotalCount");
            var dataProp = resultType.GetProperty("Data");

            Assert.NotNull(totalCountProp);
            Assert.NotNull(dataProp);

            Assert.Equal(1, totalCountProp.GetValue(resultObj));

            var data = dataProp.GetValue(resultObj) as System.Collections.IEnumerable;
            Assert.NotNull(data);

            var items = data.Cast<object>().ToList();
            Assert.NotEmpty(items);

            var firstItem = items.First();
            Assert.NotNull(firstItem);

            var itemType = firstItem.GetType();
            var sensorTypeProp = itemType.GetProperty("SensorType");
            Assert.NotNull(sensorTypeProp);

            var sensorTypeValue = sensorTypeProp.GetValue(firstItem);
            Assert.NotNull(sensorTypeValue);
            Assert.Equal(SensorType.Environmental, sensorTypeValue);
        }

        [Fact]
        public async Task GetAllSensorData_WithDatabaseError_Returns500()
        {
            await _dbContext.DisposeAsync();

            var result = await _controller.GetAllSensorData();

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

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
