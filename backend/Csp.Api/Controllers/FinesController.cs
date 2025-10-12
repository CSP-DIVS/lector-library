using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Csp.Api.Services;
using Csp.Api.DTOs;
using System.Security.Claims;

namespace Csp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinesController : ControllerBase
    {
        private readonly ILendingService _lendingService;

        public FinesController(ILendingService lendingService)
        {
            _lendingService = lendingService;
        }

        [HttpPut("{lendingId}/adjust")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<ActionResult<AdjustFineResponse>> AdjustFine(int lendingId, [FromBody] AdjustFineRequest request)
        {
            var adminId = GetCurrentUserId();
            var result = await _lendingService.AdjustFineAsync(lendingId, request, adminId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<ActionResult<List<UserFineDto>>> GetUserFines(int userId)
        {
            var fines = await _lendingService.GetUserFinesAsync(userId);
            return Ok(fines);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
