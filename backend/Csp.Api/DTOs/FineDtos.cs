namespace Csp.Api.DTOs
{
    /// <summary>
    /// Data transfer object representing a fine with related book and user information.
    /// </summary>
    public class FineDto
    {
        public int Id { get; set; }
        public int? LendingId { get; set; }
        public int UserId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string MemberEmail { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DateTime OverdueDate { get; set; }
        public int DaysOverdue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Data transfer object representing a payment transaction.
    /// </summary>
    public class PaymentDto
    {
        public int Id { get; set; }
        public int FineId { get; set; }
        public int UserId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string MemberEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }

    /// <summary>
    /// Request to process a fine payment.
    /// </summary>
    public class ProcessPaymentRequest
    {
        public int FineId { get; set; }
        public int UserId { get; set; }
        public string? TransactionId { get; set; }
    }

    /// <summary>
    /// Request to waive a fine.
    /// </summary>
    public class WaiveFineRequest
    {
        public int FineId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to adjust fine amount.
    /// </summary>
    public class AdjustFineAmountRequest
    {
        public int FineId { get; set; }
        public decimal NewAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response for fine operations.
    /// </summary>
    public class FineResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public FineDto? Fine { get; set; }
    }

    /// <summary>
    /// Response for payment operations.
    /// </summary>
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PaymentDto? Payment { get; set; }
    }

    /// <summary>
    /// Paginated response for fines.
    /// </summary>
    public class PagedFinesResponse
    {
        public List<FineDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// Paginated response for payments.
    /// </summary>
    public class PagedPaymentsResponse
    {
        public List<PaymentDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// Fine statistics for a user or system.
    /// </summary>
    public class FineStatistics
    {
        public decimal TotalOutstanding { get; set; }
        public int OutstandingCount { get; set; }
        public decimal TotalPaid { get; set; }
        public int PaidCount { get; set; }
        public decimal TotalWaived { get; set; }
        public int WaivedCount { get; set; }
    }
}
