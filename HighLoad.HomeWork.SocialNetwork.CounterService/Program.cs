using Asp.Versioning;
using HighLoad.HomeWork.SocialNetwork.CounterService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.CounterService.Options;
using HighLoad.HomeWork.SocialNetwork.CounterService.Repositories;
using HighLoad.HomeWork.SocialNetwork.CounterService.Services;
using MassTransit;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Добавляем API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("x-api-version"));
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Настройка OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithMetrics(b => b
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddPrometheusExporter());

// Настройка Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Counters API", Version = "v1" });
});

// Настройка опций
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

// Настройка Redis кэша
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection("Redis:ConnectionString").Value;
    options.InstanceName = builder.Configuration.GetSection("Redis:InstanceName").Value;
});

// Добавляем сервисы
builder.Services.AddScoped<ICounterRepository, CounterRepository>();
builder.Services.AddScoped<ICounterService, CounterService>();

// Настройка MassTransit c RabbitMQ
var rabbitMqOptions = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>();

builder.Services.AddMassTransit(config =>
{
    // Регистрируем обработчики сообщений
    config.AddConsumer<NewMessageConsumer>();
    config.AddConsumer<MessageReadConsumer>();

    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(rabbitMqOptions!.Host, rabbitMqOptions.VirtualHost, h =>
        {
            h.Username(rabbitMqOptions.Username);
            h.Password(rabbitMqOptions.Password);
        });

        // Регистрируем очереди для наших обработчиков сообщений и настраиваем маршрутизацию
        cfg.ReceiveEndpoint("counter-service-new-message", e =>
        {
            e.ConfigureConsumer<NewMessageConsumer>(ctx);
            
            // Привязываем очередь к обмену сообщениями от DialogService
            e.Bind("message-events", b =>
            {
                b.ExchangeType = "topic";
                b.RoutingKey = "message.new"; // слушаем события новых сообщений
            });
        });

        cfg.ReceiveEndpoint("counter-service-message-read", e =>
        {
            e.ConfigureConsumer<MessageReadConsumer>(ctx);
            
            // Привязываем очередь к обмену сообщениями от DialogService
            e.Bind("message-events", b =>
            {
                b.ExchangeType = "topic";
                b.RoutingKey = "message.read"; // слушаем события прочитанных сообщений
            });
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Counters API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpMetrics();

app.UseMetricServer();

app.UseAuthorization();

app.MapControllers();

app.MapPrometheusScrapingEndpoint();

app.Run();
