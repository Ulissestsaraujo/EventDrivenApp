using EventProducer;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Data;
using Shared.Messages;

namespace IntegrationTests
{
    public class WorkerMessagePublishingTests
    {
        [Fact]
        public async Task Worker_GeneratesAndPublishesSensorData_MessageIsPublishedCorrectly()
        {
            await using var provider = new ServiceCollection()
                .AddDbContext<AppDbContext>(
                    options => options.UseInMemoryDatabase("worker_publishing_test"),
                    ServiceLifetime.Singleton
                )
                .AddMassTransitTestHarness(cfg => { })
                .AddLogging()
                .AddSingleton<IServiceScopeFactory>(sp => new TestServiceScopeFactory(sp))
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();

            var logger = provider.GetRequiredService<ILogger<Worker>>();
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            var worker = new Worker(logger, harness.Bus, scopeFactory);

            await harness.Start();

            try
            {
                var sensorData = worker.GenerateSensorData();
                var message = worker.MapToMessage(sensorData);

                await harness.Bus.Publish(message);

                Assert.True(await harness.Published.Any<SensorDataMessage>());

                var publishedMessage = await harness
                    .Published.SelectAsync<SensorDataMessage>()
                    .FirstOrDefault();

                Assert.NotNull(publishedMessage);
                var messageObject = publishedMessage.Context.Message;
                Assert.Equal(sensorData.SensorId, messageObject.SensorId);
                Assert.Equal(sensorData.SensorType, messageObject.SensorType);
                Assert.Equal(sensorData.Temperature, messageObject.Temperature);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Fact]
        public async Task Worker_SavesDataToDatabase_DataIsPersisted()
        {
            await using var provider = new ServiceCollection()
                .AddDbContext<AppDbContext>(
                    options => options.UseInMemoryDatabase("worker_database_test"),
                    ServiceLifetime.Singleton
                )
                .AddMassTransitTestHarness(cfg => { })
                .AddLogging()
                .AddSingleton<IServiceScopeFactory>(sp => new TestServiceScopeFactory(sp))
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            var logger = provider.GetRequiredService<ILogger<Worker>>();
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            var worker = new Worker(logger, harness.Bus, scopeFactory);

            await harness.Start();

            try
            {
                var sensorData = worker.GenerateSensorData();
                await worker.SaveSensorDataAsync(sensorData);

                var dbContext = provider.GetRequiredService<AppDbContext>();
                var savedData = await dbContext.SensorData.FirstOrDefaultAsync(d =>
                    d.SensorId == sensorData.SensorId && d.Timestamp == sensorData.Timestamp
                );

                Assert.NotNull(savedData);
                Assert.Equal(sensorData.SensorId, savedData.SensorId);
                Assert.Equal(sensorData.SensorType, savedData.SensorType);
                Assert.Equal(sensorData.Temperature, savedData.Temperature);
                Assert.Equal(sensorData.Humidity, savedData.Humidity);
                Assert.Equal(sensorData.Pressure, savedData.Pressure);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Fact]
        public async Task Worker_CompleteFlow_GeneratesPublishesAndConsumes()
        {
            await using var provider = new ServiceCollection()
                .AddDbContext<AppDbContext>(
                    options => options.UseInMemoryDatabase("complete_flow_test"),
                    ServiceLifetime.Singleton
                )
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<EventConsumer.Consumers.SensorDataConsumer>();
                })
                .AddSingleton<IServiceScopeFactory>(sp => new TestServiceScopeFactory(sp))
                .AddLogging()
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            var logger = provider.GetRequiredService<ILogger<Worker>>();
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            var worker = new Worker(logger, harness.Bus, scopeFactory);

            await harness.Start();

            try
            {
                var dbContext = provider.GetRequiredService<AppDbContext>();
                await dbContext.Database.EnsureCreatedAsync();

                var sensorData = worker.GenerateSensorData();
                await worker.SaveSensorDataAsync(sensorData);
                await harness.Bus.Publish(worker.MapToMessage(sensorData));

                Assert.True(await harness.Published.Any<SensorDataMessage>());

                Assert.True(await harness.Consumed.Any<SensorDataMessage>());
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}
