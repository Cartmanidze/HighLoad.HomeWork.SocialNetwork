
using HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.DialogService.Repositories;

var builder = WebApplication.CreateBuilder(args);

var citusConnection = builder.Configuration.GetConnectionString("CitusDb")
                      ?? Environment.GetEnvironmentVariable("ConnectionStrings__CitusDb")
                      ?? "Host=localhost;Port=5432;Database=citusdb;Username=postgres;Password=postgres";

builder.Services.AddSingleton<IDialogService>(_ => new DialogService(citusConnection));

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();