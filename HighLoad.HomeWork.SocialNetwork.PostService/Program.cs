using System.Text;
using HighLoad.HomeWork.SocialNetwork.PostService.Clients;
using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.PostService.Middlewares;
using HighLoad.HomeWork.SocialNetwork.PostService.Options;
using HighLoad.HomeWork.SocialNetwork.PostService.Repositories;
using HighLoad.HomeWork.SocialNetwork.PostService.Services;
using HighLoad.HomeWork.SocialNetwork.PostService.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Refit;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFeedCacheService, FeedCacheService>();

builder.Services.AddSingleton<IWebsocketConnectionManager, InMemoryWebsocketConnectionManager>();

builder.Services.AddHostedService<WebsocketBroadcastBackgroundService>();

var userServiceUrl = builder.Configuration["ServiceUrls:UserServiceUrl"];
if (string.IsNullOrEmpty(userServiceUrl))
{
    throw new InvalidOperationException("UserServiceUrl is not configured in appsettings.json (ServiceUrls:UserServiceUrl).");
}

builder.Services.AddScoped<AuthHeaderHandler>();

builder.Services.AddRefitClient<IUserClient>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri(userServiceUrl);
    })
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<DbOptions>(builder.Configuration.GetSection("ConnectionStringsDatabases"));

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMQ")
);

builder.Services.AddMemoryCache();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PostService API",
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

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();
app.UseMiddleware<WebSocketMiddleware>();

app.MapControllers();

app.Run();
