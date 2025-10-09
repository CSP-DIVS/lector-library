using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Csp.Api.Services;
using Csp.Api.DTOs;
using System.Security.Claims;

namespace Csp.Api.Controllers
{
    /// <summary>
    /// API controller for managing book lending operations.
    /// Handles borrowing, returning, renewing books, and retrieving lending records.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LendingsController : ControllerBase
    {
        private readonly ILendingService _lendingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LendingsController"/> class.
        /// </summary>
        /// <param name="lendingService">The lending service for handling business logic.</param>
        public LendingsController(ILendingService lendingService)
        {
            _lendingService = lendingService;
        }

        /// <summary>
        /// Allows a librarian to lend a book to a user.
        /// </summary>
        /// <param name="request">The borrow book request containing book and user information.</param>
        /// <returns>A <see cref="LendingResponse"/> indicating success or failure with lending details.</returns>
        /// <response code="200">Book borrowed successfully.</response>
        /// <response code="400">Invalid request or business rule violation (e.g., no copies available, user already has the book).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have librarian permissions.</response>
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

        /// <summary>
        /// Allows a librarian to process the return of a borrowed book.
        /// </summary>
        /// <param name="request">The return book request containing lending ID and optional fine amount.</param>
        /// <returns>A <see cref="LendingResponse"/> indicating success or failure with return details.</returns>
        /// <response code="200">Book returned successfully.</response>
        /// <response code="400">Invalid request or lending record not found.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have librarian permissions.</response>
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

        /// <summary>
        /// Allows a user to renew their loan for a borrowed book.
        /// </summary>
        /// <param name="request">The renew loan request containing the lending ID.</param>
        /// <returns>A <see cref="LendingResponse"/> indicating success or failure with updated lending details.</returns>
        /// <response code="200">Loan renewed successfully.</response>
        /// <response code="400">Invalid request or renewal not allowed (e.g., maximum renewals reached, overdue, has pending reservations).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not own this lending record.</response>
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

        /// <summary>
        /// Retrieves a paginated list of active (currently borrowed) books.
        /// Librarians can view all active loans; members can only view their own.
        /// </summary>
        /// <param name="userId">Optional user ID to filter loans. Ignored for members (automatically set to current user).</param>
        /// <param name="page">The page number to retrieve (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <returns>A <see cref="PagedLendingsResponse"/> containing the list of active loans.</returns>
        /// <response code="200">Active loans retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
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

        /// <summary>
        /// Retrieves a paginated list of loan history (returned books).
        /// Librarians can view all loan history; members can only view their own.
        /// </summary>
        /// <param name="userId">Optional user ID to filter history. Ignored for members (automatically set to current user).</param>
        /// <param name="page">The page number to retrieve (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <returns>A <see cref="PagedLendingsResponse"/> containing the loan history.</returns>
        /// <response code="200">Loan history retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
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

        /// <summary>
        /// Retrieves detailed information about a specific lending record by ID.
        /// Members can only view their own lending records.
        /// </summary>
        /// <param name="id">The lending record ID.</param>
        /// <returns>A <see cref="LendingDto"/> containing the lending details.</returns>
        /// <response code="200">Lending record retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have permission to view this lending record.</response>
        /// <response code="404">Lending record not found.</response>
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

        /// <summary>
        /// Retrieves the current user's ID from the authentication claims.
        /// </summary>
        /// <returns>The user ID, or 0 if not found.</returns>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Retrieves the current user's role from the authentication claims.
        /// </summary>
        /// <returns>The user role, defaulting to "Member" if not found.</returns>
        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";
        }
    }
}
