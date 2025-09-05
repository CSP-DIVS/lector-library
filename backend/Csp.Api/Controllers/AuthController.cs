using Microsoft.AspNetCore.Mvc;
using Csp.Api.DTOs;
using Csp.Api.Services;

namespace Csp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new LoginResponse 
                { 
                    Success = false, 
                    Message = "Username and password are required" 
                });
            }

            var result = await _userService.LoginAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return Unauthorized(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || 
                string.IsNullOrEmpty(request.Email) || 
                string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new LoginResponse 
                { 
                    Success = false, 
                    Message = "Username, email and password are required" 
                });
            }

            var result = await _userService.RegisterAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        [HttpGet("test-credentials")]
        public ActionResult GetTestCredentials()
        {
            return Ok(new 
            { 
                message = "Use these credentials to test login",
                username = "testuser",
                password = "password123"
            });
        }
    }
}
