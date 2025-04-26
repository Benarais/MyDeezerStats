using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MyDeezerStats.Application.ExcelServices;
using MyDeezerStats.Application.Interfaces;
using MyDeezerStats.Application.MongoDbServices;
using MyDeezerStats.Domain.Repositories;
using MyDeezerStats.Infrastructure.Mongo;
using MyDeezerStats.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

// Charger les configurations de l'application
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Ajouter les services à l'injection de dépendances
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Ajouter la configuration JwtSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Ajouter les autres services
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IListeningRepository, ListeningRepository>();
builder.Services.AddScoped<IListeningService, ListeningService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddControllers();

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    if (settings == null)
    {
        throw new InvalidOperationException("MongoDbSettings configuration is missing or invalid.");
    }

    var client = new MongoClient(settings.ConnectionString);
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024;
});

// Configuration de l'authentification JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"], 
            ValidAudience = builder.Configuration["Jwt:Audience"],  
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)) 
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configurer CORS
app.UseCors("AllowLocalhost");

// Log des requêtes entrantes
app.Use(async (context, next) =>
{
    Console.WriteLine($"Incoming request: {context.Request.Method} {context.Request.Path}");
    await next.Invoke();
});

// Authentification et autorisation
app.UseAuthentication();
app.UseAuthorization();

// Mappe les contrôleurs
app.MapControllers();

// Lance l'application
app.Run();
