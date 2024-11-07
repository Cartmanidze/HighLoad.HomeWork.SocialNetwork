using HighLoad.HomeWork.SocialNetwork.Interfaces;
using HighLoad.HomeWork.SocialNetwork.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов в контейнер
builder.Services.AddControllers();

// Регистрация пользовательских сервисов
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();

var app = builder.Build();

// Конвейер обработки запросов
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();