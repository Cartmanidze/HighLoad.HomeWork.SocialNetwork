using System.Text;
using HighLoad.HomeWork.SocialNetwork.Data;
using HighLoad.HomeWork.SocialNetwork.Interfaces;
using HighLoad.HomeWork.SocialNetwork.Middlewares;
using HighLoad.HomeWork.SocialNetwork.Options;
using HighLoad.HomeWork.SocialNetwork.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Сервисы
builder.Services.AddControllers();

builder.Services.AddOpenTelemetry()
    .WithMetrics(mb =>
    {
        mb.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddPrometheusExporter();
    });


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Social Network API", Version = "v1" });
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
                Reference = new OpenApiReference{ Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Регистрация опций репликации
builder.Services.Configure<DbReplicationOptions>(builder.Configuration.GetSection("Replication"));

// Регистрация зависимостей
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<ITransactionState, HttpContextTransactionState>();
builder.Services.AddTransient<IDbConnectionFactory, ReplicationRoutingDataSource>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasherService>();

// Настройка аутентификации JWT
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

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Social Network API V1");
        c.RoutePrefix = "swagger";
    });
}

// Middleware для определения read-only запросов
app.UseMiddleware<ReadOnlyRoutingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Включаем сбор метрик HTTP запросов
app.UseHttpMetrics();

app.UseMetricServer();

app.MapControllers();

app.MapPrometheusScrapingEndpoint();

app.Run();
