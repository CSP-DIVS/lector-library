using Csp.Api.Models;
using Csp.Api.DTOs;
using Csp.Api.Data;
using MySql.Data.MySqlClient;

namespace Csp.Api.Services
{
    /// <summary>
    /// Interface defining the contract for fine and payment operations.
    /// </summary>
    public interface IFineService
    {
        /// <summary>
        /// Processes a payment for a fine.
        /// </summary>
        Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request);

        /// <summary>
        /// Waives a fine (admin/librarian only).
        /// </summary>
        Task<FineResponse> WaiveFineAsync(WaiveFineRequest request);

        /// <summary>
        /// Adjusts the fine amount (admin/librarian only).
        /// </summary>
        Task<FineResponse> AdjustFineAmountAsync(AdjustFineAmountRequest request);

        /// <summary>
        /// Gets all fines for a user.
        /// </summary>
        Task<PagedFinesResponse> GetUserFinesAsync(int userId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Gets all fines in the system.
        /// </summary>
        Task<PagedFinesResponse> GetAllFinesAsync(int page = 1, int pageSize = 10);

        /// <summary>
        /// Gets payment history for a user.
        /// </summary>
        Task<PagedPaymentsResponse> GetUserPaymentsAsync(int userId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Gets all payment history.
        /// </summary>
        Task<PagedPaymentsResponse> GetAllPaymentsAsync(int page = 1, int pageSize = 10);

        /// <summary>
        /// Gets fine statistics for a user.
        /// </summary>
        Task<FineStatistics> GetUserFineStatisticsAsync(int userId);

        /// <summary>
        /// Creates fines for overdue loans that don't have fines yet.
        /// </summary>
        Task<int> CreateFinesForOverdueLoansAsync();

        /// <summary>
        /// Initializes the fine and payment database tables.
        /// </summary>
        Task InitializeFineTablesAsync();
    }

    /// <summary>
    /// Service implementation for managing fines and payments.
    /// </summary>
    public class FineService : IFineService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private const decimal FINE_PER_DAY = 20.0m; // Rs. 20 per day

        public FineService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                                 ?? _configuration.GetConnectionString("Default")
                                 ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task InitializeFineTablesAsync()
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var createTablesSql = SqlQueryLoader.LoadQuery("Fines", "CreateFinesTables");
            await using var cmd = new MySqlCommand(createTablesSql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> CreateFinesForOverdueLoansAsync()
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = SqlQueryLoader.LoadQuery("Fines", "GetOverdueLoansWithoutFines");
            var insertSql = SqlQueryLoader.LoadQuery("Fines", "InsertFine");

            var overdueLoans = new List<(int LendingId, int UserId, int BookId, DateTime DueDate, int DaysOverdue, decimal FineAmount)>();

            await using (var cmd = new MySqlCommand(sql, conn))
            {
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    overdueLoans.Add((
                        reader.GetInt32(reader.GetOrdinal("LendingId")),
                        reader.GetInt32(reader.GetOrdinal("UserId")),
                        reader.GetInt32(reader.GetOrdinal("BookId")),
                        reader.GetDateTime(reader.GetOrdinal("DueDate")),
                        reader.GetInt32(reader.GetOrdinal("DaysOverdue")),
                        reader.GetDecimal(reader.GetOrdinal("FineAmount"))
                    ));
                }
            }

            int createdCount = 0;
            foreach (var loan in overdueLoans)
            {
                await using var insertCmd = new MySqlCommand(insertSql, conn);
                insertCmd.Parameters.AddWithValue("@LendingId", loan.LendingId);
                insertCmd.Parameters.AddWithValue("@UserId", loan.UserId);
                insertCmd.Parameters.AddWithValue("@BookId", loan.BookId);
                insertCmd.Parameters.AddWithValue("@Reason", "Overdue Book");
                insertCmd.Parameters.AddWithValue("@Amount", loan.FineAmount);
                insertCmd.Parameters.AddWithValue("@Status", "Outstanding");
                insertCmd.Parameters.AddWithValue("@DueDate", loan.DueDate);
                insertCmd.Parameters.AddWithValue("@OverdueDate", loan.DueDate.AddDays(1));
                insertCmd.Parameters.AddWithValue("@DaysOverdue", loan.DaysOverdue);
                insertCmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

                await insertCmd.ExecuteNonQueryAsync();
                
                // Update lending status to Overdue
                var updateLendingSql = "UPDATE Lendings SET Status = 'Overdue' WHERE Id = @LendingId AND Status = 'Active'";
                await using var updateCmd = new MySqlCommand(updateLendingSql, conn);
                updateCmd.Parameters.AddWithValue("@LendingId", loan.LendingId);
                await updateCmd.ExecuteNonQueryAsync();
                
                createdCount++;
            }

            return createdCount;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Get fine details
                var getFineQuery = SqlQueryLoader.LoadQuery("Fines", "GetFineById");
                await using var getCmd = new MySqlCommand(getFineQuery, conn, transaction);
                getCmd.Parameters.AddWithValue("@Id", request.FineId);

                FineDto? fine = null;
                await using (var reader = await getCmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        fine = MapFineDto((MySqlDataReader)reader);
                    }
                }

                if (fine == null)
                {
                    await transaction.RollbackAsync();
                    return new PaymentResponse
                    {
                        Success = false,
                        Message = "Fine not found"
                    };
                }

                if (fine.Status != "Outstanding")
                {
                    await transaction.RollbackAsync();
                    return new PaymentResponse
                    {
                        Success = false,
                        Message = "Fine is not outstanding"
                    };
                }

                // Create payment record
                var transactionId = request.TransactionId ?? $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{request.FineId}";
                var description = $"Payment for fine - {fine.BookTitle}";

                var insertPaymentSql = SqlQueryLoader.LoadQuery("Fines", "InsertPayment");
                await using var paymentCmd = new MySqlCommand(insertPaymentSql, conn, transaction);
                paymentCmd.Parameters.AddWithValue("@FineId", request.FineId);
                paymentCmd.Parameters.AddWithValue("@UserId", request.UserId);
                paymentCmd.Parameters.AddWithValue("@Amount", fine.Amount);
                paymentCmd.Parameters.AddWithValue("@PaymentMethod", "Cash");
                paymentCmd.Parameters.AddWithValue("@TransactionId", transactionId);
                paymentCmd.Parameters.AddWithValue("@Description", description);
                paymentCmd.Parameters.AddWithValue("@PaymentDate", DateTime.UtcNow);

                var paymentId = Convert.ToInt32(await paymentCmd.ExecuteScalarAsync());

                // Update fine status
                var updateFineSql = SqlQueryLoader.LoadQuery("Fines", "UpdateFineStatus");
                await using var updateCmd = new MySqlCommand(updateFineSql, conn, transaction);
                updateCmd.Parameters.AddWithValue("@Id", request.FineId);
                updateCmd.Parameters.AddWithValue("@Status", "Paid");
                updateCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                await updateCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                return new PaymentResponse
                {
                    Success = true,
                    Message = "Payment processed successfully",
                    Payment = new PaymentDto
                    {
                        Id = paymentId,
                        FineId = request.FineId,
                        UserId = request.UserId,
                        Amount = fine.Amount,
                        TransactionId = transactionId,
                        Description = description,
                        PaymentDate = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new PaymentResponse
                {
                    Success = false,
                    Message = $"Error processing payment: {ex.Message}"
                };
            }
        }

        public async Task<FineResponse> WaiveFineAsync(WaiveFineRequest request)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var updateSql = SqlQueryLoader.LoadQuery("Fines", "UpdateFineStatus");
            await using var cmd = new MySqlCommand(updateSql, conn);
            cmd.Parameters.AddWithValue("@Id", request.FineId);
            cmd.Parameters.AddWithValue("@Status", "Waived");
            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

            var rows = await cmd.ExecuteNonQueryAsync();

            if (rows > 0)
            {
                return new FineResponse
                {
                    Success = true,
                    Message = "Fine waived successfully"
                };
            }

            return new FineResponse
            {
                Success = false,
                Message = "Fine not found"
            };
        }

        public async Task<FineResponse> AdjustFineAmountAsync(AdjustFineAmountRequest request)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var updateSql = SqlQueryLoader.LoadQuery("Fines", "UpdateFineAmount");
            await using var cmd = new MySqlCommand(updateSql, conn);
            cmd.Parameters.AddWithValue("@Id", request.FineId);
            cmd.Parameters.AddWithValue("@NewAmount", request.NewAmount);
            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

            var rows = await cmd.ExecuteNonQueryAsync();

            if (rows > 0)
            {
                return new FineResponse
                {
                    Success = true,
                    Message = $"Fine amount adjusted to ₹{request.NewAmount:F2}"
                };
            }

            return new FineResponse
            {
                Success = false,
                Message = "Fine not found"
            };
        }

        public async Task<PagedFinesResponse> GetUserFinesAsync(int userId, int page = 1, int pageSize = 10)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = SqlQueryLoader.LoadQuery("Fines", "GetUserFines");
            var countSql = SqlQueryLoader.LoadQuery("Fines", "CountUserFines");

            var fines = new List<FineDto>();

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                fines.Add(MapFineDto((MySqlDataReader)reader));
            }

            await reader.CloseAsync();

            await using var countCmd = new MySqlCommand(countSql, conn);
            countCmd.Parameters.AddWithValue("@UserId", userId);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            return new PagedFinesResponse
            {
                Items = fines,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedFinesResponse> GetAllFinesAsync(int page = 1, int pageSize = 10)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = SqlQueryLoader.LoadQuery("Fines", "GetAllFines");
            var countSql = SqlQueryLoader.LoadQuery("Fines", "CountAllFines");

            var fines = new List<FineDto>();

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                fines.Add(MapFineDto((MySqlDataReader)reader));
            }

            await reader.CloseAsync();

            await using var countCmd = new MySqlCommand(countSql, conn);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            return new PagedFinesResponse
            {
                Items = fines,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedPaymentsResponse> GetUserPaymentsAsync(int userId, int page = 1, int pageSize = 10)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = SqlQueryLoader.LoadQuery("Fines", "GetUserPayments");
            var countSql = SqlQueryLoader.LoadQuery("Fines", "CountUserPayments");

            var payments = new List<PaymentDto>();

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                payments.Add(MapPaymentDto((MySqlDataReader)reader));
            }

            await reader.CloseAsync();

            await using var countCmd = new MySqlCommand(countSql, conn);
            countCmd.Parameters.AddWithValue("@UserId", userId);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            return new PagedPaymentsResponse
            {
                Items = payments,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedPaymentsResponse> GetAllPaymentsAsync(int page = 1, int pageSize = 10)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = SqlQueryLoader.LoadQuery("Fines", "GetAllPayments");
            var countSql = SqlQueryLoader.LoadQuery("Fines", "CountAllPayments");

            var payments = new List<PaymentDto>();

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                payments.Add(MapPaymentDto((MySqlDataReader)reader));
            }

            await reader.CloseAsync();

            await using var countCmd = new MySqlCommand(countSql, conn);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            return new PagedPaymentsResponse
            {
                Items = payments,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<FineStatistics> GetUserFineStatisticsAsync(int userId)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = SqlQueryLoader.LoadQuery("Fines", "GetUserFineStatistics");
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new FineStatistics
                {
                    TotalOutstanding = reader.IsDBNull(reader.GetOrdinal("TotalOutstanding")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalOutstanding")),
                    OutstandingCount = reader.IsDBNull(reader.GetOrdinal("OutstandingCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("OutstandingCount")),
                    TotalPaid = reader.IsDBNull(reader.GetOrdinal("TotalPaid")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalPaid")),
                    PaidCount = reader.IsDBNull(reader.GetOrdinal("PaidCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("PaidCount")),
                    TotalWaived = reader.IsDBNull(reader.GetOrdinal("TotalWaived")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalWaived")),
                    WaivedCount = reader.IsDBNull(reader.GetOrdinal("WaivedCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("WaivedCount"))
                };
            }

            return new FineStatistics();
        }

        private FineDto MapFineDto(MySqlDataReader reader)
        {
            return new FineDto
            {
                Id = reader.GetInt32("Id"),
                LendingId = reader.IsDBNull(reader.GetOrdinal("LendingId")) ? null : reader.GetInt32("LendingId"),
                UserId = reader.GetInt32("UserId"),
                MemberName = reader.GetString("MemberName"),
                MemberEmail = reader.GetString("MemberEmail"),
                BookId = reader.GetInt32("BookId"),
                BookTitle = reader.GetString("BookTitle"),
                BookAuthor = reader.GetString("BookAuthor"),
                Reason = reader.GetString("Reason"),
                Amount = reader.GetDecimal("Amount"),
                Status = reader.GetString("Status"),
                DueDate = reader.GetDateTime("DueDate"),
                OverdueDate = reader.GetDateTime("OverdueDate"),
                DaysOverdue = reader.GetInt32("DaysOverdue"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime("UpdatedAt")
            };
        }

        private PaymentDto MapPaymentDto(MySqlDataReader reader)
        {
            return new PaymentDto
            {
                Id = reader.GetInt32("Id"),
                FineId = reader.GetInt32("FineId"),
                UserId = reader.GetInt32("UserId"),
                MemberName = reader.GetString("MemberName"),
                MemberEmail = reader.GetString("MemberEmail"),
                Amount = reader.GetDecimal("Amount"),
                TransactionId = reader.GetString("TransactionId"),
                Description = reader.GetString("Description"),
                PaymentDate = reader.GetDateTime("PaymentDate")
            };
        }
    }
}
