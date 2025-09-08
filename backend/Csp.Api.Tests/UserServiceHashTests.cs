using System.Reflection;
using Csp.Api.Services;
using Microsoft.Extensions.Configuration;

namespace Csp.Api.Tests;

public class UserServiceHashTests
{
    private static UserService CreateService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=test;User Id=test;Password=test;SslMode=None",
                ["Jwt:Secret"] = "unit-test-secret-1234567890",
                ["Jwt:Issuer"] = "csp-api",
                ["Jwt:Audience"] = "csp-web"
            })
            .Build();

        var jwt = new JwtTokenService(config);
        return new UserService(config, jwt);
    }

    [Fact]
    public void Hash_And_Verify_Password_Work()
    {
        var service = CreateService();

        var hashMethod = typeof(UserService).GetMethod("HashPassword", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var verifyMethod = typeof(UserService).GetMethod("VerifyPassword", BindingFlags.NonPublic | BindingFlags.Instance)!;

        var password = "P@ssw0rd!";
        var hash = (string)hashMethod.Invoke(service, new object[] { password })!;

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);

        var ok = (bool)verifyMethod.Invoke(service, new object[] { password, hash })!;
        Assert.True(ok);

        var bad = (bool)verifyMethod.Invoke(service, new object[] { "wrong", hash })!;
        Assert.False(bad);
    }
}


