using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Csp.Api.DTOs;
using Csp.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Csp.Api.Tests;

public class FakeUserService : IUserService
{
    public Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        if (request.Username == "member" && request.Password == "ok")
            return Task.FromResult(new LoginResponse { Success = true, User = new UserDto { Id = 2, Username = "member", Email = "m@e.com", Role = "Member", IsActive = true }, Token = "t" });
        if (request.Username == "inactive" && request.Password == "ok")
            return Task.FromResult(new LoginResponse { Success = false, Message = "Your account is inactive. Please contact an administrator." });
        return Task.FromResult(new LoginResponse { Success = false, Message = "Invalid username or password" });
    }

    public Task<LoginResponse> RegisterAsync(RegisterRequest request) => Task.FromResult(new LoginResponse { Success = true });
    public Task InitializeDatabaseAsync() => Task.CompletedTask;

    public Task<LoginResponse> CreateMemberAsync(CreateMemberRequest request, int actorUserId)
    {
        if (request.Username == "dup") return Task.FromResult(new LoginResponse { Success = false, Message = "Username or email already exists" });
        return Task.FromResult(new LoginResponse { Success = true, User = new UserDto { Id = 5, Username = request.Username, Email = request.Email, Role = "Member", IsActive = true } });
    }

    public Task<PagedMembersResponse> GetMembersAsync(string? search, int page, int pageSize)
    {
        var all = new List<UserDto>
        {
            new UserDto{ Id=1, Username="a", Email="a@e.com", Role="Member", IsActive=true },
            new UserDto{ Id=2, Username="b", Email="b@e.com", Role="Member", IsActive=false }
        };
        return Task.FromResult(new PagedMembersResponse{ Items = all, Total = all.Count, Page = 1, PageSize = 10 });
    }

    public Task<bool> UpdateMemberAsync(int id, UpdateMemberRequest request, int actorUserId) => Task.FromResult(true);
    public Task<bool> UpdateUserStatusAsync(int id, bool isActive, int actorUserId) => Task.FromResult(id != actorUserId);
    public Task<UserDto?> GetCurrentUserAsync(int id) => Task.FromResult<UserDto?>(new UserDto{ Id=id, Username="me", Email="me@e.com", Role="Member", IsActive=true });
    public Task<bool> UpdateMyProfileAsync(int id, UpdateProfileRequest request) => Task.FromResult(true);
    public Task<bool> ChangePasswordAsync(int id, ChangePasswordRequest request) => Task.FromResult(request.NewPassword.Length >= 8);

    // FIX: Implement missing methods from IUserService
    public Task<LoginResponse> CreateUserAsync(CreateUserRequest request, int actorUserId)
    {
        if (request.Username == "dup")
        {
            return Task.FromResult(new LoginResponse { Success = false, Message = "Username or email already exists" });
        }
        var newUser = new UserDto { Id = new Random().Next(100, 200), Username = request.Username, Email = request.Email, Role = request.Role, IsActive = true };
        return Task.FromResult(new LoginResponse { Success = true, User = newUser });
    }

    public Task<PagedMembersResponse> GetUsersAsync(string? search, int page, int pageSize, string? sort, bool? active)
    {
        var users = new List<UserDto>
        {
            new UserDto{ Id=1, Username="admin", Email="admin@e.com", Role="Admin", IsActive=true },
            new UserDto{ Id=2, Username="member", Email="m@e.com", Role="Member", IsActive=true },
            new UserDto{ Id=3, Username="inactive", Email="ia@e.com", Role="Member", IsActive=false }
        };
        return Task.FromResult(new PagedMembersResponse { Items = users, Total = users.Count, Page = page, PageSize = pageSize });
    }

    public Task<bool> UpdateUserAsync(int id, UpdateUserRequest request, int actorUserId)
    {
        return Task.FromResult(true); // Always succeed for mock purposes
    }

    public Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        if (username == "member")
        {
            return Task.FromResult<UserDto?>(new UserDto { Id = 2, Username = "member", Email = "m@e.com", Role = "Member", IsActive = true });
        }
        return Task.FromResult<UserDto?>(null);
    }
}

public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserService));
            if (descriptor != null) services.Remove(descriptor);
            services.AddSingleton<IUserService, FakeUserService>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }

    public HttpClient CreateClientAsAdmin(int userId = 1, string username = "admin")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Username", username);
        return client;
    }

    public HttpClient CreateClientAsMember(int userId = 2, string username = "member")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Member");
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Username", username);
        return client;
    }

}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    // FIX: Update constructor to remove obsolete ISystemClock
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers.ContainsKey("X-Test-Role") ? Request.Headers["X-Test-Role"].ToString() : "Member";
        var sub = Request.Headers.ContainsKey("X-Test-UserId") ? Request.Headers["X-Test-UserId"].ToString() : "1";
        var name = Request.Headers.ContainsKey("X-Test-Username") ? Request.Headers["X-Test-Username"].ToString() : "testuser";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, sub),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role),
            new Claim("sub", sub)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
