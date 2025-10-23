namespace Csp.Api.Models
{
    /// <summary>
    /// Represents a payment transaction for a fine.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Gets or sets the unique identifier for the payment.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the fine ID this payment is for.
        /// </summary>
        public int FineId { get; set; }

        /// <summary>
        /// Gets or sets the user ID who made the payment.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the payment amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the payment method (Cash, Credit Card, Debit Card, Online).
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transaction ID from payment gateway.
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when the payment was made.
        /// </summary>
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the status of the payment (Completed, Pending, Failed, Refunded).
        /// </summary>
        public string Status { get; set; } = "Completed";
    }
}
