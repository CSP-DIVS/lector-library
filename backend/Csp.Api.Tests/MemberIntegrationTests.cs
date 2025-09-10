using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Csp.Api.DTOs;
using Csp.Api.Tests; // Add this using statement
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Csp.Api.Tests
{
    public class MemberIntegrationTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public MemberIntegrationTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateMember_WhenUsernameExists_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClientAsAdmin();
            var dto = new CreateMemberRequest { Username = "dup", Email = "unique1@e.com", Password = "Password123!" };

            // Act
            var response = await client.PostAsJsonAsync("/api/users/members", dto); // fixed the bad endpoint of /api/auth/create-member

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var error = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.Equal("Username or email already exists", error.Message);
        }

        [Fact]
        public async Task UpdateMember_AsMember_ReturnsForbidden()
        {
            // Arrange
            var memberClient = _factory.CreateClientAsMember();
            var updateDto = new UpdateMemberRequest { Username = "someusername", Email = "u@e.com" };
            var otherMemberId = 3; // An ID other than the logged-in user (2)

            // Act
            // A member tries to update another member's profile
            var response = await memberClient.PutAsJsonAsync($"/api/users/{otherMemberId}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        
        [Fact]
        public async Task UpdateMember_AsAdmin_ReturnsOk()
        {
            // Arrange
            var adminClient = _factory.CreateClientAsAdmin();
            var updateDto = new UpdateMemberRequest { Username = "someusername", Email = "u@e.com" };
            var memberId = 3;

            // Act
            // An admin updates a member's profile
            var response = await adminClient.PutAsJsonAsync($"/api/users/{memberId}", updateDto);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ToggleStatus_ForOwnAdminAccount_ReturnsBadRequest()
        {
            // Arrange
            var adminClient = _factory.CreateClientAsAdmin(userId: 1); // Admin's ID is 1
            var statusDto = new { isActive = false };

            // Act
            // Admin tries to deactivate themselves
            var response = await adminClient.PutAsJsonAsync($"/api/users/1/status", statusDto);

            // Assert
            // The FakeUserService returns 'false' if actorUserId == id
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ToggleStatus_ForOtherUserAsAdmin_ReturnsOk()
        {
            // Arrange
            var adminClient = _factory.CreateClientAsAdmin(userId: 1);
            var statusDto = new { isActive = false };

            // Act
            // Admin deactivates another user
            var response = await adminClient.PutAsJsonAsync($"/api/users/2/status", statusDto);

            // Assert
            // The FakeUserService returns 'true' if actorUserId != id
            response.EnsureSuccessStatusCode();
        }
    }
}
