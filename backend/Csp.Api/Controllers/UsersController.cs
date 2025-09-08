using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Csp.Api.Services;
using Csp.Api.DTOs;
using System.Security.Claims;

namespace Csp.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPost("members")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<ActionResult<LoginResponse>> CreateMember([FromBody] CreateMemberRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new LoginResponse { Success = false, Message = "Username, email and password are required" });

            var actorId = GetUserId();
            var result = await userService.CreateMemberAsync(request, actorId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<ActionResult<LoginResponse>> CreateUser([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new LoginResponse { Success = false, Message = "Username, email and password are required" });

            var actorId = GetUserId();
            var result = await userService.CreateUserAsync(request, actorId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<ActionResult<PagedMembersResponse>> GetUsers([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? role = null, [FromQuery] bool? isActive = null)
        {
            if (page <= 0 || pageSize <= 0) return BadRequest();
            var result = await userService.GetUsersAsync(q, page, pageSize, role, isActive);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<ActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email)) return BadRequest();
            var actorId = GetUserId();
            var ok = await userService.UpdateUserAsync(id, request, actorId);
            if (!ok) return BadRequest();
            return Ok();
        }

        [HttpGet("members")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<ActionResult<PagedMembersResponse>> GetMembers([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0) return BadRequest();
            var result = await userService.GetMembersAsync(q, page, pageSize);
            return Ok(result);
        }

        [HttpPut("members/{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<ActionResult> UpdateMember([FromRoute] int id, [FromBody] UpdateMemberRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email)) return BadRequest();
            var actorId = GetUserId();
            var ok = await userService.UpdateMemberAsync(id, request, actorId);
            if (!ok) return BadRequest();
            return Ok();
        }

        [HttpPut("{id}/status")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<ActionResult> UpdateStatus([FromRoute] int id, [FromQuery] bool isActive)
        {
            var actorId = GetUserId();
            var ok = await userService.UpdateUserStatusAsync(id, isActive, actorId);
            if (!ok) return BadRequest();
            return Ok();
        }

        [HttpGet("my-profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetMyProfile()
        {
            var id = GetUserId();
            if (id > 0)
            {
                var byId = await userService.GetCurrentUserAsync(id);
                if (byId != null) return Ok(byId);
            }

            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            if (!string.IsNullOrEmpty(name))
            {
                var byName = await userService.GetUserByUsernameAsync(name);
                if (byName != null) return Ok(byName);
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var fallback = new UserDto
            {
                Id = id,
                Username = name ?? string.Empty,
                Email = string.Empty,
                Role = role,
                IsActive = true
            };
            return Ok(fallback);
        }

        [HttpPut("my-profile")]
        [Authorize]
        public async Task<ActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            var id = GetUserId();
            var ok = await userService.UpdateMyProfileAsync(id, request);
            if (!ok) return BadRequest();
            return Ok();
        }

        [HttpPut("my-password")]
        [Authorize]
        public async Task<ActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest request)
        {
            var id = GetUserId();
            var ok = await userService.ChangePasswordAsync(id, request);
            if (!ok) return BadRequest();
            return Ok();
        }

        private int GetUserId()
        {
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value
                      ?? "0";
            if (int.TryParse(sub, out var id)) return id;
            return 0;
        }
    }
}


