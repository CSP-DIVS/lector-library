using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Csp.Api.Services;
using Csp.Api.DTOs;

namespace Csp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all payment endpoints
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IReceiptService _receiptService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService, 
            IReceiptService receiptService,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Records a payment for a fine (Admin/Librarian only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "RequireLibrarian")] // Only librarians and administrators can record payments
        public async Task<ActionResult<PaymentResponse>> RecordPayment([FromBody] RecordPaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value?.Errors ?? [])
                                      .Select(x => x.ErrorMessage)
                                      .ToList();
                
                return BadRequest(new PaymentResponse
                {
                    Success = false,
                    Message = $"Validation failed: {string.Join(", ", errors)}"
                });
            }

            try
            {
                // Get the current user ID from the JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    _logger.LogWarning("Invalid or missing user ID in JWT token");
                    return Unauthorized(new PaymentResponse
                    {
                        Success = false,
                        Message = "Invalid authentication token"
                    });
                }

                _logger.LogInformation("Recording payment. Amount: {Amount}, MemberId: {MemberId}, LendingId: {LendingId}, RecordedBy: {RecordedBy}",
                    request.Amount, request.MemberId, request.LendingId, currentUserId);

                var result = await _paymentService.RecordPaymentAsync(request, currentUserId);

                if (result.Success)
                {
                    _logger.LogInformation("Payment recorded successfully. PaymentId: {PaymentId}, RemainingBalance: {RemainingBalance}",
                        result.Payment?.Id, result.RemainingBalance);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Payment recording failed. Reason: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error recording payment. MemberId: {MemberId}, LendingId: {LendingId}",
                    request.MemberId, request.LendingId);

                return StatusCode(500, new PaymentResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred while processing the payment"
                });
            }
        }

        /// <summary>
        /// Gets payment history with optional filtering (Admin/Librarian only)
        /// </summary>
        [HttpGet("history")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<ActionResult<PaymentHistoryResponse>> GetPaymentHistory(
            [FromQuery] int? memberId = null,
            [FromQuery] int? lendingId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1)
                {
                    return BadRequest(new { message = "Page number must be greater than 0" });
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(new { message = "Page size must be between 1 and 100" });
                }

                var request = new GetPaymentHistoryRequest
                {
                    MemberId = memberId,
                    LendingId = lendingId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Page = page,
                    PageSize = pageSize
                };

                _logger.LogInformation("Retrieving payment history. Filters - MemberId: {MemberId}, LendingId: {LendingId}, FromDate: {FromDate}, ToDate: {ToDate}, Page: {Page}, PageSize: {PageSize}",
                    memberId, lendingId, fromDate, toDate, page, pageSize);

                var result = await _paymentService.GetPaymentHistoryAsync(request);

                _logger.LogInformation("Payment history retrieved successfully. TotalCount: {TotalCount}, ReturnedCount: {ReturnedCount}",
                    result.TotalCount, result.Payments.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment history");
                return StatusCode(500, new { message = "An error occurred while retrieving payment history" });
            }
        }

        /// <summary>
        /// Gets the total amount paid for a specific lending (Admin/Librarian only)
        /// </summary>
        [HttpGet("total/{lendingId:int}")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<ActionResult<decimal>> GetTotalPaidAmount(int lendingId)
        {
            try
            {
                if (lendingId <= 0)
                {
                    return BadRequest(new { message = "Invalid lending ID" });
                }

                _logger.LogInformation("Getting total paid amount for lending {LendingId}", lendingId);

                var totalPaid = await _paymentService.GetTotalPaidAmountAsync(lendingId);

                _logger.LogInformation("Total paid amount retrieved for lending {LendingId}: {TotalPaid}", lendingId, totalPaid);

                return Ok(new { lendingId, totalPaid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total paid amount for lending {LendingId}", lendingId);
                return StatusCode(500, new { message = "An error occurred while retrieving payment information" });
            }
        }

        /// <summary>
        /// Gets payments for a specific member (Admin/Librarian only, or the member themselves)
        /// </summary>
        [HttpGet("member/{memberId:int}")]
        public async Task<ActionResult<PaymentHistoryResponse>> GetMemberPayments(
            int memberId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Get the current user ID from the JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userRoleClaim = User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Invalid authentication token" });
                }

                // Check authorization: Admin/Librarian can view any member's payments, members can only view their own
                var isAdminOrLibrarian = userRoleClaim?.Value == "Administrator" || userRoleClaim?.Value == "Librarian";
                if (!isAdminOrLibrarian && currentUserId != memberId)
                {
                    return Forbid("You can only view your own payment history");
                }

                // Validate parameters
                if (memberId <= 0)
                {
                    return BadRequest(new { message = "Invalid member ID" });
                }

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var request = new GetPaymentHistoryRequest
                {
                    MemberId = memberId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Page = page,
                    PageSize = pageSize
                };

                _logger.LogInformation("Retrieving payment history for member {MemberId}. RequestedBy: {RequestedBy}", 
                    memberId, currentUserId);

                var result = await _paymentService.GetPaymentHistoryAsync(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment history for member {MemberId}", memberId);
                return StatusCode(500, new { message = "An error occurred while retrieving payment history" });
            }
        }

        /// <summary>
        /// Health check endpoint for payment service
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new 
            { 
                service = "PaymentService",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Generates and downloads a PDF receipt for a payment (Admin/Librarian only)
        /// </summary>
        [HttpGet("{id:int}/receipt")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<IActionResult> GetPaymentReceipt(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid payment ID" });
                }

                _logger.LogInformation("Generating receipt for payment {PaymentId}", id);

                // Get receipt data
                var receiptData = await _receiptService.GetReceiptDataAsync(id);
                if (receiptData == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", id);
                    return NotFound(new { message = "Payment not found" });
                }

                // Generate PDF
                var pdfBytes = await _receiptService.GenerateReceiptPdfAsync(receiptData);

                _logger.LogInformation("Receipt generated successfully for payment {PaymentId}", id);

                // Return PDF file
                return File(pdfBytes, "application/pdf", $"receipt-{receiptData.TransactionId}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating receipt for payment {PaymentId}", id);
                return StatusCode(500, new { message = "An error occurred while generating the receipt" });
            }
        }
    }
}