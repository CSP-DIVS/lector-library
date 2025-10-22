namespace Csp.Api.Models
{
    /// <summary>
    /// Represents a fine issued for overdue books or other library violations.
    /// </summary>
    public class Fine
    {
        /// <summary>
        /// Gets or sets the unique identifier for the fine.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the lending ID associated with this fine.
        /// </summary>
        public int? LendingId { get; set; }

        /// <summary>
        /// Gets or sets the user ID who owes the fine.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the book ID associated with this fine.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the reason for the fine.
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the fine amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the status of the fine (Outstanding, Paid, Waived).
        /// </summary>
        public string Status { get; set; } = "Outstanding";

        /// <summary>
        /// Gets or sets the due date of the book when fine was incurred.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets when the book became overdue.
        /// </summary>
        public DateTime OverdueDate { get; set; }

        /// <summary>
        /// Gets or sets the number of days overdue.
        /// </summary>
        public int DaysOverdue { get; set; }

        /// <summary>
        /// Gets or sets when the fine was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets when the fine was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
