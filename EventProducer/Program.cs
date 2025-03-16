using EventProducer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Data;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq(
        (context, cfg) =>
        {
            var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
            var host = rabbitConfig["Host"] ?? "rabbitmq";
            var username = rabbitConfig["Username"] ?? "guest";
            var password = rabbitConfig["Password"] ?? "guest";

            cfg.Host(
                host,
                "/",
                h =>
                {
                    h.Username(username);
                    h.Password(password);
                }
            );

            cfg.ConfigureEndpoints(context);
        }
    );
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=producer.db"
    )
);

var host = builder.Build();
host.Run();

public partial class Program { }
