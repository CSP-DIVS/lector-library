using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Csp.Integration.Tests
{
    public class ProfileTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ProfileTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private static async Task<string> LoginAsync(HttpClient client, string username, string password)
        {
            var resp = await client.PostAsJsonAsync("/api/Auth/login", new { Username = username, Password = password });
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(body);
            Assert.True(body!.Success);
            return body.Token!;
        }

        private static async Task<(string username, string password)> CreateUniqueMemberAsync(HttpClient client)
        {
            // Admin login
            var adminToken = await LoginAsync(client, "admin", "admin123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Create unique member
            var unique = Guid.NewGuid().ToString("N").Substring(0, 8);
            var username = $"profile_{unique}";
            var email = $"profile_{unique}@example.com";
            var password = "memberpass!1";
            var createResp = await client.PostAsJsonAsync("/api/users/members", new { Username = username, Email = email, Password = password });
            Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);

            // Clear admin auth for subsequent member auth
            client.DefaultRequestHeaders.Authorization = null;
            return (username, password);
        }

        [Fact]
        public async Task UpdateMyProfile_Succeeds_And_Reflects_In_Get()
        {
            var client = _factory.CreateClient();
            var creds = await CreateUniqueMemberAsync(client);
            var token = await LoginAsync(client, creds.username, creds.password);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Update to unique username/email
            var unique = Guid.NewGuid().ToString("N").Substring(0, 8);
            var update = new { Username = $"member_{unique}", Email = $"mem_{unique}@example.com" };
            var updateResp = await client.PutAsJsonAsync("/api/users/my-profile", update);
            Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

            // Read back
            var getResp = await client.GetAsync("/api/users/my-profile");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
            var me = await getResp.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(me);
            Assert.Equal(update.Username, me!.Username);
            Assert.Equal(update.Email, me.Email);
        }

        [Fact]
        public async Task UpdateMyProfile_Unauthorized_Without_Token()
        {
            var client = _factory.CreateClient();
            var resp = await client.PutAsJsonAsync("/api/users/my-profile", new { Username = "x", Email = "x@e.com" });
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact]
        public async Task UpdateMyProfile_BadRequest_On_Duplicate_Username_Or_Email()
        {
            var client = _factory.CreateClient();
            var creds = await CreateUniqueMemberAsync(client);
            var token = await LoginAsync(client, creds.username, creds.password);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Try to use existing admin email to trigger duplicate
            var resp = await client.PutAsJsonAsync("/api/users/my-profile", new { Username = "admin", Email = "admin@example.com" });
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task ChangeMyPassword_BadRequest_On_Incorrect_CurrentPassword()
        {
            var client = _factory.CreateClient();
            var creds = await CreateUniqueMemberAsync(client);
            var token = await LoginAsync(client, creds.username, creds.password);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.PutAsJsonAsync("/api/users/my-password", new { CurrentPassword = "wrong", NewPassword = "newpassword1" });
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task ChangeMyPassword_BadRequest_On_TooShort_NewPassword() // creat new member and try to change password to too short
        {
            var client = _factory.CreateClient();
            var creds = await CreateUniqueMemberAsync(client);
            var token = await LoginAsync(client, creds.username, creds.password);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.PutAsJsonAsync("/api/users/my-password", new { CurrentPassword = "member123!", NewPassword = "short" });
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        private sealed class LoginResponseDto
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public UserDto? User { get; set; }
            public string? Token { get; set; }
        }

        private sealed class UserDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }
    }
}


