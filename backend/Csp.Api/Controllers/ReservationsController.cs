using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Csp.Api.Services;
using Csp.Api.DTOs;
using System.Security.Claims;

namespace Csp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

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

        [HttpGet]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<ActionResult<PagedReservationsResponse>> GetAllReservations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _reservationService.GetAllReservationsAsync(page, pageSize);
            return Ok(result);
        }

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
