using System.Net;
using System.Net.Http.Json;
using Csp.Api.DTOs;

namespace Csp.Api.Tests;

public class LoginIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory factory;
    public LoginIntegrationTests(ApiFactory factory) { this.factory = factory; }

    [Fact]
    public async Task Login_Success()
    {
        var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Username = "member", Password = "ok" });
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword()
    {
        var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Username = "member", Password = "bad" });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Login_Inactive()
    {
        var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Username = "inactive", Password = "ok" });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}


