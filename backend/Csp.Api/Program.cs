using MySql.Data.MySqlClient;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for local React dev server
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();

// Example minimal ADO.NET usage in a test endpoint
app.MapGet("/health/db", async () =>
{
    await using var conn = new MySqlConnection(builder.Configuration.GetConnectionString("Default"));
    await conn.OpenAsync();
    await using var cmd = new MySqlCommand("SELECT 1", conn);
    var result = await cmd.ExecuteScalarAsync();
    return Results.Ok(new { db = result });
});

app.Run();