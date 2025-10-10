namespace Csp.Api.DTOs
{
    // Lending DTOs
    public class LendingDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string BookIsbn { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string MemberEmail { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? FineAmount { get; set; }
        public bool FinePaid { get; set; }
        public int RenewalCount { get; set; }
        public int MaxRenewals { get; set; }
        public bool IsOverdue { get; set; }
        public int? OverdueDays { get; set; }
    }

    public class BorrowBookRequest
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
        public int LoanDurationDays { get; set; } = 14; // Default 14 days
    }

    public class RenewLoanRequest
    {
        public int LendingId { get; set; }
    }

    public class ReturnBookRequest
    {
        public int LendingId { get; set; }
        public decimal? FineAmount { get; set; }
    }

    public class LendingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public LendingDto? Lending { get; set; }
    }

    public class PagedLendingsResponse
    {
        public List<LendingDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    // Reservation DTOs
    public class ReservationDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string BookIsbn { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string MemberEmail { get; set; } = string.Empty;
        public DateTime ReservedDate { get; set; }
        public int QueuePosition { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? AvailableDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? FulfilledDate { get; set; }
        public string? EstimatedAvailable { get; set; }
    }

    public class CreateReservationRequest
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
    }

    public class FulfillReservationRequest
    {
        public int ReservationId { get; set; }
        public int LoanDurationDays { get; set; } = 14;
    }

    public class ReservationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ReservationDto? Reservation { get; set; }
    }

    public class PagedReservationsResponse
    {
        public List<ReservationDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
