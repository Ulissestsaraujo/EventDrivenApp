using System;
using System.Linq;
using System.Threading.Tasks;
using EventConsumer.Consumers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Data;
using Shared.Messages;
using Shared.Models;
using Xunit;

namespace IntegrationTests
{
    public class MessageFlowTests
    {
        [Fact]
        public async Task SensorData_Published_IsConsumedAndStoredCorrectly()
        {
            await using var provider = new ServiceCollection()
                .AddDbContext<AppDbContext>(
                    options => options.UseInMemoryDatabase("message_flow_test"),
                    ServiceLifetime.Singleton
                )
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<SensorDataConsumer>();
                })
                .AddSingleton<IServiceScopeFactory>(sp => new TestServiceScopeFactory(sp))
                .AddLogging()
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();

            await harness.Start();

            try
            {
                var message = new SensorDataMessage
                {
                    SensorId = "env-001",
                    SensorType = SensorType.Environmental,
                    Temperature = 22.5,
                    Humidity = 45.0,
                    Pressure = 1013.25,
                    Timestamp = DateTime.UtcNow,
                };

                await harness.Bus.Publish(message);

                Assert.True(
                    await harness.Consumed.Any<SensorDataMessage>(),
                    "Message was not consumed"
                );

                var consumeContext = await harness
                    .Consumed.SelectAsync<SensorDataMessage>()
                    .FirstOrDefault();
                Assert.NotNull(consumeContext);

                var messageObject = consumeContext.Context.Message;
                Assert.Equal("env-001", messageObject.SensorId);

                var dbContext = provider.GetRequiredService<AppDbContext>();
                var savedData = await dbContext.SensorData.FirstOrDefaultAsync(d =>
                    d.SensorId == "env-001"
                );

                Assert.NotNull(savedData);
                Assert.Equal(message.Temperature, savedData.Temperature);
                Assert.Equal(message.Humidity, savedData.Humidity);
                Assert.Equal(message.Pressure, savedData.Pressure);
                Assert.True(savedData.Processed);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Fact]
        public async Task Consumer_WhenReceivingInvalidMessage_LogsErrorAndDoesNotSaveData()
        {
            await using var provider = new ServiceCollection()
                .AddDbContext<AppDbContext>(
                    options => options.UseInMemoryDatabase("invalid_message_test"),
                    ServiceLifetime.Singleton
                )
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<SensorDataConsumer>();
                })
                .AddSingleton<IServiceScopeFactory>(sp => new TestServiceScopeFactory(sp))
                .AddLogging()
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();

            var consumerHarness = harness.GetConsumerHarness<SensorDataConsumer>();

            await harness.Start();

            try
            {
                var message = new SensorDataMessage
                {
                    SensorType = SensorType.Environmental,
                    Temperature = 22.5,
                    Timestamp = DateTime.UtcNow,
                };

                await harness.Bus.Publish(message);

                Assert.True(
                    await harness.Consumed.Any<SensorDataMessage>(),
                    "Message was not consumed"
                );

                var dbContext = provider.GetRequiredService<AppDbContext>();
                Assert.Empty(await dbContext.SensorData.ToListAsync());
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}
