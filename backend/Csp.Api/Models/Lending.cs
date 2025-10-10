namespace Csp.Api.Models
{
    public class Lending
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Active"; // Active, Returned, Overdue
        public decimal? FineAmount { get; set; }
        public bool FinePaid { get; set; } = false;
        public int RenewalCount { get; set; } = 0;
        public int MaxRenewals { get; set; } = 2;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Reservation
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateTime ReservedDate { get; set; } = DateTime.UtcNow;
        public int QueuePosition { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Available, Fulfilled, Cancelled, Expired
        public DateTime? AvailableDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? FulfilledDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
