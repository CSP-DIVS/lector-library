using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Csp.Integration.Tests
{
    public class LoginTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public LoginTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_Succeeds_With_Seeded_TestUser()
        {
            var client = _factory.CreateClient();
            var request = new { Username = "testuser", Password = "password123" };

            var response = await client.PostAsJsonAsync("/api/Auth/login", request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(body); // body is not null
            Assert.True(body!.Success); // login successful
            Assert.NotNull(body.Token); // jwt token is returned
            Assert.Equal("testuser", body.User!.Username); // username is testuser
        }

        [Fact]
        public async Task Login_Fails_With_Wrong_Password()
        {
            var client = _factory.CreateClient();
            var request = new { Username = "testuser", Password = "wrongpwd" };

            var response = await client.PostAsJsonAsync("/api/Auth/login", request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(body);
            Assert.False(body!.Success);
        }

        [Fact]
        public async Task Login_BadRequest_When_Missing_Username_Or_Password()
        {
            var client = _factory.CreateClient();
            var request = new { Username = "", Password = "" };

            var response = await client.PostAsJsonAsync("/api/Auth/login", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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


