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
        Task<LoginResponse> CreateMemberAsync(CreateMemberRequest request, int actorUserId);
        Task<PagedMembersResponse> GetMembersAsync(string? search, int page, int pageSize);
        Task<bool> UpdateMemberAsync(int id, UpdateMemberRequest request, int actorUserId);
        Task<bool> UpdateUserStatusAsync(int id, bool isActive, int actorUserId);
        Task<UserDto?> GetCurrentUserAsync(int id);
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<bool> UpdateMyProfileAsync(int id, UpdateProfileRequest request);
        Task<bool> ChangePasswordAsync(int id, ChangePasswordRequest request);
    }

    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly IJwtTokenService _jwtTokenService;

        public UserService(IConfiguration configuration, IJwtTokenService jwtTokenService)
        {
            _configuration = configuration;
            _jwtTokenService = jwtTokenService;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                                 ?? _configuration.GetConnectionString("Default")
                                 ?? throw new InvalidOperationException("Connection string not found");
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

                    var createTableSql = @"
                        CREATE TABLE IF NOT EXISTS users (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            Username VARCHAR(50) UNIQUE NOT NULL,
                            Email VARCHAR(100) UNIQUE NOT NULL,
                            PasswordHash VARCHAR(255) NOT NULL,
                            Role VARCHAR(20) NOT NULL DEFAULT 'Member',
                            IsActive TINYINT(1) NOT NULL DEFAULT 1,
                            CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                        )";

                    await using var cmd = new MySqlCommand(createTableSql, conn);
                    await cmd.ExecuteNonQueryAsync();

                    var createAuditLogSql = @"
                        CREATE TABLE IF NOT EXISTS audit_log (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            ActorUserId INT NULL,
                            Action VARCHAR(100) NOT NULL,
                            TargetUserId INT NULL,
                            Details TEXT NULL,
                            CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                        )";
                    await using var auditCmd = new MySqlCommand(createAuditLogSql, conn);
                    await auditCmd.ExecuteNonQueryAsync();

                    var countUsersSql = "SELECT COUNT(*) FROM users";
                    await using var countCmd = new MySqlCommand(countUsersSql, conn);
                    var totalUsers = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                    if (totalUsers == 0)
                    {
                        var adminPass = HashPassword("admin123!");
                        var librarianPass = HashPassword("lib123!");
                        var memberPass = HashPassword("member123!");

                        var seedSql = @"
                            INSERT INTO users (Username, Email, PasswordHash, Role, IsActive) VALUES
                            ('admin', 'admin@example.com', @AdminHash, 'Administrator', 1),
                            ('librarian', 'librarian@example.com', @LibrarianHash, 'Librarian', 1),
                            ('member', 'member@example.com', @MemberHash, 'Member', 1),
                            ('testuser', 'test@example.com', @TestHash, 'Member', 1)
                        ";
                        await using var seedCmd = new MySqlCommand(seedSql, conn);
                        seedCmd.Parameters.AddWithValue("@AdminHash", adminPass);
                        seedCmd.Parameters.AddWithValue("@LibrarianHash", librarianPass);
                        seedCmd.Parameters.AddWithValue("@MemberHash", memberPass);
                        seedCmd.Parameters.AddWithValue("@TestHash", HashPassword("password123"));
                        await seedCmd.ExecuteNonQueryAsync();
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

            var sql = @"SELECT Id, Username, Email, PasswordHash, Role, IsActive 
                        FROM users 
                        WHERE Username = @Identifier OR Email = @Identifier";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Identifier", request.Username);

            await using var reader = await cmd.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                var storedHash = reader["PasswordHash"]?.ToString();
                if (!string.IsNullOrEmpty(storedHash) && VerifyPassword(request.Password, storedHash))
                {
                    var isActive = Convert.ToBoolean(reader["IsActive"]);
                    if (!isActive)
                    {
                        return new LoginResponse { Success = false, Message = "Your account is inactive. Please contact an administrator." };
                    }
                    return new LoginResponse
                    {
                        Success = true,
                        Message = "Login successful",
                        User = new UserDto
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Username = reader["Username"]?.ToString() ?? "",
                            Email = reader["Email"]?.ToString() ?? "",
                            Role = reader["Role"]?.ToString() ?? "",
                            IsActive = isActive
                        },
                        Token = _jwtTokenService.GenerateToken(
                            Convert.ToInt32(reader["Id"]),
                            reader["Username"]?.ToString() ?? string.Empty,
                            reader["Role"]?.ToString() ?? string.Empty
                        )
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
                INSERT INTO users (Username, Email, PasswordHash, Role, IsActive) 
                VALUES (@Username, @Email, @PasswordHash, 'Member', 1);
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

        public async Task<LoginResponse> CreateMemberAsync(CreateMemberRequest request, int actorUserId)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var checkSql = "SELECT COUNT(*) FROM users WHERE Username = @Username OR Email = @Email";
            await using var checkCmd = new MySqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@Username", request.Username);
            checkCmd.Parameters.AddWithValue("@Email", request.Email);
            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;
            if (exists)
            {
                return new LoginResponse { Success = false, Message = "Username or email already exists" };
            }

            var insertSql = @"INSERT INTO users (Username, Email, PasswordHash, Role, IsActive)
                              VALUES (@Username, @Email, @PasswordHash, 'Member', 1);
                              SELECT LAST_INSERT_ID();";
            await using var insertCmd = new MySqlCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("@Username", request.Username);
            insertCmd.Parameters.AddWithValue("@Email", request.Email);
            insertCmd.Parameters.AddWithValue("@PasswordHash", HashPassword(request.Password));
            var userId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());

            await WriteAuditAsync(conn, actorUserId, "CreateMember", userId, $"username={request.Username}");

            return new LoginResponse
            {
                Success = true,
                Message = "Member registered successfully",
                User = new UserDto { Id = userId, Username = request.Username, Email = request.Email, Role = "Member", IsActive = true }
            };
        }

        public async Task<PagedMembersResponse> GetMembersAsync(string? search, int page, int pageSize)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var where = "WHERE Role = 'Member'" + (string.IsNullOrWhiteSpace(search) ? "" : " AND (Username LIKE @q OR Email LIKE @q)");
            var countSql = $"SELECT COUNT(*) FROM users {where}";
            await using var countCmd = new MySqlCommand(countSql, conn);
            if (!string.IsNullOrWhiteSpace(search)) countCmd.Parameters.AddWithValue("@q", $"%{search}%");
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            var offset = (page - 1) * pageSize;
            var listSql = $@"SELECT Id, Username, Email, Role, IsActive FROM users {where} ORDER BY Id DESC LIMIT @ps OFFSET @off";
            await using var listCmd = new MySqlCommand(listSql, conn);
            if (!string.IsNullOrWhiteSpace(search)) listCmd.Parameters.AddWithValue("@q", $"%{search}%");
            listCmd.Parameters.AddWithValue("@ps", pageSize);
            listCmd.Parameters.AddWithValue("@off", offset);

            var items = new List<UserDto>();
            await using var reader = await listCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new UserDto
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Username = reader["Username"]?.ToString() ?? string.Empty,
                    Email = reader["Email"]?.ToString() ?? string.Empty,
                    Role = reader["Role"]?.ToString() ?? string.Empty,
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                });
            }

            return new PagedMembersResponse { Items = items, Total = total, Page = page, PageSize = pageSize };
        }

        public async Task<bool> UpdateMemberAsync(int id, UpdateMemberRequest request, int actorUserId)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var dupSql = "SELECT COUNT(*) FROM users WHERE (Username = @Username OR Email = @Email) AND Id <> @Id";
            await using var dupCmd = new MySqlCommand(dupSql, conn);
            dupCmd.Parameters.AddWithValue("@Username", request.Username);
            dupCmd.Parameters.AddWithValue("@Email", request.Email);
            dupCmd.Parameters.AddWithValue("@Id", id);
            var dup = Convert.ToInt32(await dupCmd.ExecuteScalarAsync()) > 0;
            if (dup) return false;

            var updateSql = "UPDATE users SET Username=@Username, Email=@Email WHERE Id=@Id AND Role='Member'";
            await using var updateCmd = new MySqlCommand(updateSql, conn);
            updateCmd.Parameters.AddWithValue("@Username", request.Username);
            updateCmd.Parameters.AddWithValue("@Email", request.Email);
            updateCmd.Parameters.AddWithValue("@Id", id);
            var rows = await updateCmd.ExecuteNonQueryAsync();

            if (rows > 0)
            {
                await WriteAuditAsync(conn, actorUserId, "UpdateMember", id, $"username={request.Username}");
                return true;
            }
            return false;
        }

        private static async Task WriteAuditAsync(MySqlConnection conn, int actorUserId, string action, int targetUserId, string details)
        {
            var sql = "INSERT INTO audit_log (ActorUserId, Action, TargetUserId, Details) VALUES (@ActorUserId, @Action, @TargetUserId, @Details)";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);
            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@TargetUserId", targetUserId);
            cmd.Parameters.AddWithValue("@Details", details);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> UpdateUserStatusAsync(int id, bool isActive, int actorUserId)
        {
            if (id == actorUserId) return false;
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var updateSql = "UPDATE users SET IsActive=@IsActive WHERE Id=@Id";
            await using var updateCmd = new MySqlCommand(updateSql, conn);
            updateCmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
            updateCmd.Parameters.AddWithValue("@Id", id);
            var rows = await updateCmd.ExecuteNonQueryAsync();
            if (rows > 0)
            {
                await WriteAuditAsync(conn, actorUserId, isActive ? "ReactivateUser" : "DeactivateUser", id, "");
                return true;
            }
            return false;
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

        public async Task<UserDto?> GetCurrentUserAsync(int id)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT Id, Username, Email, Role, IsActive FROM users WHERE Id=@Id";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync())
            {
                return new UserDto
                {
                    Id = Convert.ToInt32(r["Id"]),
                    Username = r["Username"]?.ToString() ?? string.Empty,
                    Email = r["Email"]?.ToString() ?? string.Empty,
                    Role = r["Role"]?.ToString() ?? string.Empty,
                    IsActive = Convert.ToBoolean(r["IsActive"])
                };
            }
            return null;
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT Id, Username, Email, Role, IsActive FROM users WHERE Username=@Username";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username);
            await using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync())
            {
                return new UserDto
                {
                    Id = Convert.ToInt32(r["Id"]),
                    Username = r["Username"]?.ToString() ?? string.Empty,
                    Email = r["Email"]?.ToString() ?? string.Empty,
                    Role = r["Role"]?.ToString() ?? string.Empty,
                    IsActive = Convert.ToBoolean(r["IsActive"])
                };
            }
            return null;
        }

        public async Task<bool> UpdateMyProfileAsync(int id, UpdateProfileRequest request)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var dupSql = "SELECT COUNT(*) FROM users WHERE (Username=@Username OR Email=@Email) AND Id<>@Id";
            await using var dupCmd = new MySqlCommand(dupSql, conn);
            dupCmd.Parameters.AddWithValue("@Username", request.Username);
            dupCmd.Parameters.AddWithValue("@Email", request.Email);
            dupCmd.Parameters.AddWithValue("@Id", id);
            var dup = Convert.ToInt32(await dupCmd.ExecuteScalarAsync()) > 0;
            if (dup) return false;

            var sql = "UPDATE users SET Username=@Username, Email=@Email WHERE Id=@Id";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", request.Username);
            cmd.Parameters.AddWithValue("@Email", request.Email);
            cmd.Parameters.AddWithValue("@Id", id);
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> ChangePasswordAsync(int id, ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8) return false;
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var getSql = "SELECT PasswordHash FROM users WHERE Id=@Id";
            await using var getCmd = new MySqlCommand(getSql, conn);
            getCmd.Parameters.AddWithValue("@Id", id);
            var stored = (string?)await getCmd.ExecuteScalarAsync();
            if (string.IsNullOrEmpty(stored) || !VerifyPassword(request.CurrentPassword, stored)) return false;

            var updSql = "UPDATE users SET PasswordHash=@Hash WHERE Id=@Id";
            await using var updCmd = new MySqlCommand(updSql, conn);
            updCmd.Parameters.AddWithValue("@Hash", HashPassword(request.NewPassword));
            updCmd.Parameters.AddWithValue("@Id", id);
            var rows = await updCmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}
