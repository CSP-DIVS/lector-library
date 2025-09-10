using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Csp.Api.Tests
{
    public static class TestAuthExtensions
    {
        public static HttpClient CreateClientAsAdmin(this WebApplicationFactory<Program> factory, int userId = 1)
        {
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
            client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
            return client;
        }

        public static HttpClient CreateClientAsMember(this WebApplicationFactory<Program> factory, int userId = 2)
        {
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Test-Role", "Member");
            client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
            return client;
        }
    }
}
