namespace Csp.Api.Models
{
    /// <summary>
    /// Represents a book lending transaction in the library system.
    /// Tracks the borrowing, return, and renewal of books by library members.
    /// </summary>
    public class Lending
    {
        /// <summary>
        /// Gets or sets the unique identifier for the lending transaction.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the book being borrowed.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who borrowed the book.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the book was borrowed.
        /// Defaults to the current UTC time when the lending is created.
        /// </summary>
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the book is due to be returned.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the book was actually returned.
        /// Null if the book has not been returned yet.
        /// </summary>
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Gets or sets the current status of the lending.
        /// Valid values: "Active", "Returned", "Overdue".
        /// Defaults to "Active" when the lending is created.
        /// </summary>
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Gets or sets the fine amount charged for overdue returns.
        /// Null if no fine is applicable.
        /// </summary>
        public decimal? FineAmount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the fine has been paid.
        /// Defaults to false.
        /// </summary>
        public bool FinePaid { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of times this lending has been renewed.
        /// Defaults to 0.
        /// </summary>
        public int RenewalCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum number of renewals allowed for this lending.
        /// Defaults to 2.
        /// </summary>
        public int MaxRenewals { get; set; } = 2;

        /// <summary>
        /// Gets or sets the date and time when the lending record was created.
        /// Defaults to the current UTC time.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the lending record was last updated.
        /// Defaults to the current UTC time and should be updated on any modification.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents a book reservation in the library system.
    /// Allows users to reserve books that are currently unavailable and tracks the reservation queue.
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// Gets or sets the unique identifier for the reservation.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the book being reserved.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who made the reservation.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the reservation was made.
        /// Defaults to the current UTC time when the reservation is created.
        /// </summary>
        public DateTime ReservedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the position of this reservation in the queue.
        /// Lower numbers indicate higher priority.
        /// </summary>
        public int QueuePosition { get; set; }

        /// <summary>
        /// Gets or sets the current status of the reservation.
        /// Valid values: "Pending", "Available", "Fulfilled", "Cancelled", "Expired".
        /// Defaults to "Pending" when the reservation is created.
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Gets or sets the date and time when the reserved book became available.
        /// Null if the book is not yet available.
        /// </summary>
        public DateTime? AvailableDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the reservation will expire if not fulfilled.
        /// Null if no expiry date is set.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the reservation was fulfilled.
        /// Null if the reservation has not been fulfilled yet.
        /// </summary>
        public DateTime? FulfilledDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the reservation record was created.
        /// Defaults to the current UTC time.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the reservation record was last updated.
        /// Defaults to the current UTC time and should be updated on any modification.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
