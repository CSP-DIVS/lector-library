using System.Net;
using System.Net.Http.Json;
using Csp.Api.DTOs;

namespace Csp.Api.Tests;

public class MemberIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory factory;
    public MemberIntegrationTests(ApiFactory factory) { this.factory = factory; }

    [Fact]
    public async Task MemberCrud_Flows()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");
        client.DefaultRequestHeaders.Add("X-Test-UserId", "99");

        var bad = await client.PostAsJsonAsync("/api/users/members", new CreateMemberRequest { Username = "dup", Email = "dup@e.com", Password = "x" });
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);

        var ok = await client.PostAsJsonAsync("/api/users/members", new CreateMemberRequest { Username = "new", Email = "n@e.com", Password = "x" });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);

        var list = await client.GetAsync("/api/users/members");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);

        var upd = await client.PutAsJsonAsync("/api/users/members/1", new UpdateMemberRequest { Username = "u", Email = "u@e.com" });
        Assert.Equal(HttpStatusCode.OK, upd.StatusCode);
    }

    [Fact]
    public async Task StatusToggle_Works()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");
        client.DefaultRequestHeaders.Add("X-Test-UserId", "99");
        var resp = await client.PutAsync("/api/users/2/status?isActive=true", null);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}


