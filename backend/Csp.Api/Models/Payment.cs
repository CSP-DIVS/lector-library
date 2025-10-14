using System;
using System.ComponentModel.DataAnnotations;

namespace Csp.Api.Models
{
    /// <summary>
    /// Represents a payment record for fine payments
    /// </summary>
    public class Payment
    {
        public int Id { get; set; }
        
        [Required]
        public int LendingId { get; set; }
        
        [Required]
        public int MemberId { get; set; }
        
        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Payment amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Cash";
        
        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        public int RecordedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties (if using EF Core)
        public virtual User? Member { get; set; }
        public virtual User? RecordedByUser { get; set; }
        public virtual Lending? Lending { get; set; }
    }
}