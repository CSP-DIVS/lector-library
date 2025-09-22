using MySql.Data.MySqlClient;
using Microsoft.OpenApi.Models;
using Csp.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

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

public partial class Program { }