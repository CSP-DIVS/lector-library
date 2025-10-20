using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Csp.Api.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(int userId, string username, string role);
        TokenValidationParameters GetValidationParameters();
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GenerateToken(int userId, string username, string role)
        {
            // Support both configuration shapes: "Jwt" and older/alternate "JwtSettings"
            var secret = configuration["Jwt:Secret"] 
                         ?? configuration["JwtSettings:SecretKey"] 
                         ?? "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
            if (secret.Length < 32) secret = secret.PadRight(32, '0');
            var issuer = configuration["Jwt:Issuer"] ?? configuration["JwtSettings:Issuer"] ?? "csp-api";
            var audience = configuration["Jwt:Audience"] ?? configuration["JwtSettings:Audience"] ?? "csp-web";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public TokenValidationParameters GetValidationParameters()
        {
            // Support both configuration shapes: "Jwt" and older/alternate "JwtSettings"
            var secret = configuration["Jwt:Secret"] 
                         ?? configuration["JwtSettings:SecretKey"] 
                         ?? "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
            if (secret.Length < 32) secret = secret.PadRight(32, '0');
            var issuer = configuration["Jwt:Issuer"] ?? configuration["JwtSettings:Issuer"] ?? "csp-api";
            var audience = configuration["Jwt:Audience"] ?? configuration["JwtSettings:Audience"] ?? "csp-web";

            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };
        }
    }
}


