using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Csp.Api.Services;
using Csp.Api.DTOs;

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

            var actorId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var result = await userService.CreateMemberAsync(request, actorId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
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
            var actorId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var ok = await userService.UpdateMemberAsync(id, request, actorId);
            if (!ok) return BadRequest();
            return Ok();
        }

        [HttpPut("{id}/status")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<ActionResult> UpdateStatus([FromRoute] int id, [FromQuery] bool isActive)
        {
            var actorId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var ok = await userService.UpdateUserStatusAsync(id, isActive, actorId);
            if (!ok) return BadRequest();
            return Ok();
        }

        [HttpGet("my-profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetMyProfile()
        {
            var id = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var user = await userService.GetCurrentUserAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("my-profile")]
        [Authorize]
        public async Task<ActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            var id = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var ok = await userService.UpdateMyProfileAsync(id, request);
            if (!ok) return BadRequest();
            return Ok();
        }

        [HttpPut("my-password")]
        [Authorize]
        public async Task<ActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest request)
        {
            var id = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var ok = await userService.ChangePasswordAsync(id, request);
            if (!ok) return BadRequest();
            return Ok();
        }
    }
}


