using System.Net;
using System.Net.Http.Json;
using Csp.Api.DTOs;

namespace Csp.Api.Tests;

public class MyProfileIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory factory;
    public MyProfileIntegrationTests(ApiFactory factory) { this.factory = factory; }

    [Fact]
    public async Task Profile_Get_Update_ChangePassword()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Member");
        client.DefaultRequestHeaders.Add("X-Test-UserId", "1");

        var get = await client.GetAsync("/api/users/my-profile");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var upd = await client.PutAsJsonAsync("/api/users/my-profile", new UpdateProfileRequest { Username = "me", Email = "me@e.com" });
        Assert.Equal(HttpStatusCode.OK, upd.StatusCode);

        var badPwd = await client.PutAsJsonAsync("/api/users/my-password", new ChangePasswordRequest { CurrentPassword = "x", NewPassword = "short" });
        Assert.Equal(HttpStatusCode.BadRequest, badPwd.StatusCode);

        var goodPwd = await client.PutAsJsonAsync("/api/users/my-password", new ChangePasswordRequest { CurrentPassword = "ok", NewPassword = "longenough" });
        Assert.Equal(HttpStatusCode.OK, goodPwd.StatusCode);
    }
}


