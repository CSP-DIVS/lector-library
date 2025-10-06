using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Csp.Api.Services;
using Csp.Api.DTOs;
using System.Security.Claims;

namespace Csp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LendingsController : ControllerBase
    {
        private readonly ILendingService _lendingService;

        public LendingsController(ILendingService lendingService)
        {
            _lendingService = lendingService;
        }

        [HttpPost("borrow")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<ActionResult<LendingResponse>> BorrowBook([FromBody] BorrowBookRequest request)
        {
            var result = await _lendingService.BorrowBookAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("return")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<ActionResult<LendingResponse>> ReturnBook([FromBody] ReturnBookRequest request)
        {
            var result = await _lendingService.ReturnBookAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("renew")]
        [Authorize]
        public async Task<ActionResult<LendingResponse>> RenewLoan([FromBody] RenewLoanRequest request)
        {
            var userId = GetCurrentUserId();
            var result = await _lendingService.RenewLoanAsync(request, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("active")]
        [Authorize]
        public async Task<ActionResult<PagedLendingsResponse>> GetActiveLoans(
            [FromQuery] int? userId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var currentUserId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            // Members can only see their own loans
            if (userRole == "Member")
            {
                userId = currentUserId;
            }

            var result = await _lendingService.GetActiveLoansAsync(userId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("history")]
        [Authorize]
        public async Task<ActionResult<PagedLendingsResponse>> GetLoanHistory(
            [FromQuery] int? userId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var currentUserId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            // Members can only see their own history
            if (userRole == "Member")
            {
                userId = currentUserId;
            }

            var result = await _lendingService.GetLoanHistoryAsync(userId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<LendingDto>> GetLending(int id)
        {
            var lending = await _lendingService.GetLendingByIdAsync(id);
            
            if (lending == null)
            {
                return NotFound(new { message = "Lending record not found" });
            }

            // Members can only see their own lendings
            var userRole = GetCurrentUserRole();
            var currentUserId = GetCurrentUserId();
            if (userRole == "Member" && lending.UserId != currentUserId)
            {
                return Forbid();
            }

            return Ok(lending);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";
        }
    }
}
