using System;
using System.ComponentModel.DataAnnotations;

namespace Csp.Api.DTOs
{
    /// <summary>
    /// Request DTO for recording a payment
    /// </summary>
    public class RecordPaymentRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Member ID is required")]
        public int MemberId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Lending ID is required")]
        public int LendingId { get; set; }

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Payment amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Payment method is required")]
        public string PaymentMethod { get; set; } = "Cash";

        public DateTime? PaymentDate { get; set; }
    }

    /// <summary>
    /// Response DTO for payment recording result
    /// </summary>
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PaymentDto? Payment { get; set; }
        public decimal RemainingBalance { get; set; }
    }

    /// <summary>
    /// DTO for payment information
    /// </summary>
    public class PaymentDto
    {
        public int Id { get; set; }
        public int LendingId { get; set; }
        public int MemberId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public int RecordedBy { get; set; }
        public string RecordedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Request DTO for getting payment history
    /// </summary>
    public class GetPaymentHistoryRequest
    {
        public int? MemberId { get; set; }
        public int? LendingId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Response DTO for payment history
    /// </summary>
    public class PaymentHistoryResponse
    {
        public List<PaymentDto> Payments { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
    }
}