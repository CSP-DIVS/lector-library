using Csp.Api.DTOs;

namespace Csp.Api.Services
{
    /// <summary>
    /// Interface for Payment Service operations
    /// Handles payment recording, validation, and retrieval functionality
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Records a payment against a lending fine
        /// </summary>
        /// <param name="request">Payment details including amount, method, lending ID, etc.</param>
        /// <param name="recordedByUserId">ID of the user recording the payment (Admin/Librarian)</param>
        /// <returns>Payment recording result with success/failure status</returns>
        Task<PaymentResponse> RecordPaymentAsync(RecordPaymentRequest request, int recordedByUserId);

        /// <summary>
        /// Retrieves payment history with optional filtering
        /// </summary>
        /// <param name="request">Request parameters for payment history retrieval</param>
        /// <returns>List of payment records with pagination info</returns>
        Task<PaymentHistoryResponse> GetPaymentHistoryAsync(GetPaymentHistoryRequest request);

        /// <summary>
        /// Gets the total amount paid for a specific lending
        /// </summary>
        /// <param name="lendingId">Lending ID to get total payments for</param>
        /// <returns>Total amount paid for the lending</returns>
        Task<decimal> GetTotalPaidAmountAsync(int lendingId);
    }
}