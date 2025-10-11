using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Csp.Api.Services;
using Csp.Api.DTOs;
using System.Security.Claims;

namespace Csp.Api.Controllers
{
    /// <summary>
    /// API controller for managing book reservation operations.
    /// Handles creating, canceling, fulfilling reservations, and retrieving reservation records.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationsController"/> class.
        /// </summary>
        /// <param name="reservationService">The reservation service for handling business logic.</param>
        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        /// <summary>
        /// Creates a new reservation for a book.
        /// Members can only create reservations for themselves; librarians can create for any user.
        /// </summary>
        /// <param name="request">The create reservation request containing book and user information.</param>
        /// <returns>A <see cref="ReservationResponse"/> indicating success or failure with reservation details.</returns>
        /// <response code="200">Reservation created successfully.</response>
        /// <response code="400">Invalid request or business rule violation (e.g., book not found, already reserved, copies available).</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReservationResponse>> CreateReservation([FromBody] CreateReservationRequest request)
        {
            var currentUserId = GetCurrentUserId();
            
            // Members can only create reservations for themselves
            var userRole = GetCurrentUserRole();
            if (userRole == "Member")
            {
                request.UserId = currentUserId;
            }

            var result = await _reservationService.CreateReservationAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Cancels an existing reservation.
        /// Users can only cancel their own reservations.
        /// </summary>
        /// <param name="id">The reservation ID to cancel.</param>
        /// <returns>A <see cref="ReservationResponse"/> indicating success or failure.</returns>
        /// <response code="200">Reservation canceled successfully.</response>
        /// <response code="400">Invalid request or reservation cannot be canceled (e.g., not found, already fulfilled).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not own this reservation.</response>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ReservationResponse>> CancelReservation(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _reservationService.CancelReservationAsync(id, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Fulfills a reservation by converting it into a lending transaction.
        /// Only librarians can fulfill reservations.
        /// </summary>
        /// <param name="request">The fulfill reservation request containing reservation ID and loan duration.</param>
        /// <returns>A <see cref="ReservationResponse"/> indicating success or failure with updated reservation details.</returns>
        /// <response code="200">Reservation fulfilled successfully and converted to a loan.</response>
        /// <response code="400">Invalid request or reservation cannot be fulfilled (e.g., not found, invalid status).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have librarian permissions.</response>
        [HttpPost("fulfill")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<ActionResult<ReservationResponse>> FulfillReservation([FromBody] FulfillReservationRequest request)
        {
            var result = await _reservationService.FulfillReservationAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of the current user's reservations.
        /// </summary>
        /// <param name="page">The page number to retrieve (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <returns>A <see cref="PagedReservationsResponse"/> containing the user's reservations.</returns>
        /// <response code="200">User reservations retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet("my-reservations")]
        [Authorize]
        public async Task<ActionResult<PagedReservationsResponse>> GetMyReservations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var result = await _reservationService.GetUserReservationsAsync(userId, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of all reservations in the system.
        /// Only librarians can access this endpoint.
        /// </summary>
        /// <param name="page">The page number to retrieve (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <returns>A <see cref="PagedReservationsResponse"/> containing all reservations.</returns>
        /// <response code="200">All reservations retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have librarian permissions.</response>
        [HttpGet]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<ActionResult<PagedReservationsResponse>> GetAllReservations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _reservationService.GetAllReservationsAsync(page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves detailed information about a specific reservation by ID.
        /// Members can only view their own reservations.
        /// </summary>
        /// <param name="id">The reservation ID.</param>
        /// <returns>A <see cref="ReservationDto"/> containing the reservation details.</returns>
        /// <response code="200">Reservation retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have permission to view this reservation.</response>
        /// <response code="404">Reservation not found.</response>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReservationDto>> GetReservation(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            
            if (reservation == null)
            {
                return NotFound(new { message = "Reservation not found" });
            }

            // Members can only see their own reservations
            var userRole = GetCurrentUserRole();
            var currentUserId = GetCurrentUserId();
            if (userRole == "Member" && reservation.UserId != currentUserId)
            {
                return Forbid();
            }

            return Ok(reservation);
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
