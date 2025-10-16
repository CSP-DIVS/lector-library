using Csp.Api.DTOs;

namespace Csp.Api.Services;

public interface IReceiptService
{
    /// <summary>
    /// Retrieves payment receipt data by payment ID
    /// </summary>
    /// <param name="paymentId">The ID of the payment</param>
    /// <returns>Receipt data if found, null otherwise</returns>
    Task<PaymentReceiptDto?> GetReceiptDataAsync(int paymentId);

    /// <summary>
    /// Generates a PDF receipt from receipt data
    /// </summary>
    /// <param name="data">The receipt data</param>
    /// <returns>PDF file as byte array</returns>
    Task<byte[]> GenerateReceiptPdfAsync(PaymentReceiptDto data);
}
