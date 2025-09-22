using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Csp.Integration.Tests
{
    public class MemberManagementTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public MemberManagementTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private static async Task<string> LoginAsAdminAsync(HttpClient client)
        {
            var login = new { Username = "admin", Password = "admin123!" };
            var resp = await client.PostAsJsonAsync("/api/Auth/login", login);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(body);
            Assert.True(body!.Success);
            Assert.False(string.IsNullOrEmpty(body.Token));
            return body.Token!;
        }

        [Fact]
        public async Task Create_Read_Update_Deactivate_Reactivate_Member_Flow()
        {
            var client = _factory.CreateClient();
            var token = await LoginAsAdminAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create member
            var unique = Guid.NewGuid().ToString("N").Substring(0, 8);
            var createReq = new { Username = $"member_{unique}", Email = $"member_{unique}@example.com", Password = "memberpass!1" }; // generate unique username and email to avoid conflicts
            var createResp = await client.PostAsJsonAsync("/api/users/members", createReq);
            Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(created);
            Assert.True(created!.Success);
            Assert.NotNull(created.User);
            var memberId = created.User!.Id;

            // Read members list (should include our user)
            var listResp = await client.GetAsync("/api/users/members?page=1&pageSize=20&q=" + createReq.Username);
            Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);
            var list = await listResp.Content.ReadFromJsonAsync<PagedMembersResponseDto>();
            Assert.NotNull(list);
            Assert.True(list!.Items.Any(u => u.Username == createReq.Username));

            // Update member
            var updateReq = new { Username = createReq.Username + "_upd", Email = $"upd_{unique}@example.com" };
            var updateResp = await client.PutAsJsonAsync($"/api/users/members/{memberId}", updateReq);
            Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

            // Verify update via list
            var listResp2 = await client.GetAsync("/api/users/members?page=1&pageSize=20&q=" + updateReq.Username);
            Assert.Equal(HttpStatusCode.OK, listResp2.StatusCode);
            var list2 = await listResp2.Content.ReadFromJsonAsync<PagedMembersResponseDto>();
            Assert.NotNull(list2);
            Assert.True(list2!.Items.Any(u => u.Username == updateReq.Username));

            // Deactivate
            var deactResp = await client.PutAsync($"/api/users/{memberId}/status?isActive=false", content: null);
            Assert.Equal(HttpStatusCode.OK, deactResp.StatusCode);

            // Reactivate
            var reactResp = await client.PutAsync($"/api/users/{memberId}/status?isActive=true", content: null);
            Assert.Equal(HttpStatusCode.OK, reactResp.StatusCode);
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

        private sealed class PagedMembersResponseDto
        {
            public IEnumerable<UserDto> Items { get; set; } = Enumerable.Empty<UserDto>();
            public int Total { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }
    }
}


