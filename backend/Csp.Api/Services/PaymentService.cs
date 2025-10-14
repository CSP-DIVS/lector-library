using MySql.Data.MySqlClient;
using Csp.Api.Models;
using Csp.Api.DTOs;
using System.Text.Json;

namespace Csp.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly string _connectionString;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IConfiguration configuration, ILogger<PaymentService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentException("Database connection string is required");
            _logger = logger;
        }

        /// <summary>
        /// Records a payment for a fine and updates the lending balance
        /// </summary>
        public async Task<PaymentResponse> RecordPaymentAsync(RecordPaymentRequest request, int recordedByUserId)
        {
            if (request == null) 
                throw new ArgumentNullException(nameof(request));

            try
            {
                // Validate request
                var validationResult = await ValidatePaymentRequestAsync(request);
                if (!validationResult.IsValid)
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        Message = validationResult.ErrorMessage
                    };
                }

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();

                try
                {
                    // Get current lending details with lock to prevent concurrent updates
                    var lending = await GetLendingWithLockAsync(connection, transaction, request.LendingId);
                    if (lending == null)
                    {
                        return new PaymentResponse
                        {
                            Success = false,
                            Message = "Lending record not found"
                        };
                    }

            // Verify member matches
            if (lending.UserId != request.MemberId)
            {
                return new PaymentResponse
                {
                    Success = false,
                    Message = "Member ID does not match the lending record"
                };
            }                    // Check if payment amount exceeds outstanding balance
                    var currentFineAmount = lending.FineAmount ?? 0;
                    if (request.Amount > currentFineAmount)
                    {
                        return new PaymentResponse
                        {
                            Success = false,
                            Message = $"Payment amount (${request.Amount:F2}) cannot exceed outstanding balance (${currentFineAmount:F2})"
                        };
                    }

                    // Insert payment record
                    var paymentDate = request.PaymentDate ?? DateTime.UtcNow;
                    var payment = await InsertPaymentAsync(connection, transaction, request, recordedByUserId, paymentDate);

                    // Update lending fine amount
                    var newFineAmount = currentFineAmount - request.Amount;
                    await UpdateLendingFineAmountAsync(connection, transaction, request.LendingId, newFineAmount);

                    // Log audit trail
                    LogPaymentAudit(connection, transaction, payment, lending, recordedByUserId);

                    await transaction.CommitAsync();

                    // Get recorded by user name for response
                    var recordedByName = await GetUserNameAsync(recordedByUserId);

                    _logger.LogInformation("Payment recorded successfully. PaymentId: {PaymentId}, Amount: {Amount}, MemberId: {MemberId}, LendingId: {LendingId}", 
                        payment.Id, request.Amount, request.MemberId, request.LendingId);

                    return new PaymentResponse
                    {
                        Success = true,
                        Message = "Payment recorded successfully",
                        Payment = new PaymentDto
                        {
                            Id = payment.Id,
                            LendingId = payment.LendingId,
                            MemberId = payment.MemberId,
                            Amount = payment.Amount,
                            PaymentMethod = payment.PaymentMethod,
                            PaymentDate = payment.PaymentDate,
                            RecordedBy = payment.RecordedBy,
                            RecordedByName = recordedByName,
                            CreatedAt = payment.CreatedAt
                        },
                        RemainingBalance = newFineAmount
                    };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording payment. MemberId: {MemberId}, LendingId: {LendingId}, Amount: {Amount}", 
                    request.MemberId, request.LendingId, request.Amount);
                
                return new PaymentResponse
                {
                    Success = false,
                    Message = "An error occurred while recording the payment. Please try again."
                };
            }
        }

        /// <summary>
        /// Gets payment history based on filters
        /// </summary>
        public async Task<PaymentHistoryResponse> GetPaymentHistoryAsync(GetPaymentHistoryRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var whereConditions = new List<string>();
                var parameters = new List<MySqlParameter>();

                // Build WHERE clause
                if (request.MemberId.HasValue)
                {
                    whereConditions.Add("p.MemberId = @memberId");
                    parameters.Add(new MySqlParameter("@memberId", request.MemberId.Value));
                }

                if (request.LendingId.HasValue)
                {
                    whereConditions.Add("p.LendingId = @lendingId");
                    parameters.Add(new MySqlParameter("@lendingId", request.LendingId.Value));
                }

                if (request.FromDate.HasValue)
                {
                    whereConditions.Add("p.PaymentDate >= @fromDate");
                    parameters.Add(new MySqlParameter("@fromDate", request.FromDate.Value));
                }

                if (request.ToDate.HasValue)
                {
                    whereConditions.Add("p.PaymentDate <= @toDate");
                    parameters.Add(new MySqlParameter("@toDate", request.ToDate.Value.AddDays(1))); // Include full day
                }

                var whereClause = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";

                // Get total count
                var countQuery = $@"
                    SELECT COUNT(*) 
                    FROM payments p 
                    {whereClause}";

                using var countCommand = new MySqlCommand(countQuery, connection);
                countCommand.Parameters.AddRange(parameters.ToArray());
                var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

                // Get paginated results
                var offset = (request.Page - 1) * request.PageSize;
                var query = $@"
                    SELECT p.Id, p.LendingId, p.MemberId, p.Amount, p.PaymentMethod, 
                           p.PaymentDate, p.RecordedBy, p.CreatedAt,
                           u.Username as RecordedByName
                    FROM payments p
                    LEFT JOIN users u ON p.RecordedBy = u.Id
                    {whereClause}
                    ORDER BY p.PaymentDate DESC, p.CreatedAt DESC
                    LIMIT @pageSize OFFSET @offset";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddRange(parameters.ToArray());
                command.Parameters.AddWithValue("@pageSize", request.PageSize);
                command.Parameters.AddWithValue("@offset", offset);

                var payments = new List<PaymentDto>();
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    payments.Add(new PaymentDto
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        LendingId = reader.GetInt32(reader.GetOrdinal("LendingId")),
                        MemberId = reader.GetInt32(reader.GetOrdinal("MemberId")),
                        Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                        PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate")),
                        RecordedBy = reader.GetInt32(reader.GetOrdinal("RecordedBy")),
                        RecordedByName = reader.IsDBNull(reader.GetOrdinal("RecordedByName")) ? "" : reader.GetString(reader.GetOrdinal("RecordedByName")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }

                return new PaymentHistoryResponse
                {
                    Payments = payments,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    HasNextPage = (request.Page * request.PageSize) < totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment history");
                throw;
            }
        }

        /// <summary>
        /// Gets the total amount paid for a specific lending
        /// </summary>
        public async Task<decimal> GetTotalPaidAmountAsync(int lendingId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT COALESCE(SUM(Amount), 0) FROM payments WHERE LendingId = @lendingId";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@lendingId", lendingId);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToDecimal(result ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total paid amount for lending {LendingId}", lendingId);
                throw;
            }
        }

        #region Private Methods

        private Task<(bool IsValid, string ErrorMessage)> ValidatePaymentRequestAsync(RecordPaymentRequest request)
        {
            if (request.Amount <= 0)
                return Task.FromResult((false, "Payment amount must be greater than zero"));

            if (string.IsNullOrWhiteSpace(request.PaymentMethod))
                return Task.FromResult((false, "Payment method is required"));

            // Validate payment method
            var validMethods = new[] { "Cash", "Credit Card", "Debit Card", "Bank Transfer", "Check" };
            if (!validMethods.Contains(request.PaymentMethod))
                return Task.FromResult((false, $"Invalid payment method. Valid methods are: {string.Join(", ", validMethods)}"));

            // Validate payment date
            if (request.PaymentDate.HasValue && request.PaymentDate.Value > DateTime.UtcNow)
                return Task.FromResult((false, "Payment date cannot be in the future"));

            return Task.FromResult((true, string.Empty));
        }

        private async Task<Models.Lending?> GetLendingWithLockAsync(MySqlConnection connection, MySqlTransaction transaction, int lendingId)
        {
            var query = @"
                SELECT Id, UserId, BookId, BorrowDate, DueDate, ReturnDate, FineAmount, Status
                FROM lendings 
                WHERE Id = @lendingId 
                FOR UPDATE";

            using var command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@lendingId", lendingId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Models.Lending
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    BookId = reader.GetInt32(reader.GetOrdinal("BookId")),
                    BorrowDate = reader.GetDateTime(reader.GetOrdinal("BorrowDate")),
                    DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                    ReturnDate = reader.IsDBNull(reader.GetOrdinal("ReturnDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ReturnDate")),
                    FineAmount = reader.IsDBNull(reader.GetOrdinal("FineAmount")) ? null : reader.GetDecimal(reader.GetOrdinal("FineAmount")),
                    Status = reader.GetString(reader.GetOrdinal("Status"))
                };
            }

            return null;
        }

        private async Task<Payment> InsertPaymentAsync(MySqlConnection connection, MySqlTransaction transaction, 
            RecordPaymentRequest request, int recordedByUserId, DateTime paymentDate)
        {
            var query = @"
                INSERT INTO payments (LendingId, MemberId, Amount, PaymentMethod, PaymentDate, RecordedBy, CreatedAt)
                VALUES (@lendingId, @memberId, @amount, @paymentMethod, @paymentDate, @recordedBy, @createdAt);
                SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@lendingId", request.LendingId);
            command.Parameters.AddWithValue("@memberId", request.MemberId);
            command.Parameters.AddWithValue("@amount", request.Amount);
            command.Parameters.AddWithValue("@paymentMethod", request.PaymentMethod);
            command.Parameters.AddWithValue("@paymentDate", paymentDate);
            command.Parameters.AddWithValue("@recordedBy", recordedByUserId);
            command.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);

            var paymentId = Convert.ToInt32(await command.ExecuteScalarAsync());

            return new Payment
            {
                Id = paymentId,
                LendingId = request.LendingId,
                MemberId = request.MemberId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                PaymentDate = paymentDate,
                RecordedBy = recordedByUserId,
                CreatedAt = DateTime.UtcNow
            };
        }

        private async Task UpdateLendingFineAmountAsync(MySqlConnection connection, MySqlTransaction transaction, int lendingId, decimal newFineAmount)
        {
            var query = "UPDATE lendings SET FineAmount = @fineAmount WHERE Id = @lendingId";
            using var command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@fineAmount", newFineAmount);
            command.Parameters.AddWithValue("@lendingId", lendingId);

            await command.ExecuteNonQueryAsync();
        }

        private void LogPaymentAudit(MySqlConnection connection, MySqlTransaction transaction, Payment payment, Models.Lending lending, int recordedByUserId)
        {
            var auditData = new
            {
                PaymentId = payment.Id,
                LendingId = payment.LendingId,
                MemberId = payment.MemberId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaymentDate = payment.PaymentDate,
                PreviousFineAmount = lending.FineAmount + payment.Amount, // Original amount before payment
                NewFineAmount = lending.FineAmount,
                RecordedBy = recordedByUserId,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Payment audit trail: {AuditData}", JsonSerializer.Serialize(auditData));
        }

        private async Task<string> GetUserNameAsync(int userId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT Username FROM users WHERE Id = @userId";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", userId);

                var result = await command.ExecuteScalarAsync();
                return result?.ToString() ?? $"User {userId}";
            }
            catch
            {
                return $"User {userId}";
            }
        }

        #endregion
    }
}