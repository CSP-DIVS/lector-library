using System.IdentityModel.Tokens.Jwt;
using Csp.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Csp.Api.Tests;

public class JwtTokenServiceTests
{
    private static (JwtTokenService svc, TokenValidationParameters parms) Create()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "0123456789abcdef0123456789abcdef",
                ["Jwt:Issuer"] = "csp-api",
                ["Jwt:Audience"] = "csp-web"
            })
            .Build();
        var svc = new JwtTokenService(config);
        return (svc, svc.GetValidationParameters());
    }

    [Fact]
    public void Generates_And_Validates_Token_With_Role()
    {
        var (svc, parms) = Create();
        var token = svc.GenerateToken(42, "alice", "Administrator");

        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(token, parms, out var validatedToken);

        var jwt = (JwtSecurityToken)validatedToken;
        Assert.Equal("csp-api", jwt.Issuer);
        Assert.Contains(jwt.Claims, c => c.Type.EndsWith("/name") && c.Value == "alice");
        Assert.Contains(jwt.Claims, c => c.Type.EndsWith("/nameidentifier") && c.Value == "42");
        Assert.Contains(jwt.Claims, c => c.Type.EndsWith("/role") && c.Value == "Administrator");
    }
}


