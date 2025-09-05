using Csp.Api.Models;
using Csp.Api.DTOs;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Csp.Api.Services
{
    public interface IUserService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> RegisterAsync(RegisterRequest request);
        Task InitializeDatabaseAsync();
    }

    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
                              throw new InvalidOperationException("Connection string not found");
        }

        public async Task InitializeDatabaseAsync()
        {
            var maxRetries = 10;
            var delay = TimeSpan.FromSeconds(5);

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await using var conn = new MySqlConnection(_connectionString);
                    await conn.OpenAsync();

                    // Create users table if it doesn't exist
                    var createTableSql = @"
                        CREATE TABLE IF NOT EXISTS users (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            Username VARCHAR(50) UNIQUE NOT NULL,
                            Email VARCHAR(100) UNIQUE NOT NULL,
                            PasswordHash VARCHAR(255) NOT NULL,
                            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                        )";

                    await using var cmd = new MySqlCommand(createTableSql, conn);
                    await cmd.ExecuteNonQueryAsync();

                    // Create a default test user if none exists
                    var checkUserSql = "SELECT COUNT(*) FROM users WHERE Username = 'testuser'";
                    await using var checkCmd = new MySqlCommand(checkUserSql, conn);
                    var userCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                    if (userCount == 0)
                    {
                        var testPasswordHash = HashPassword("password123");
                        var insertUserSql = @"
                            INSERT INTO users (Username, Email, PasswordHash) 
                            VALUES ('testuser', 'test@example.com', @PasswordHash)";
                        
                        await using var insertCmd = new MySqlCommand(insertUserSql, conn);
                        insertCmd.Parameters.AddWithValue("@PasswordHash", testPasswordHash);
                        await insertCmd.ExecuteNonQueryAsync();
                    }

                    Console.WriteLine("Database initialized successfully!");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database initialization attempt {i + 1} failed: {ex.Message}");
                    if (i < maxRetries - 1)
                    {
                        Console.WriteLine($"Retrying in {delay.TotalSeconds} seconds...");
                        await Task.Delay(delay);
                    }
                    else
                    {
                        Console.WriteLine("Max retries reached. Database initialization failed.");
                        throw;
                    }
                }
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT Id, Username, Email, PasswordHash FROM users WHERE Username = @Username";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", request.Username);

            await using var reader = await cmd.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                var storedHash = reader["PasswordHash"]?.ToString();
                if (!string.IsNullOrEmpty(storedHash) && VerifyPassword(request.Password, storedHash))
                {
                    return new LoginResponse
                    {
                        Success = true,
                        Message = "Login successful",
                        User = new UserDto
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Username = reader["Username"]?.ToString() ?? "",
                            Email = reader["Email"]?.ToString() ?? ""
                        },
                        Token = "simple-jwt-token-" + Convert.ToInt32(reader["Id"]) // Simple token for testing
                    };
                }
            }

            return new LoginResponse
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Check if user already exists
            var checkSql = "SELECT COUNT(*) FROM users WHERE Username = @Username OR Email = @Email";
            await using var checkCmd = new MySqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@Username", request.Username);
            checkCmd.Parameters.AddWithValue("@Email", request.Email);
            
            var existingCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
            if (existingCount > 0)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Username or email already exists"
                };
            }

            // Insert new user
            var insertSql = @"
                INSERT INTO users (Username, Email, PasswordHash) 
                VALUES (@Username, @Email, @PasswordHash);
                SELECT LAST_INSERT_ID();";
            
            await using var insertCmd = new MySqlCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("@Username", request.Username);
            insertCmd.Parameters.AddWithValue("@Email", request.Email);
            insertCmd.Parameters.AddWithValue("@PasswordHash", HashPassword(request.Password));

            var userId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());

            return new LoginResponse
            {
                Success = true,
                Message = "Registration successful",
                User = new UserDto
                {
                    Id = userId,
                    Username = request.Username,
                    Email = request.Email
                },
                Token = "simple-jwt-token-" + userId
            };
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "salt"));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
    }
}
