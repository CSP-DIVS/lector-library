using Csp.Api.Models;
using Csp.Api.DTOs;
using Csp.Api.Data;
using MySql.Data.MySqlClient;

namespace Csp.Api.Services
{
    public interface ILendingService
    {
        Task<LendingResponse> BorrowBookAsync(BorrowBookRequest request);
        Task<LendingResponse> ReturnBookAsync(ReturnBookRequest request);
        Task<LendingResponse> RenewLoanAsync(RenewLoanRequest request, int userId);
        Task<PagedLendingsResponse> GetActiveLoansAsync(int? userId = null, int page = 1, int pageSize = 10);
        Task<PagedLendingsResponse> GetLoanHistoryAsync(int? userId = null, int page = 1, int pageSize = 10);
        Task<LendingDto?> GetLendingByIdAsync(int id);
        Task InitializeLendingTablesAsync();
        Task<AdjustFineResponse> AdjustFineAsync(int lendingId, AdjustFineRequest request, int adminUserId);
    }

    public class LendingService : ILendingService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private const decimal FINE_PER_DAY = 1.0m;

        public LendingService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                                 ?? _configuration.GetConnectionString("Default")
                                 ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task InitializeLendingTablesAsync()
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var createLendingsTableSql = SqlQueryLoader.LoadQuery("Lendings", "CreateLendingsTable");

            await using var cmd = new MySqlCommand(createLendingsTableSql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<AdjustFineResponse> AdjustFineAsync(int lendingId, AdjustFineRequest request, int adminUserId)
        {
            // Validate
            if (lendingId <= 0)
                return new AdjustFineResponse { Success = false, Message = "Invalid lending id" };
            if (request.NewAmount < 0)
                return new AdjustFineResponse { Success = false, Message = "New amount must be >= 0" };
            if (string.IsNullOrWhiteSpace(request.Reason))
                return new AdjustFineResponse { Success = false, Message = "A reason is required for all fine adjustments." };

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                // Get current fine and userId for audit target
                var getSql = SqlQueryLoader.LoadQuery("Lendings", "GetFineInfoForAdjustment");
                await using var getCmd = new MySqlCommand(getSql, conn, tx);
                getCmd.Parameters.AddWithValue("@LendingId", lendingId);
                await using var r = await getCmd.ExecuteReaderAsync();
                if (!await r.ReadAsync())
                {
                    await r.CloseAsync();
                    await tx.RollbackAsync();
                    return new AdjustFineResponse { Success = false, Message = "Lending record not found" };
                }
                var currentFine = r.IsDBNull(0) ? (decimal?)null : r.GetDecimal(0);
                var memberUserId = r.GetInt32(1);
                await r.CloseAsync();

                var original = currentFine ?? 0m;

                // Update fine and paid flag
                var updSql = SqlQueryLoader.LoadQuery("Lendings", "AdjustFineAmount");
                await using var updCmd = new MySqlCommand(updSql, conn, tx);
                updCmd.Parameters.AddWithValue("@LendingId", lendingId);
                updCmd.Parameters.AddWithValue("@NewAmount", request.NewAmount);
                updCmd.Parameters.AddWithValue("@FinePaid", request.NewAmount == 0 ? 1 : 0);
                updCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                var rows = await updCmd.ExecuteNonQueryAsync();
                if (rows == 0)
                {
                    await tx.RollbackAsync();
                    return new AdjustFineResponse { Success = false, Message = "Failed to adjust fine" };
                }

                // Audit
                var auditSql = SqlQueryLoader.LoadQuery("Users", "InsertAuditLog");
                await using var auditCmd = new MySqlCommand(auditSql, conn, tx);
                auditCmd.Parameters.AddWithValue("@ActorUserId", adminUserId);
                auditCmd.Parameters.AddWithValue("@Action", request.NewAmount == 0 ? "WaiveFine" : "AdjustFine");
                auditCmd.Parameters.AddWithValue("@TargetUserId", memberUserId);
                var details = $"lendingId={lendingId}; original={original:F2}; new={request.NewAmount:F2}; reason={request.Reason}";
                auditCmd.Parameters.AddWithValue("@Details", details);
                await auditCmd.ExecuteNonQueryAsync();

                await tx.CommitAsync();

                var lending = await GetLendingByIdAsync(lendingId);
                return new AdjustFineResponse
                {
                    Success = true,
                    Message = request.NewAmount == 0 ? "Fine waived successfully" : "Fine updated successfully",
                    LendingId = lendingId,
                    OriginalAmount = original,
                    NewAmount = request.NewAmount,
                    FinePaid = request.NewAmount == 0,
                    Lending = lending
                };
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return new AdjustFineResponse { Success = false, Message = $"Error adjusting fine: {ex.Message}" };
            }
        }

        public async Task<LendingResponse> BorrowBookAsync(BorrowBookRequest request)
        {
            if (request.BookId <= 0 || request.UserId <= 0)
            {
                return new LendingResponse
                {
                    Success = false,
                    Message = "Invalid BookId or UserId"
                };
            }

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Check if book exists and is active
                var checkBookSql = SqlQueryLoader.LoadQuery("Lendings", "CheckBookAvailability");
                
                await using var bookCmd = new MySqlCommand(checkBookSql, conn, transaction);
                bookCmd.Parameters.AddWithValue("@BookId", request.BookId);
                
                await using var reader = await bookCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    await reader.CloseAsync();
                    await transaction.RollbackAsync();
                    return new LendingResponse
                    {
                        Success = false,
                        Message = "Book not found"
                    };
                }

                var isActive = reader.GetBoolean(1);
                var availableCopies = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                await reader.CloseAsync();

                if (!isActive)
                {
                    await transaction.RollbackAsync();
                    return new LendingResponse
                    {
                        Success = false,
                        Message = "Book is not active"
                    };
                }

                if (availableCopies <= 0)
                {
                    await transaction.RollbackAsync();
                    return new LendingResponse
                    {
                        Success = false,
                        Message = "No copies available for borrowing"
                    };
                }

                // Check if user already has an active loan for this book
                var checkExistingLoanSql = SqlQueryLoader.LoadQuery("Lendings", "CheckExistingLoan");
                
                await using var checkLoanCmd = new MySqlCommand(checkExistingLoanSql, conn, transaction);
                checkLoanCmd.Parameters.AddWithValue("@BookId", request.BookId);
                checkLoanCmd.Parameters.AddWithValue("@UserId", request.UserId);
                
                var existingLoans = Convert.ToInt32(await checkLoanCmd.ExecuteScalarAsync());
                if (existingLoans > 0)
                {
                    await transaction.RollbackAsync();
                    return new LendingResponse
                    {
                        Success = false,
                        Message = "User already has an active loan for this book"
                    };
                }

                // Create lending record
                var dueDate = DateTime.UtcNow.AddDays(request.LoanDurationDays);
                var insertLendingSql = SqlQueryLoader.LoadQuery("Lendings", "InsertLending");
                
                await using var lendingCmd = new MySqlCommand(insertLendingSql, conn, transaction);
                lendingCmd.Parameters.AddWithValue("@BookId", request.BookId);
                lendingCmd.Parameters.AddWithValue("@UserId", request.UserId);
                lendingCmd.Parameters.AddWithValue("@BorrowDate", DateTime.UtcNow);
                lendingCmd.Parameters.AddWithValue("@DueDate", dueDate);
                
                var lendingId = Convert.ToInt32(await lendingCmd.ExecuteScalarAsync());

                // Update available copies
                var updateInventorySql = SqlQueryLoader.LoadQuery("Lendings", "DecrementAvailableCopies");
                
                await using var inventoryCmd = new MySqlCommand(updateInventorySql, conn, transaction);
                inventoryCmd.Parameters.AddWithValue("@BookId", request.BookId);
                inventoryCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                await inventoryCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                var lending = await GetLendingByIdAsync(lendingId);
                return new LendingResponse
                {
                    Success = true,
                    Message = "Book borrowed successfully",
                    Lending = lending
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new LendingResponse
                {
                    Success = false,
                    Message = $"Error borrowing book: {ex.Message}"
                };
            }
        }

        public async Task<LendingResponse> ReturnBookAsync(ReturnBookRequest request)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Get lending details
                var getLendingSql = SqlQueryLoader.LoadQuery("Lendings", "GetLendingForReturn");
                
                await using var lendingCmd = new MySqlCommand(getLendingSql, conn, transaction);
                lendingCmd.Parameters.AddWithValue("@LendingId", request.LendingId);
                
                await using var reader = await lendingCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    await reader.CloseAsync();
                    await transaction.RollbackAsync();
                    return new LendingResponse
                    {
                        Success = false,
                        Message = "Lending record not found"
                    };
                }

                var bookId = reader.GetInt32(0);
                var userId = reader.GetInt32(1);
                var dueDate = reader.GetDateTime(2);
                var status = reader.GetString(3);
                await reader.CloseAsync();

                if (status != "Active" && status != "Overdue")
                {
                    await transaction.RollbackAsync();
                    return new LendingResponse
                    {
                        Success = false,
                        Message = "Book is not currently on loan"
                    };
                }

                // Calculate fine if overdue
                var returnDate = DateTime.UtcNow;
                decimal? fineAmount = null;
                if (returnDate > dueDate)
                {
                    var overdueDays = (returnDate - dueDate).Days;
                    fineAmount = overdueDays * FINE_PER_DAY;
                }

                // Update lending record
                var updateLendingSql = SqlQueryLoader.LoadQuery("Lendings", "UpdateLendingReturn");
                
                await using var updateCmd = new MySqlCommand(updateLendingSql, conn, transaction);
                updateCmd.Parameters.AddWithValue("@LendingId", request.LendingId);
                updateCmd.Parameters.AddWithValue("@ReturnDate", returnDate);
                updateCmd.Parameters.AddWithValue("@FineAmount", (object?)fineAmount ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                await updateCmd.ExecuteNonQueryAsync();

                // Update available copies
                var updateInventorySql = SqlQueryLoader.LoadQuery("Lendings", "IncrementAvailableCopies");
                
                await using var inventoryCmd = new MySqlCommand(updateInventorySql, conn, transaction);
                inventoryCmd.Parameters.AddWithValue("@BookId", bookId);
                inventoryCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                await inventoryCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                var lending = await GetLendingByIdAsync(request.LendingId);
                return new LendingResponse
                {
                    Success = true,
                    Message = fineAmount.HasValue 
                        ? $"Book returned. Fine of ${fineAmount.Value:F2} applies for overdue days."
                        : "Book returned successfully",
                    Lending = lending
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new LendingResponse
                {
                    Success = false,
                    Message = $"Error returning book: {ex.Message}"
                };
            }
        }

        public async Task<LendingResponse> RenewLoanAsync(RenewLoanRequest request, int userId)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Get lending details
                var getLendingSql = SqlQueryLoader.LoadQuery("Lendings", "GetLendingForRenewal");
                
                await using var lendingCmd = new MySqlCommand(getLendingSql, conn, transaction);
                lendingCmd.Parameters.AddWithValue("@LendingId", request.LendingId);
                
                await using var reader = await lendingCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    await reader.CloseAsync();
                    await transaction.RollbackAsync();
                    return new LendingResponse
                    {
                        Success = false,
                        Message = "Lending record not found"
                    };
                }

                var bookId = reader.GetInt32(0);
                var lendingUserId = reader.GetInt32(1);
                var currentDueDate = reader.GetDateTime(2);
                var status = reader.GetString(3);
                var renewalCount = reader.GetInt32(4);
                var maxRenewals = reader.GetInt32(5);
                await reader.CloseAsync();

                // Verify user owns this lending
                if (lendingUserId != userId)
                {
                    await transaction.RollbackAsync();
                    return new LendingResponse
                    {
                        Success = false,
                        Message = "You are not authorized to renew this loan"
                    };
                }

                if (status != "Active")
                {
                    await transaction.RollbackAsync();
                    return new LendingResponse
                    {
                        Success = false,
                        Message = "Only active loans can be renewed"
                    };
                }

                if (renewalCount >= maxRenewals)
                {
                    await transaction.RollbackAsync();
                    return new LendingResponse
                    {
                        Success = false,
                        Message = $"Maximum renewal limit ({maxRenewals}) reached"
                    };
                }

                // Check if book has pending reservations
                var checkReservationsSql = SqlQueryLoader.LoadQuery("Lendings", "CheckPendingReservations");
                
                await using var reservationCmd = new MySqlCommand(checkReservationsSql, conn, transaction);
                reservationCmd.Parameters.AddWithValue("@BookId", bookId);
                
                var pendingReservations = Convert.ToInt32(await reservationCmd.ExecuteScalarAsync());
                if (pendingReservations > 0)
                {
                    await transaction.RollbackAsync();
                    return new LendingResponse
                    {
                        Success = false,
                        Message = "Cannot renew: book has pending reservations"
                    };
                }

                // Renew the loan (extend by 14 days from current due date)
                var newDueDate = currentDueDate.AddDays(14);
                var updateLendingSql = SqlQueryLoader.LoadQuery("Lendings", "UpdateLendingRenewal");
                
                await using var updateCmd = new MySqlCommand(updateLendingSql, conn, transaction);
                updateCmd.Parameters.AddWithValue("@LendingId", request.LendingId);
                updateCmd.Parameters.AddWithValue("@NewDueDate", newDueDate);
                updateCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                await updateCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                var lending = await GetLendingByIdAsync(request.LendingId);
                return new LendingResponse
                {
                    Success = true,
                    Message = $"Loan renewed successfully. New due date: {newDueDate:yyyy-MM-dd}",
                    Lending = lending
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new LendingResponse
                {
                    Success = false,
                    Message = $"Error renewing loan: {ex.Message}"
                };
            }
        }

        public async Task<PagedLendingsResponse> GetActiveLoansAsync(int? userId = null, int page = 1, int pageSize = 10)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Load SQL query based on whether userId is provided
            var queryName = userId.HasValue ? "GetActiveLoansWithUser" : "GetActiveLoans";
            var countQueryName = userId.HasValue ? "CountActiveLoansWithUser" : "CountActiveLoans";
            
            var sql = SqlQueryLoader.LoadQuery("Lendings", queryName);
            var countSql = SqlQueryLoader.LoadQuery("Lendings", countQueryName);

            var lendings = new List<LendingDto>();
            
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            if (userId.HasValue)
                cmd.Parameters.AddWithValue("@UserId", userId.Value);

            await using var readerActive = await cmd.ExecuteReaderAsync();
            var readerA = (MySqlDataReader)readerActive;
            while (await readerA.ReadAsync())
            {
                var lending = MapLendingDto(readerA);
                lendings.Add(lending);
            }
            await readerA.CloseAsync();

            // Get total count
            await using var countCmd = new MySqlCommand(countSql, conn);
            if (userId.HasValue)
                countCmd.Parameters.AddWithValue("@UserId", userId.Value);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            return new PagedLendingsResponse
            {
                Items = lendings,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedLendingsResponse> GetLoanHistoryAsync(int? userId = null, int page = 1, int pageSize = 10)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Load SQL query based on whether userId is provided
            var queryName = userId.HasValue ? "GetLoanHistoryWithUser" : "GetLoanHistory";
            var countQueryName = userId.HasValue ? "CountLoanHistoryWithUser" : "CountLoanHistory";
            
            var sql = SqlQueryLoader.LoadQuery("Lendings", queryName);
            var countSql = SqlQueryLoader.LoadQuery("Lendings", countQueryName);

            var lendings = new List<LendingDto>();
            
            await using var cmdHistory = new MySqlCommand(sql, conn);
            cmdHistory.Parameters.AddWithValue("@PageSize", pageSize);
            cmdHistory.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            if (userId.HasValue)
                cmdHistory.Parameters.AddWithValue("@UserId", userId.Value);

            await using var readerHistory = await cmdHistory.ExecuteReaderAsync();
            var reader2 = (MySqlDataReader)readerHistory;
            while (await reader2.ReadAsync())
            {
                var lending = MapLendingDto(reader2);
                lendings.Add(lending);
            }
            await reader2.CloseAsync();

            // Get total count
            await using var countCmd = new MySqlCommand(countSql, conn);
            if (userId.HasValue)
                countCmd.Parameters.AddWithValue("@UserId", userId.Value);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            return new PagedLendingsResponse
            {
                Items = lendings,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<LendingDto?> GetLendingByIdAsync(int id)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = SqlQueryLoader.LoadQuery("Lendings", "GetLendingById");

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await using var readerObj = await cmd.ExecuteReaderAsync();
            var reader = (MySqlDataReader)readerObj;
            if (await reader.ReadAsync())
            {
                return MapLendingDto(reader);
            }

            return null;
        }

        private LendingDto MapLendingDto(MySqlDataReader reader)
        {
            var isOverdue = reader.GetInt32(reader.GetOrdinal("IsOverdue")) == 1;
            var overdueDays = reader.GetInt32(reader.GetOrdinal("OverdueDays"));

            return new LendingDto
            {
                Id = reader.GetInt32("Id"),
                BookId = reader.GetInt32("BookId"),
                BookTitle = reader.GetString("Title"),
                BookAuthor = reader.GetString("Author"),
                BookIsbn = reader.GetString("Isbn"),
                UserId = reader.GetInt32("UserId"),
                Username = reader.GetString("Username"),
                MemberEmail = reader.GetString("Email"),
                BorrowDate = reader.GetDateTime("BorrowDate"),
                DueDate = reader.GetDateTime("DueDate"),
                ReturnDate = reader.IsDBNull(reader.GetOrdinal("ReturnDate")) 
                    ? null 
                    : reader.GetDateTime("ReturnDate"),
                Status = reader.GetString("Status"),
                FineAmount = reader.IsDBNull(reader.GetOrdinal("FineAmount")) 
                    ? null 
                    : reader.GetDecimal("FineAmount"),
                FinePaid = reader.GetBoolean("FinePaid"),
                RenewalCount = reader.GetInt32("RenewalCount"),
                MaxRenewals = reader.GetInt32("MaxRenewals"),
                IsOverdue = isOverdue,
                OverdueDays = isOverdue ? overdueDays : null
            };
        }
    }
}
