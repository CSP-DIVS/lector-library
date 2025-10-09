using Csp.Api.Models;
using Csp.Api.DTOs;
using Csp.Api.Data;
using MySql.Data.MySqlClient;

namespace Csp.Api.Services
{
    public interface IBookService
    {
        Task<BookResponse> CreateBookAsync(CreateBookRequest request, int actorUserId);
        Task<BookResponse> UpdateBookAsync(int id, UpdateBookRequest request, int actorUserId);
        Task<BookResponse> UpdateBookStatusAsync(int id, bool isActive, int actorUserId);
        Task<PagedBooksResponse> GetBooksAsync(BookSearchRequest request, string? userRole = null);
        Task<BookDto?> GetBookByIdAsync(int id);
        Task<bool> DeleteBookAsync(int id, int actorUserId);
    }

    public class BookService : IBookService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public BookService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                                 ?? _configuration.GetConnectionString("Default")
                                 ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<BookResponse> CreateBookAsync(CreateBookRequest request, int actorUserId)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Title) || 
                string.IsNullOrWhiteSpace(request.Author) || 
                string.IsNullOrWhiteSpace(request.Isbn) || 
                string.IsNullOrWhiteSpace(request.Category))
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = "Title, Author, ISBN, and Category are required" 
                };
            }

            // Validate ISBN format (basic validation)
            if (request.Isbn.Length < 10 || request.Isbn.Length > 20)
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = "ISBN must be between 10 and 20 characters" 
                };
            }

            // Validate published year
            if (request.PublishedYear < 1000 || request.PublishedYear > DateTime.Now.Year + 1)
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = "Published year must be between 1000 and " + (DateTime.Now.Year + 1) 
                };
            }

            // Validate total copies
            if (request.TotalCopies < 1)
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = "Total copies must be at least 1" 
                };
            }

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Check if ISBN already exists
            var checkIsbnSql = SqlQueryLoader.LoadQuery("Books", "CheckIsbnExists");
            await using var checkCmd = new MySqlCommand(checkIsbnSql, conn);
            checkCmd.Parameters.AddWithValue("@Isbn", request.Isbn);
            var isbnExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

            if (isbnExists)
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = "A book with this ISBN already exists" 
                };
            }

            // Insert book
            var insertBookSql = SqlQueryLoader.LoadQuery("Books", "InsertBook");
            
            await using var bookCmd = new MySqlCommand(insertBookSql, conn);
            bookCmd.Parameters.AddWithValue("@Title", request.Title);
            bookCmd.Parameters.AddWithValue("@Author", request.Author);
            bookCmd.Parameters.AddWithValue("@Isbn", request.Isbn);
            bookCmd.Parameters.AddWithValue("@Category", request.Category);
            bookCmd.Parameters.AddWithValue("@PublishedYear", request.PublishedYear);
            bookCmd.Parameters.AddWithValue("@CreatedBy", actorUserId);
            bookCmd.Parameters.AddWithValue("@UpdatedBy", actorUserId);

            var bookId = Convert.ToInt32(await bookCmd.ExecuteScalarAsync());

            // Insert inventory record
            var insertInventorySql = SqlQueryLoader.LoadQuery("Books", "InsertBookInventory");
            
            await using var inventoryCmd = new MySqlCommand(insertInventorySql, conn);
            inventoryCmd.Parameters.AddWithValue("@BookId", bookId);
            inventoryCmd.Parameters.AddWithValue("@TotalCopies", request.TotalCopies);
            inventoryCmd.Parameters.AddWithValue("@AvailableCopies", request.TotalCopies);
            await inventoryCmd.ExecuteNonQueryAsync();

            // Log audit
            await WriteAuditAsync(conn, actorUserId, "CreateBook", bookId, 
                $"title={request.Title}, isbn={request.Isbn}");

            // Return created book
            var createdBook = await GetBookByIdAsync(bookId);
            return new BookResponse 
            { 
                Success = true, 
                Message = "Book created successfully", 
                Book = createdBook 
            };
        }

        public async Task<BookResponse> UpdateBookAsync(int id, UpdateBookRequest request, int actorUserId)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Title) || 
                string.IsNullOrWhiteSpace(request.Author) || 
                string.IsNullOrWhiteSpace(request.Isbn) || 
                string.IsNullOrWhiteSpace(request.Category))
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = "Title, Author, ISBN, and Category are required" 
                };
            }

            // Validate ISBN format
            if (request.Isbn.Length < 10 || request.Isbn.Length > 20)
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = "ISBN must be between 10 and 20 characters" 
                };
            }

            // Validate published year
            if (request.PublishedYear < 1000 || request.PublishedYear > DateTime.Now.Year + 1)
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = "Published year must be between 1000 and " + (DateTime.Now.Year + 1) 
                };
            }

            // Validate total copies
            if (request.TotalCopies < 1)
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = "Total copies must be at least 1" 
                };
            }

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Check if book exists
            var bookExistsSql = SqlQueryLoader.LoadQuery("Books", "CheckBookExists");
            await using var existsCmd = new MySqlCommand(bookExistsSql, conn);
            existsCmd.Parameters.AddWithValue("@Id", id);
            var bookExists = Convert.ToInt32(await existsCmd.ExecuteScalarAsync()) > 0;

            if (!bookExists)
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = "Book not found" 
                };
            }

            // Check if ISBN already exists for different book
            var checkIsbnSql = SqlQueryLoader.LoadQuery("Books", "CheckIsbnExistsForOtherBook");
            await using var checkCmd = new MySqlCommand(checkIsbnSql, conn);
            checkCmd.Parameters.AddWithValue("@Isbn", request.Isbn);
            checkCmd.Parameters.AddWithValue("@Id", id);
            var isbnExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

            if (isbnExists)
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = "A book with this ISBN already exists" 
                };
            }

            // Update book
            var updateBookSql = SqlQueryLoader.LoadQuery("Books", "UpdateBook");
            
            await using var bookCmd = new MySqlCommand(updateBookSql, conn);
            bookCmd.Parameters.AddWithValue("@Title", request.Title);
            bookCmd.Parameters.AddWithValue("@Author", request.Author);
            bookCmd.Parameters.AddWithValue("@Isbn", request.Isbn);
            bookCmd.Parameters.AddWithValue("@Category", request.Category);
            bookCmd.Parameters.AddWithValue("@PublishedYear", request.PublishedYear);
            bookCmd.Parameters.AddWithValue("@UpdatedBy", actorUserId);
            bookCmd.Parameters.AddWithValue("@Id", id);

            var rowsAffected = await bookCmd.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = "Failed to update book" 
                };
            }

            // Update inventory - recalculate available copies
            var getCurrentInventorySql = SqlQueryLoader.LoadQuery("Books", "GetCurrentInventory");
            await using var getInventoryCmd = new MySqlCommand(getCurrentInventorySql, conn);
            getInventoryCmd.Parameters.AddWithValue("@BookId", id);
            
            int currentTotal = 0;
            int currentAvailable = 0;
            await using var inventoryReader = await getInventoryCmd.ExecuteReaderAsync();
            if (await inventoryReader.ReadAsync())
            {
                currentTotal = Convert.ToInt32(inventoryReader["TotalCopies"]);
                currentAvailable = Convert.ToInt32(inventoryReader["AvailableCopies"]);
            }
            await inventoryReader.CloseAsync();
            
            // Calculate new available copies
            int totalDifference = request.TotalCopies - currentTotal;
            int newAvailableCopies = Math.Max(0, currentAvailable + totalDifference);
            
            var updateInventorySql = SqlQueryLoader.LoadQuery("Books", "UpdateBookInventory");
            
            await using var inventoryCmd = new MySqlCommand(updateInventorySql, conn);
            inventoryCmd.Parameters.AddWithValue("@TotalCopies", request.TotalCopies);
            inventoryCmd.Parameters.AddWithValue("@AvailableCopies", newAvailableCopies);
            inventoryCmd.Parameters.AddWithValue("@BookId", id);
            await inventoryCmd.ExecuteNonQueryAsync();

            // Log audit
            await WriteAuditAsync(conn, actorUserId, "UpdateBook", id, 
                $"title={request.Title}, isbn={request.Isbn}");

            // Return updated book
            var updatedBook = await GetBookByIdAsync(id);
            return new BookResponse 
            { 
                Success = true, 
                Message = "Book updated successfully", 
                Book = updatedBook 
            };
        }

        public async Task<BookResponse> UpdateBookStatusAsync(int id, bool isActive, int actorUserId)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Check if book exists
            var bookExistsSql = SqlQueryLoader.LoadQuery("Books", "CheckBookExists");
            await using var existsCmd = new MySqlCommand(bookExistsSql, conn);
            existsCmd.Parameters.AddWithValue("@Id", id);
            var bookExists = Convert.ToInt32(await existsCmd.ExecuteScalarAsync()) > 0;

            if (!bookExists)
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = "Book not found" 
                };
            }

            // Check if actor user exists (to avoid foreign key constraint issues)
            var userExistsSql = SqlQueryLoader.LoadQuery("Books", "CheckUserExists");
            await using var userExistsCmd = new MySqlCommand(userExistsSql, conn);
            userExistsCmd.Parameters.AddWithValue("@UserId", actorUserId);
            var userExists = Convert.ToInt32(await userExistsCmd.ExecuteScalarAsync()) > 0;

            if (!userExists)
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = $"Invalid user ID: {actorUserId}. User not found in database." 
                };
            }

            // If deactivating, check if book is on loan
            if (!isActive)
            {
                // For now, we'll assume no loan system exists yet
                // In a real implementation, you'd check a loans table
                // var checkLoansSql = "SELECT COUNT(*) FROM loans WHERE BookId = @BookId AND Status = 'Active'";
                // This would be implemented when the loan system is added
            }

            // Update book status
            var updateSql = SqlQueryLoader.LoadQuery("Books", "UpdateBookStatus");
            
            await using var cmd = new MySqlCommand(updateSql, conn);
            cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@Id", id);

            try
            {
                var rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return new BookResponse 
                    { 
                        Success = false, 
                        Message = "Failed to update book status - no rows affected" 
                    };
                }
            }
            catch (Exception ex)
            {
                return new BookResponse 
                { 
                    Success = false, 
                    Message = $"Database error while updating book status: {ex.Message}" 
                };
            }

            // Log audit
            await WriteAuditAsync(conn, actorUserId, isActive ? "ReactivateBook" : "DeactivateBook", id, "");

            // Return updated book
            var updatedBook = await GetBookByIdAsync(id);
            return new BookResponse 
            { 
                Success = true, 
                Message = isActive ? "Book reactivated successfully" : "Book deactivated successfully", 
                Book = updatedBook 
            };
        }

        public async Task<PagedBooksResponse> GetBooksAsync(BookSearchRequest request, string? userRole = null)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var conditions = new List<string>();
            var parameters = new List<MySqlParameter>();

            // Only show active books to members, all books to staff
            if (userRole == "Member")
            {
                conditions.Add("b.IsActive = 1");
            }
            // For Librarian and Administrator, show all books (no IsActive filter)

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                conditions.Add("(b.Title LIKE @search OR b.Author LIKE @search OR b.Isbn LIKE @search)");
                parameters.Add(new MySqlParameter("@search", $"%{request.Search}%"));
            }

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                conditions.Add("b.Category = @category");
                parameters.Add(new MySqlParameter("@category", request.Category));
            }

            if (!string.IsNullOrWhiteSpace(request.Author))
            {
                conditions.Add("b.Author LIKE @author");
                parameters.Add(new MySqlParameter("@author", $"%{request.Author}%"));
            }

            var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            // Count total records
            var countSql = $@"
                SELECT COUNT(*) 
                FROM books b 
                LEFT JOIN book_inventory bi ON b.Id = bi.BookId 
                {whereClause}";

            await using var countCmd = new MySqlCommand(countSql, conn);
            foreach (var param in parameters)
            {
                countCmd.Parameters.Add(param);
            }

            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            // Get paginated results
            var offset = (request.Page - 1) * request.PageSize;
            var listSql = $@"
                SELECT b.Id, b.Title, b.Author, b.Isbn, b.Category, b.PublishedYear, 
                       b.IsActive, b.CreatedAt, b.UpdatedAt,
                       COALESCE(bi.TotalCopies, 0) as TotalCopies,
                       COALESCE(bi.AvailableCopies, 0) as AvailableCopies,
                       CASE 
                           WHEN COALESCE(bi.AvailableCopies, 0) > 0 THEN 'Available'
                           ELSE 'Unavailable'
                       END as Status
                FROM books b 
                LEFT JOIN book_inventory bi ON b.Id = bi.BookId 
                {whereClause}
                ORDER BY b.Title 
                LIMIT @pageSize OFFSET @offset";

            await using var listCmd = new MySqlCommand(listSql, conn);
            foreach (var param in parameters)
            {
                listCmd.Parameters.Add(param);
            }
            listCmd.Parameters.Add(new MySqlParameter("@pageSize", request.PageSize));
            listCmd.Parameters.Add(new MySqlParameter("@offset", offset));

            var items = new List<BookDto>();
            await using var reader = await listCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new BookDto
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Title = reader["Title"]?.ToString() ?? string.Empty,
                    Author = reader["Author"]?.ToString() ?? string.Empty,
                    Isbn = reader["Isbn"]?.ToString() ?? string.Empty,
                    Category = reader["Category"]?.ToString() ?? string.Empty,
                    PublishedYear = Convert.ToInt32(reader["PublishedYear"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                    TotalCopies = Convert.ToInt32(reader["TotalCopies"]),
                    AvailableCopies = Convert.ToInt32(reader["AvailableCopies"]),
                    Status = reader["Status"]?.ToString() ?? string.Empty,
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                    UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                });
            }

            return new PagedBooksResponse 
            { 
                Items = items, 
                Total = total, 
                Page = request.Page, 
                PageSize = request.PageSize 
            };
        }

        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = SqlQueryLoader.LoadQuery("Books", "GetBookById");

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new BookDto
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Title = reader["Title"]?.ToString() ?? string.Empty,
                    Author = reader["Author"]?.ToString() ?? string.Empty,
                    Isbn = reader["Isbn"]?.ToString() ?? string.Empty,
                    Category = reader["Category"]?.ToString() ?? string.Empty,
                    PublishedYear = Convert.ToInt32(reader["PublishedYear"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                    TotalCopies = Convert.ToInt32(reader["TotalCopies"]),
                    AvailableCopies = Convert.ToInt32(reader["AvailableCopies"]),
                    Status = reader["Status"]?.ToString() ?? string.Empty,
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                    UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                };
            }

            return null;
        }

        public async Task<bool> DeleteBookAsync(int id, int actorUserId)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Check if book exists
            var bookExistsSql = SqlQueryLoader.LoadQuery("Books", "CheckBookExists");
            await using var existsCmd = new MySqlCommand(bookExistsSql, conn);
            existsCmd.Parameters.AddWithValue("@Id", id);
            var bookExists = Convert.ToInt32(await existsCmd.ExecuteScalarAsync()) > 0;

            if (!bookExists)
            {
                return false;
            }

            // Delete book (cascade will handle inventory)
            var deleteSql = SqlQueryLoader.LoadQuery("Books", "DeleteBook");
            await using var cmd = new MySqlCommand(deleteSql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                await WriteAuditAsync(conn, actorUserId, "DeleteBook", id, "");
                return true;
            }

            return false;
        }

        private static async Task WriteAuditAsync(MySqlConnection conn, int actorUserId, string action, int targetId, string details)
        {
            var sql = SqlQueryLoader.LoadQuery("Books", "InsertAuditLog");
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);
            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@TargetUserId", targetId);
            cmd.Parameters.AddWithValue("@Details", details);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
