using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Csp.Api.Tests
{
    public class MaintenanceControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        public MaintenanceControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TriggerFineCalculation_AdminAuth_ReturnsAffectedCount()
        {
            // Arrange: create client and authenticate as admin (replace with real token in real test)
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var jwt = scope.ServiceProvider.GetRequiredService<Csp.Api.Services.IJwtTokenService>();
            var token = jwt.GenerateToken(1, "admin", "Administrator");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Act
            var response = await client.PostAsync("/api/maintenance/trigger-fine-calculation", null);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(json);

            // Assert: should contain 'affected' property (number of rows updated)
            Assert.True(obj["affected"] != null);
        }
    }
}
