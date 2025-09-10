using Microsoft.AspNetCore.Mvc;

namespace Csp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HealthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "healthy",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            return Ok(new
            {
                hasJwtSecret = !string.IsNullOrEmpty(_configuration["JwtSettings:SecretKey"]),
                hasConnectionString = !string.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection")),
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                allowedOrigins = _configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "Not configured" }
            });
        }
    }
}
