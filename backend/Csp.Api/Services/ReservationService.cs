using Csp.Api.Models;
using Csp.Api.DTOs;
using Csp.Api.Data;
using MySql.Data.MySqlClient;

namespace Csp.Api.Services
{
    public interface IReservationService
    {
        Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request);
        Task<ReservationResponse> CancelReservationAsync(int reservationId, int userId);
        Task<ReservationResponse> FulfillReservationAsync(FulfillReservationRequest request);
        Task<PagedReservationsResponse> GetUserReservationsAsync(int userId, int page = 1, int pageSize = 10);
        Task<PagedReservationsResponse> GetAllReservationsAsync(int page = 1, int pageSize = 10);
        Task<ReservationDto?> GetReservationByIdAsync(int id);
        Task InitializeReservationTablesAsync();
    }

    public class ReservationService : IReservationService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private const int RESERVATION_EXPIRY_DAYS = 3; // Days to pick up after becoming available

        public ReservationService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                                 ?? _configuration.GetConnectionString("Default")
                                 ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task InitializeReservationTablesAsync()
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var createReservationsTableSql = SqlQueryLoader.LoadQuery("Reservations", "CreateReservationsTable");

            await using var cmd = new MySqlCommand(createReservationsTableSql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request)
        {
            if (request.BookId <= 0 || request.UserId <= 0)
            {
                return new ReservationResponse
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
                // Check if book exists
                var checkBookSql = SqlQueryLoader.LoadQuery("Reservations", "CheckBookExists");
                await using var bookCmd = new MySqlCommand(checkBookSql, conn, transaction);
                bookCmd.Parameters.AddWithValue("@BookId", request.BookId);
                var bookExists = Convert.ToInt32(await bookCmd.ExecuteScalarAsync()) > 0;

                if (!bookExists)
                {
                    await transaction.RollbackAsync();
                    return new ReservationResponse
                    {
                        Success = false,
                        Message = "Book not found or not active"
                    };
                }

                // Check if user already has an active loan for this book
                var checkLoanSql = SqlQueryLoader.LoadQuery("Reservations", "CheckActiveLoan");
                
                await using var loanCmd = new MySqlCommand(checkLoanSql, conn, transaction);
                loanCmd.Parameters.AddWithValue("@BookId", request.BookId);
                loanCmd.Parameters.AddWithValue("@UserId", request.UserId);
                var hasActiveLoan = Convert.ToInt32(await loanCmd.ExecuteScalarAsync()) > 0;

                if (hasActiveLoan)
                {
                    await transaction.RollbackAsync();
                    return new ReservationResponse
                    {
                        Success = false,
                        Message = "You already have an active loan for this book"
                    };
                }

                // Check if user already has a reservation for this book
                var checkReservationSql = SqlQueryLoader.LoadQuery("Reservations", "CheckExistingReservation");
                
                await using var reservationCmd = new MySqlCommand(checkReservationSql, conn, transaction);
                reservationCmd.Parameters.AddWithValue("@BookId", request.BookId);
                reservationCmd.Parameters.AddWithValue("@UserId", request.UserId);
                var hasReservation = Convert.ToInt32(await reservationCmd.ExecuteScalarAsync()) > 0;

                if (hasReservation)
                {
                    await transaction.RollbackAsync();
                    return new ReservationResponse
                    {
                        Success = false,
                        Message = "You already have a reservation for this book"
                    };
                }

                // Get queue position (count of pending reservations + 1)
                var getQueuePositionSql = SqlQueryLoader.LoadQuery("Reservations", "GetQueuePosition");
                
                await using var queueCmd = new MySqlCommand(getQueuePositionSql, conn, transaction);
                queueCmd.Parameters.AddWithValue("@BookId", request.BookId);
                var queuePosition = Convert.ToInt32(await queueCmd.ExecuteScalarAsync()) + 1;

                // Create reservation
                var insertReservationSql = SqlQueryLoader.LoadQuery("Reservations", "InsertReservation");
                
                await using var insertCmd = new MySqlCommand(insertReservationSql, conn, transaction);
                insertCmd.Parameters.AddWithValue("@BookId", request.BookId);
                insertCmd.Parameters.AddWithValue("@UserId", request.UserId);
                insertCmd.Parameters.AddWithValue("@ReservedDate", DateTime.UtcNow);
                insertCmd.Parameters.AddWithValue("@QueuePosition", queuePosition);
                
                var reservationId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());

                await transaction.CommitAsync();

                var reservation = await GetReservationByIdAsync(reservationId);
                return new ReservationResponse
                {
                    Success = true,
                    Message = $"Reservation created successfully. Queue position: {queuePosition}",
                    Reservation = reservation
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ReservationResponse
                {
                    Success = false,
                    Message = $"Error creating reservation: {ex.Message}"
                };
            }
        }

        public async Task<ReservationResponse> CancelReservationAsync(int reservationId, int userId)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Get reservation details
                var getReservationSql = SqlQueryLoader.LoadQuery("Reservations", "GetReservationForCancel");
                
                await using var getCmd = new MySqlCommand(getReservationSql, conn, transaction);
                getCmd.Parameters.AddWithValue("@ReservationId", reservationId);
                
                await using var reader = await getCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    await reader.CloseAsync();
                    await transaction.RollbackAsync();
                    return new ReservationResponse
                    {
                        Success = false,
                        Message = "Reservation not found"
                    };
                }

                var bookId = reader.GetInt32(0);
                var reservationUserId = reader.GetInt32(1);
                var status = reader.GetString(2);
                var queuePosition = reader.GetInt32(3);
                await reader.CloseAsync();

                // Verify user owns this reservation
                if (reservationUserId != userId)
                {
                    await transaction.RollbackAsync();
                    return new ReservationResponse
                    {
                        Success = false,
                        Message = "You are not authorized to cancel this reservation"
                    };
                }

                if (status == "Cancelled" || status == "Fulfilled" || status == "Expired")
                {
                    await transaction.RollbackAsync();
                    return new ReservationResponse
                    {
                        Success = false,
                        Message = $"Reservation is already {status.ToLower()}"
                    };
                }

                // Cancel reservation
                var cancelReservationSql = SqlQueryLoader.LoadQuery("Reservations", "CancelReservation");
                
                await using var cancelCmd = new MySqlCommand(cancelReservationSql, conn, transaction);
                cancelCmd.Parameters.AddWithValue("@ReservationId", reservationId);
                cancelCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                await cancelCmd.ExecuteNonQueryAsync();

                // Update queue positions for remaining reservations
                var updateQueueSql = SqlQueryLoader.LoadQuery("Reservations", "UpdateQueuePositions");
                
                await using var updateQueueCmd = new MySqlCommand(updateQueueSql, conn, transaction);
                updateQueueCmd.Parameters.AddWithValue("@BookId", bookId);
                updateQueueCmd.Parameters.AddWithValue("@QueuePosition", queuePosition);
                updateQueueCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                await updateQueueCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                var reservation = await GetReservationByIdAsync(reservationId);
                return new ReservationResponse
                {
                    Success = true,
                    Message = "Reservation cancelled successfully",
                    Reservation = reservation
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ReservationResponse
                {
                    Success = false,
                    Message = $"Error cancelling reservation: {ex.Message}"
                };
            }
        }

        public async Task<ReservationResponse> FulfillReservationAsync(FulfillReservationRequest request)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Get reservation details
                var getReservationSql = SqlQueryLoader.LoadQuery("Reservations", "GetReservationForFulfill");
                
                await using var getCmd = new MySqlCommand(getReservationSql, conn, transaction);
                getCmd.Parameters.AddWithValue("@ReservationId", request.ReservationId);
                
                await using var reader = await getCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    await reader.CloseAsync();
                    await transaction.RollbackAsync();
                    return new ReservationResponse
                    {
                        Success = false,
                        Message = "Reservation not found"
                    };
                }

                var bookId = reader.GetInt32(0);
                var userId = reader.GetInt32(1);
                var status = reader.GetString(2);
                await reader.CloseAsync();

                if (status != "Available" && status != "Pending")
                {
                    await transaction.RollbackAsync();
                    return new ReservationResponse
                    {
                        Success = false,
                        Message = "Reservation cannot be fulfilled"
                    };
                }

                // Check available copies
                var checkAvailabilitySql = SqlQueryLoader.LoadQuery("Reservations", "CheckAvailableCopies");
                await using var availCmd = new MySqlCommand(checkAvailabilitySql, conn, transaction);
                availCmd.Parameters.AddWithValue("@BookId", bookId);
                var availableCopies = Convert.ToInt32(await availCmd.ExecuteScalarAsync());

                if (availableCopies <= 0)
                {
                    await transaction.RollbackAsync();
                    return new ReservationResponse
                    {
                        Success = false,
                        Message = "No copies available"
                    };
                }

                // Create lending record
                var dueDate = DateTime.UtcNow.AddDays(request.LoanDurationDays);
                var createLendingSql = SqlQueryLoader.LoadQuery("Reservations", "CreateLendingFromReservation");
                
                await using var lendingCmd = new MySqlCommand(createLendingSql, conn, transaction);
                lendingCmd.Parameters.AddWithValue("@BookId", bookId);
                lendingCmd.Parameters.AddWithValue("@UserId", userId);
                lendingCmd.Parameters.AddWithValue("@BorrowDate", DateTime.UtcNow);
                lendingCmd.Parameters.AddWithValue("@DueDate", dueDate);
                await lendingCmd.ExecuteNonQueryAsync();

                // Update reservation status
                var updateReservationSql = SqlQueryLoader.LoadQuery("Reservations", "FulfillReservation");
                
                await using var updateCmd = new MySqlCommand(updateReservationSql, conn, transaction);
                updateCmd.Parameters.AddWithValue("@ReservationId", request.ReservationId);
                updateCmd.Parameters.AddWithValue("@FulfilledDate", DateTime.UtcNow);
                updateCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                await updateCmd.ExecuteNonQueryAsync();

                // Update available copies
                var updateInventorySql = SqlQueryLoader.LoadQuery("Reservations", "DecrementAvailableCopies");
                
                await using var inventoryCmd = new MySqlCommand(updateInventorySql, conn, transaction);
                inventoryCmd.Parameters.AddWithValue("@BookId", bookId);
                inventoryCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                await inventoryCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                var reservation = await GetReservationByIdAsync(request.ReservationId);
                return new ReservationResponse
                {
                    Success = true,
                    Message = "Reservation fulfilled and book issued successfully",
                    Reservation = reservation
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ReservationResponse
                {
                    Success = false,
                    Message = $"Error fulfilling reservation: {ex.Message}"
                };
            }
        }

        public async Task<PagedReservationsResponse> GetUserReservationsAsync(int userId, int page = 1, int pageSize = 10)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = SqlQueryLoader.LoadQuery("Reservations", "GetUserReservations");
            var countSql = SqlQueryLoader.LoadQuery("Reservations", "CountUserReservations");

            var reservations = new List<ReservationDto>();
            
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

            await using var readerUser = await cmd.ExecuteReaderAsync();
            var readerU = (MySqlDataReader)readerUser;
            while (await readerU.ReadAsync())
            {
                var reservation = MapReservationDto(readerU);
                reservations.Add(reservation);
            }
            await readerU.CloseAsync();

            // Get total count
            await using var countCmd = new MySqlCommand(countSql, conn);
            countCmd.Parameters.AddWithValue("@UserId", userId);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            return new PagedReservationsResponse
            {
                Items = reservations,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedReservationsResponse> GetAllReservationsAsync(int page = 1, int pageSize = 10)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = SqlQueryLoader.LoadQuery("Reservations", "GetAllReservations");
            var countSql = SqlQueryLoader.LoadQuery("Reservations", "CountAllReservations");

            var reservations = new List<ReservationDto>();
            
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

            await using var readerAll = await cmd.ExecuteReaderAsync();
            var readerL = (MySqlDataReader)readerAll;
            while (await readerL.ReadAsync())
            {
                var reservation = MapReservationDto(readerL);
                reservations.Add(reservation);
            }
            await readerL.CloseAsync();

            // Get total count
            await using var countCmd = new MySqlCommand(countSql, conn);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            return new PagedReservationsResponse
            {
                Items = reservations,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ReservationDto?> GetReservationByIdAsync(int id)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = SqlQueryLoader.LoadQuery("Reservations", "GetReservationById");

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await using var readerById = await cmd.ExecuteReaderAsync();
            var readerI = (MySqlDataReader)readerById;
            if (await readerI.ReadAsync())
            {
                return MapReservationDto(readerI);
            }

            return null;
        }

        private ReservationDto MapReservationDto(MySqlDataReader reader)
        {
            return new ReservationDto
            {
                Id = reader.GetInt32("Id"),
                BookId = reader.GetInt32("BookId"),
                BookTitle = reader.GetString("Title"),
                BookAuthor = reader.GetString("Author"),
                BookIsbn = reader.GetString("Isbn"),
                UserId = reader.GetInt32("UserId"),
                Username = reader.GetString("Username"),
                MemberEmail = reader.GetString("Email"),
                ReservedDate = reader.GetDateTime("ReservedDate"),
                QueuePosition = reader.GetInt32("QueuePosition"),
                Status = reader.GetString("Status"),
                AvailableDate = reader.IsDBNull(reader.GetOrdinal("AvailableDate")) 
                    ? null 
                    : reader.GetDateTime("AvailableDate"),
                ExpiryDate = reader.IsDBNull(reader.GetOrdinal("ExpiryDate")) 
                    ? null 
                    : reader.GetDateTime("ExpiryDate"),
                FulfilledDate = reader.IsDBNull(reader.GetOrdinal("FulfilledDate")) 
                    ? null 
                    : reader.GetDateTime("FulfilledDate"),
                EstimatedAvailable = null // Can be calculated based on current loans
            };
        }
    }
}
