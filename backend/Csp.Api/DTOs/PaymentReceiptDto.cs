namespace Csp.Api.DTOs;

public class PaymentReceiptDto
{
    public int PaymentId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string RecordedByName { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string LibraryName { get; set; } = "Lector Library";
    public string LibraryAddress { get; set; } = "SLIIT Campus, Malabe, Sri Lanka";
}
