using EventConsumer;
using EventConsumer.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SensorDataConsumer>();

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

            // Configure the consumer endpoint
            cfg.ReceiveEndpoint(
                "sensor-data-queue",
                e =>
                {
                    e.PrefetchCount = 32;
                    e.ConcurrentMessageLimit = 8;
                    e.UseMessageRetry(r => r.Interval(3, 1000));
                    e.ConfigureConsumer<SensorDataConsumer>(context);
                }
            );

            cfg.ConfigureEndpoints(context);
        }
    );
});

// Configure SQLite database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=consumer.db"
    )
);

// Configure CORS to allow requests from the React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReactApp",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:80",
                    "http://localhost:3000",
                    "http://localhost"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

app.Run();
