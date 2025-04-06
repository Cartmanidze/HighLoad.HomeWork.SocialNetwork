using System.Text;
using HighLoad.HomeWork.SocialNetwork.DialogService.Clients;
using HighLoad.HomeWork.SocialNetwork.DialogService.Events;
using HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.DialogService.Middlewares;
using HighLoad.HomeWork.SocialNetwork.DialogService.Options;
using HighLoad.HomeWork.SocialNetwork.DialogService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using Prometheus;
using Refit;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddConfiguration(builder.Configuration.GetSection("Logging")).AddConsole().AddDebug();

// var redisConnString = builder.Configuration.GetConnectionString("Redis");
//
// if (string.IsNullOrWhiteSpace(redisConnString))
// {
//     redisConnString = "localhost:6379";
// }

var jwtSettings = builder.Configuration.GetSection("Jwt");

var citusConnection = builder.Configuration.GetConnectionString("CitusDb")!;

// Настройка опций RabbitMQ
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

// Настройка MassTransit
var rabbitMqOptions = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>();
builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(rabbitMqOptions.Host, rabbitMqOptions.VirtualHost, h =>
        {
            h.Username(rabbitMqOptions.Username);
            h.Password(rabbitMqOptions.Password);
        });
        
        cfg.Message<NewMessageEvent>(x => 
        {
            x.SetEntityName("message-events");
        });
        
        cfg.Message<MessageReadEvent>(x => 
        {
            x.SetEntityName("message-events"); 

        });
        
        // Объявляем точку обмена message-events с типом topic
        cfg.Publish<NewMessageEvent>(x =>
        {
            x.ExchangeType = "topic";
        });
        
        cfg.Publish<MessageReadEvent>(x =>
        {
            x.ExchangeType = "topic";
        });
    });
});

// Регистрируем сервис публикации сообщений через MassTransit
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

// Регистрируем сервис для работы с диалогами
builder.Services.AddScoped<IDialogService>(provider => 
    new DialogService(
        citusConnection, 
        provider.GetRequiredService<IEventPublisher>(),
        provider.GetRequiredService<ILogger<DialogService>>()
    )
);

// builder.Services.AddSingleton<IDialogService, DialogServiceRedisUdf>();
//
// builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnString));

var userServiceUrl = builder.Configuration["ServiceUrls:UserServiceUrl"];
if (string.IsNullOrEmpty(userServiceUrl))
{
    throw new InvalidOperationException("UserServiceUrl is not configured in appsettings.json (ServiceUrls:UserServiceUrl).");
}

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<AuthHeaderHandler>();

builder.Services.AddRefitClient<IUserClient>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri(userServiceUrl);
    })
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped<IUserValidationService, UserValidationService>();

builder.Services.AddControllers();

builder.Services.AddOpenTelemetry()
    .WithMetrics(b => b
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddPrometheusExporter());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DialogService API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите 'Bearer' [пробел] и ваш токен JWT."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = key
        };
    });


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpMetrics();

app.UseMetricServer();

app.UseMiddleware<RequestIdMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapPrometheusScrapingEndpoint();

app.Run();