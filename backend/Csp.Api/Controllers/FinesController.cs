using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Csp.Api.Services;
using Csp.Api.DTOs;
using System.Security.Claims;

namespace Csp.Api.Controllers
{
    /// <summary>
    /// API controller for managing fines and payments.
    /// Handles fine creation, payment processing, and retrieval operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FinesController : ControllerBase
    {
        private readonly IFineService _fineService;

        public FinesController(IFineService fineService)
        {
            _fineService = fineService;
        }

        /// <summary>
        /// Gets fines for the authenticated user.
        /// </summary>
        [HttpGet("my-fines")]
        public async Task<IActionResult> GetMyFines([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user authentication" });
            }

            // Automatically generate fines for overdue loans before fetching
            await _fineService.CreateFinesForOverdueLoansAsync();

            var result = await _fineService.GetUserFinesAsync(userId, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Gets all fines in the system (Librarian/Admin only).
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<IActionResult> GetAllFines([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Automatically generate fines for overdue loans before fetching
            await _fineService.CreateFinesForOverdueLoansAsync();
            
            var result = await _fineService.GetAllFinesAsync(page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Gets fines for a specific user (Librarian/Admin only).
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<IActionResult> GetUserFines(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _fineService.GetUserFinesAsync(userId, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Gets payment history for the authenticated user.
        /// </summary>
        [HttpGet("my-payments")]
        public async Task<IActionResult> GetMyPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user authentication" });
            }

            var result = await _fineService.GetUserPaymentsAsync(userId, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Gets all payment history (Librarian/Admin only).
        /// </summary>
        [HttpGet("payments")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<IActionResult> GetAllPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _fineService.GetAllPaymentsAsync(page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Gets payment history for a specific user (Librarian/Admin only).
        /// </summary>
        [HttpGet("payments/user/{userId}")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<IActionResult> GetUserPayments(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _fineService.GetUserPaymentsAsync(userId, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Gets fine statistics for the authenticated user.
        /// </summary>
        [HttpGet("my-statistics")]
        public async Task<IActionResult> GetMyStatistics()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user authentication" });
            }

            var result = await _fineService.GetUserFineStatisticsAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Processes a payment for a fine.
        /// </summary>
        [HttpPost("pay")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user authentication" });
            }

            request.UserId = userId;
            var result = await _fineService.ProcessPaymentAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Waives a fine (Librarian/Admin only).
        /// </summary>
        [HttpPost("waive")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<IActionResult> WaiveFine([FromBody] WaiveFineRequest request)
        {
            var result = await _fineService.WaiveFineAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Adjusts the fine amount (Librarian/Admin only).
        /// </summary>
        [HttpPost("adjust-amount")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<IActionResult> AdjustFineAmount([FromBody] AdjustFineAmountRequest request)
        {
            var result = await _fineService.AdjustFineAmountAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Creates fines for overdue loans (Librarian/Admin only).
        /// This should be run periodically to create fines for overdue books.
        /// </summary>
        [HttpPost("generate-overdue-fines")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<IActionResult> GenerateOverdueFines()
        {
            var count = await _fineService.CreateFinesForOverdueLoansAsync();
            return Ok(new { message = $"Created {count} fine(s) for overdue loans", count });
        }
        
        /// <summary>
        /// Test endpoint to check overdue loans without creating fines (for debugging).
        /// </summary>
        [HttpGet("check-overdue")]
        public async Task<IActionResult> CheckOverdueLoans()
        {
            var count = await _fineService.CreateFinesForOverdueLoansAsync();
            return Ok(new { 
                message = $"Checked and created {count} fine(s) for overdue loans", 
                count,
                timestamp = DateTime.UtcNow 
            });
        }
    }
}
