using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MyDeezerStats.Application.DeezerServices;
using MyDeezerStats.Application.ExcelServices;
using MyDeezerStats.Application.Interfaces;
using MyDeezerStats.Application.MongoDbServices;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Repositories;
using MyDeezerStats.Infrastructure.Mongo.Authentification;
using MyDeezerStats.Infrastructure.Mongo.Ecoutes;
using MyDeezerStats.Infrastructure.Mongo.Search;
using MyDeezerStats.Infrastructure.Settings;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configuration hiérarchique
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

var isInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
var mongoHost = isInContainer ? "mongodb" : "localhost";


builder.Configuration["MongoDbSettings:ConnectionString"] =
    builder.Configuration["MongoDbSettings:ConnectionString"]!
          .Replace("localhost", mongoHost);

// Ajouter les services à l'injection de dépendances
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Ajouter la configuration JwtSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Ajouter les autres services
builder.Services.AddScoped<IDeezerService, DeezerService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IListeningService, ListeningService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IAuthService, AuthService>();

//Repository
builder.Services.AddScoped<ISearchRepository, SearchRepository>();
builder.Services.AddScoped<IListeningRepository, ListeningRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<PasswordHasher<User>>();
builder.Services.AddHttpClient();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

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
        policy.WithOrigins("http://localhost:4200", "http://localhost")
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
