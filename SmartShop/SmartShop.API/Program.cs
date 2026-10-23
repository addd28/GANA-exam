using Microsoft.EntityFrameworkCore;
using SmartShop.API.Data;
using SmartShop.API.Interfaces;
using SmartShop.API.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=SMARTSHOP;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<SmartShopDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// 2. HTTP Client and Gemini Service
builder.Services.AddHttpClient<IGeminiService, GeminiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

// 3. CORS Configuration
const string corsPolicyName = "AllowSmartShopClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 4. Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

var app = builder.Build();

// 5. Middleware Pipeline
app.UseCors(corsPolicyName);

app.UseRouting();

app.MapControllers();

// Health check endpoint for testing environment readiness
app.MapGet("/", () => Results.Ok(new
{
    service = "SmartShop API",
    status = "Online",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

app.Run();
