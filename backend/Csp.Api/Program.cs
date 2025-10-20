using MySql.Data.MySqlClient;
using Microsoft.OpenApi.Models;
using Csp.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;

// Load environment variables from .env file (Development only)
// In production (Azure), environment variables are set through Azure Configuration
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

if (isDevelopment)
{
    try
    {
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (File.Exists(envPath))
        {
            Env.Load(envPath);
            Console.WriteLine($"[DEV] Loaded environment variables from: {envPath}");
        }
        else
        {
            Console.WriteLine($"[DEV] .env file not found at: {envPath} - using system environment variables");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DEV] Warning: Could not load .env file: {ex.Message}");
        Console.WriteLine("[DEV] Continuing with system environment variables...");
    }
}
else
{
    Console.WriteLine($"[{environment}] Using Azure/system environment variables (no .env file loading)");
}

var builder = WebApplication.CreateBuilder(args);

// Add environment variable configuration source to replace placeholders
builder.Configuration.AddEnvironmentVariables();

// Manually expand environment variables in connection string for development
if (isDevelopment)
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connectionString))
    {
        // Fetch environment variables once
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "lector-library";
        var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
        // DB_PASSWORD may be empty or unset. Use null-coalescing to get empty string when absent.
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

        // Replace placeholders with actual environment variable values. Only call Replace for DB_PASSWORD
        // when a placeholder exists (to avoid calling String.Replace with an empty oldValue).
        connectionString = connectionString
            .Replace("{DB_HOST}", dbHost)
            .Replace("{DB_PORT}", dbPort)
            .Replace("{DB_NAME}", dbName)
            .Replace("{DB_USER}", dbUser);

        if (connectionString.Contains("{DB_PASSWORD}"))
        {
            // If dbPassword is null, replace with empty string (no password). If it's non-null, use its value.
            connectionString = connectionString.Replace("{DB_PASSWORD}", dbPassword ?? "");
        }

        // Update the configuration with the expanded connection string
        builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

        // Log the expanded connection string but never print the actual password. If password is empty, show '(empty)'
        var sanitized = connectionString;
        if (dbPassword != null && dbPassword.Length > 0)
        {
            sanitized = sanitized.Replace(dbPassword, "***");
        }
        else if (connectionString.Contains("password=") || connectionString.Contains("Password="))
        {
            // Optional: mask empty password in logs to avoid confusion
            sanitized = sanitized.Replace("Password=;", "Password=(empty);").Replace("password=;", "password=(empty);");
        }

        Console.WriteLine($"[DEV] Expanded connection string: {sanitized}");
    }
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ILendingService, LendingService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IFineCalculationService, FineCalculationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddHostedService<FineCalculationHostedService>();

var jwtValidation = new JwtTokenService(builder.Configuration).GetValidationParameters();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = jwtValidation;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("RequireLibrarian", policy => policy.RequireRole("Librarian", "Administrator"));
});

// CORS for production and local React dev server
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ??
    new[] { "http://localhost:5173", "http://localhost:3000", "https://lms-cyf6d5f2fqhvf7b3.southindia-01.azurewebsites.net" };

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(allowedOrigins)
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

// Configure static files for frontend
app.UseStaticFiles();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    await userService.InitializeDatabaseAsync();

    var lendingService = scope.ServiceProvider.GetRequiredService<ILendingService>();
    await lendingService.InitializeLendingTablesAsync();

    var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();
    await reservationService.InitializeReservationTablesAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Example minimal ADO.NET usage in a test endpoint
app.MapGet("/api/health/db", async () =>
{
    try
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            return Results.Problem("Connection string not found");
        }

        await using var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new MySqlCommand("SELECT 1", conn);
        var result = await cmd.ExecuteScalarAsync();
        return Results.Ok(new { db = result, connectionString = connectionString });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});

// Fallback route for SPA - only for non-API routes
app.MapFallback(async (HttpContext context) =>
{
    // Don't handle API requests with fallback
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = 404;
        return;
    }

    // Serve index.html for all other routes (SPA routing)
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();

// The partial Program class is declared here to enable integration testing with WebApplicationFactory in test projects.
public partial class Program { }