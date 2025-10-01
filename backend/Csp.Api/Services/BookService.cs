using Csp.Api.Models;
using Csp.Api.DTOs;
using MySql.Data.MySqlClient;
using System.Data;

namespace Csp.Api.Services
{
    /// <summary>
    /// Defines the business logic for managing books.
    /// </summary>
    public interface IBookService
    {
        /// <summary>
        /// Creates a new book and its associated inventory.
        /// </summary>
        /// <param name="request">The details of the book to create.</param>
        /// <param name="actorUserId">The ID of the user performing the action.</param>
        /// <returns>A response containing the created book's data.</returns>
        Task<BookResponse> CreateBookAsync(CreateBookRequest request, int actorUserId);

        /// <summary>
        /// Updates an existing book's details and inventory.
        /// </summary>
        /// <param name="id">The ID of the book to update.</param>
        /// <param name="request">The new details for the book.</param>
        /// <param name="actorUserId">The ID of the user performing the action.</param>
        /// <returns>A response containing the updated book's data.</returns>
        Task<BookResponse> UpdateBookAsync(int id, UpdateBookRequest request, int actorUserId);

        /// <summary>
        /// Activates or deactivates a book.
        /// </summary>
        /// <param name="id">The ID of the book to update.</param>
        /// <param name="isActive">The new status of the book.</param>
        /// <param name="actorUserId">The ID of the user performing the action.</param>
        /// <returns>A response containing the updated book's data.</returns>
        Task<BookResponse> UpdateBookStatusAsync(int id, bool isActive, int actorUserId);

        /// <summary>
        /// Retrieves a paginated list of books based on search criteria.
        /// </summary>
        /// <param name="request">The search and pagination parameters.</param>
        /// <param name="userRole">The role of the user making the request (e.g., "Member").</param>
        /// <returns>A paginated list of books.</returns>
        Task<PagedBooksResponse> GetBooksAsync(BookSearchRequest request, string? userRole = null);

        /// <summary>
        /// Retrieves a single book by its ID.
        /// </summary>
        /// <param name="id">The ID of the book to retrieve.</param>
        /// <returns>The book DTO, or null if not found.</returns>
        Task<BookDto?> GetBookByIdAsync(int id);

        /// <summary>
        /// Deletes a book from the system.
        /// </summary>
        /// <param name="id">The ID of the book to delete.</param>
        /// <param name="actorUserId">The ID of the user performing the action.</param>
        /// <returns>True if the book was deleted, otherwise false.</returns>
        Task<bool> DeleteBookAsync(int id, int actorUserId);
    }

    /// <summary>
    /// Implements the business logic for managing books using a MySQL database.
    /// </summary>
    public class BookService : IBookService
    {
        private readonly string _connectionString;

        public BookService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                 ?? configuration.GetConnectionString("Default")
                                 ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        /// <inheritdoc />
        public async Task<BookResponse> CreateBookAsync(CreateBookRequest request, int actorUserId)
        {
            // Model validation is handled by the controller via DataAnnotations on the DTO.
            // This keeps the service layer focused on business logic.

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Step 1: Check if a book with the same ISBN already exists.
                var checkIsbnSql = "SELECT COUNT(*) FROM books WHERE Isbn = @Isbn";
                await using var checkCmd = new MySqlCommand(checkIsbnSql, conn, transaction);
                checkCmd.Parameters.AddWithValue("@Isbn", request.Isbn);
                if (Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0)
                {
                    return new BookResponse { Success = false, Message = "A book with this ISBN already exists." };
                }

                // Step 2: Insert the new book record.
                var insertBookSql = @"
                    INSERT INTO books (Title, Author, Isbn, Category, PublishedYear, IsActive, CreatedBy, UpdatedBy)
                    VALUES (@Title, @Author, @Isbn, @Category, @PublishedYear, 1, @ActorUserId, @ActorUserId);
                    SELECT LAST_INSERT_ID();";
                
                await using var bookCmd = new MySqlCommand(insertBookSql, conn, transaction);
                bookCmd.Parameters.AddWithValue("@Title", request.Title);
                bookCmd.Parameters.AddWithValue("@Author", request.Author);
                bookCmd.Parameters.AddWithValue("@Isbn", request.Isbn);
                bookCmd.Parameters.AddWithValue("@Category", request.Category);
                bookCmd.Parameters.AddWithValue("@PublishedYear", request.PublishedYear);
                bookCmd.Parameters.AddWithValue("@ActorUserId", actorUserId);

                var bookId = Convert.ToInt32(await bookCmd.ExecuteScalarAsync());

                // Step 3: Insert the corresponding inventory record.
                var insertInventorySql = @"
                    INSERT INTO book_inventory (BookId, TotalCopies, AvailableCopies)
                    VALUES (@BookId, @TotalCopies, @TotalCopies);"; // AvailableCopies is same as TotalCopies on creation
                
                await using var inventoryCmd = new MySqlCommand(insertInventorySql, conn, transaction);
                inventoryCmd.Parameters.AddWithValue("@BookId", bookId);
                inventoryCmd.Parameters.AddWithValue("@TotalCopies", request.TotalCopies);
                await inventoryCmd.ExecuteNonQueryAsync();

                // Step 4: Log the audit trail.
                await WriteAuditAsync(conn, transaction, actorUserId, "CreateBook", bookId, $"title={request.Title}, isbn={request.Isbn}");

                // Step 5: Commit the transaction.
                await transaction.CommitAsync();

                // Step 6: Retrieve and return the newly created book.
                var createdBook = await GetBookByIdAsync(bookId);
                return new BookResponse { Success = true, Message = "Book created successfully.", Book = createdBook };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // In a real-world app, you would log this exception.
                return new BookResponse { Success = false, Message = $"An unexpected error occurred: {ex.Message}" };
            }
        }

        /// <inheritdoc />
        public async Task<BookResponse> UpdateBookAsync(int id, UpdateBookRequest request, int actorUserId)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Step 1: Retrieve the current state of the book and inventory.
                var getBookSql = @"
                    SELECT b.Id, bi.TotalCopies, bi.AvailableCopies
                    FROM books b
                    JOIN book_inventory bi ON b.Id = bi.BookId
                    WHERE b.Id = @Id FOR UPDATE;"; // Lock the row for update
                
                await using var getCmd = new MySqlCommand(getBookSql, conn, transaction);
                getCmd.Parameters.AddWithValue("@Id", id);
                
                int currentTotal = 0;
                int currentAvailable = 0;
                bool bookFound = false;
                await using (var reader = await getCmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        bookFound = true;
                        currentTotal = reader.GetInt32("TotalCopies");
                        currentAvailable = reader.GetInt32("AvailableCopies");
                    }
                }

                if (!bookFound)
                {
                    return new BookResponse { Success = false, Message = "Book not found." };
                }

                // Step 2: Check for ISBN conflict.
                var checkIsbnSql = "SELECT COUNT(*) FROM books WHERE Isbn = @Isbn AND Id <> @Id";
                await using var checkCmd = new MySqlCommand(checkIsbnSql, conn, transaction);
                checkCmd.Parameters.AddWithValue("@Isbn", request.Isbn);
                checkCmd.Parameters.AddWithValue("@Id", id);
                if (Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0)
                {
                    return new BookResponse { Success = false, Message = "A book with this ISBN already exists." };
                }

                // Step 3: Update the book details.
                var updateBookSql = @"
                    UPDATE books SET
                        Title = @Title, Author = @Author, Isbn = @Isbn, Category = @Category,
                        PublishedYear = @PublishedYear, UpdatedBy = @ActorUserId, UpdatedAt = CURRENT_TIMESTAMP
                    WHERE Id = @Id;";
                
                await using var bookCmd = new MySqlCommand(updateBookSql, conn, transaction);
                bookCmd.Parameters.AddWithValue("@Title", request.Title);
                bookCmd.Parameters.AddWithValue("@Author", request.Author);
                bookCmd.Parameters.AddWithValue("@Isbn", request.Isbn);
                bookCmd.Parameters.AddWithValue("@Category", request.Category);
                bookCmd.Parameters.AddWithValue("@PublishedYear", request.PublishedYear);
                bookCmd.Parameters.AddWithValue("@ActorUserId", actorUserId);
                bookCmd.Parameters.AddWithValue("@Id", id);
                await bookCmd.ExecuteNonQueryAsync();

                // Step 4: Update the inventory, recalculating available copies.
                int totalDifference = request.TotalCopies - currentTotal;
                int newAvailableCopies = Math.Max(0, currentAvailable + totalDifference);

                var updateInventorySql = @"
                    UPDATE book_inventory SET
                        TotalCopies = @TotalCopies, AvailableCopies = @AvailableCopies, UpdatedAt = CURRENT_TIMESTAMP
                    WHERE BookId = @BookId;";
                
                await using var inventoryCmd = new MySqlCommand(updateInventorySql, conn, transaction);
                inventoryCmd.Parameters.AddWithValue("@TotalCopies", request.TotalCopies);
                inventoryCmd.Parameters.AddWithValue("@AvailableCopies", newAvailableCopies);
                inventoryCmd.Parameters.AddWithValue("@BookId", id);
                await inventoryCmd.ExecuteNonQueryAsync();

                // Step 5: Log the audit trail.
                await WriteAuditAsync(conn, transaction, actorUserId, "UpdateBook", id, $"title={request.Title}, isbn={request.Isbn}");

                // Step 6: Commit the transaction.
                await transaction.CommitAsync();

                var updatedBook = await GetBookByIdAsync(id);
                return new BookResponse { Success = true, Message = "Book updated successfully.", Book = updatedBook };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BookResponse { Success = false, Message = $"An unexpected error occurred: {ex.Message}" };
            }
        }

        /// <inheritdoc />
        public async Task<BookResponse> UpdateBookStatusAsync(int id, bool isActive, int actorUserId)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var transaction = await conn.BeginTransactionAsync();
            
            try
            {
                 // Step 1: Check if the book exists.
                var bookExistsSql = "SELECT COUNT(*) FROM books WHERE Id = @Id";
                await using var existsCmd = new MySqlCommand(bookExistsSql, conn, transaction);
                existsCmd.Parameters.AddWithValue("@Id", id);
                if (Convert.ToInt32(await existsCmd.ExecuteScalarAsync()) == 0)
                {
                    return new BookResponse { Success = false, Message = "Book not found." };
                }

                // If deactivating, you might add a check here to see if the book is currently on loan.
                // For now, we proceed with the status update.

                // Step 2: Update the book's IsActive status.
                var updateSql = "UPDATE books SET IsActive = @IsActive, UpdatedBy = @ActorUserId, UpdatedAt = CURRENT_TIMESTAMP WHERE Id = @Id";
                await using var cmd = new MySqlCommand(updateSql, conn, transaction);
                cmd.Parameters.AddWithValue("@IsActive", isActive);
                cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();

                // Step 3: Log the action.
                var action = isActive ? "ReactivateBook" : "DeactivateBook";
                await WriteAuditAsync(conn, transaction, actorUserId, action, id, $"status={(isActive ? "Active" : "Inactive")}");

                // Step 4: Commit transaction.
                await transaction.CommitAsync();

                var updatedBook = await GetBookByIdAsync(id);
                var message = isActive ? "Book reactivated successfully." : "Book deactivated successfully.";
                return new BookResponse { Success = true, Message = message, Book = updatedBook };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BookResponse { Success = false, Message = $"An unexpected error occurred: {ex.Message}" };
            }
        }

        /// <inheritdoc />
        public async Task<PagedBooksResponse> GetBooksAsync(BookSearchRequest request, string? userRole = null)
        {
            var items = new List<BookDto>();
            int total = 0;

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var whereConditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            // Filter by role: Members should only see active books.
            if (userRole == "Member")
            {
                whereConditions.Add("b.IsActive = 1");
            }

            // Add search filters.
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                whereConditions.Add("(b.Title LIKE @Search OR b.Author LIKE @Search OR b.Isbn LIKE @Search)");
                parameters["@Search"] = $"%{request.Search}%";
            }
            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                whereConditions.Add("b.Category = @Category");
                parameters["@Category"] = request.Category;
            }
            if (!string.IsNullOrWhiteSpace(request.Author))
            {
                whereConditions.Add("b.Author LIKE @Author");
                parameters["@Author"] = $"%{request.Author}%";
            }

            var whereClause = whereConditions.Any() ? $"WHERE {string.Join(" AND ", whereConditions)}" : "";

            // Get total count.
            var countSql = $"SELECT COUNT(b.Id) FROM books b {whereClause}";
            await using (var countCmd = new MySqlCommand(countSql, conn))
            {
                foreach (var p in parameters) countCmd.Parameters.AddWithValue(p.Key, p.Value);
                total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
            }

            if (total == 0) return new PagedBooksResponse { Total = 0 };
            
            // Get paginated items.
            var querySql = $@"
                SELECT
                    b.Id, b.Title, b.Author, b.Isbn, b.Category, b.PublishedYear,
                    b.IsActive, b.CreatedAt, b.UpdatedAt,
                    COALESCE(bi.TotalCopies, 0) AS TotalCopies,
                    COALESCE(bi.AvailableCopies, 0) AS AvailableCopies,
                    CASE WHEN COALESCE(bi.AvailableCopies, 0) > 0 THEN 'Available' ELSE 'Unavailable' END AS Status
                FROM books b
                LEFT JOIN book_inventory bi ON b.Id = bi.BookId
                {whereClause}
                ORDER BY b.Title
                LIMIT @PageSize OFFSET @Offset;";

            await using (var cmd = new MySqlCommand(querySql, conn))
            {
                foreach (var p in parameters) cmd.Parameters.AddWithValue(p.Key, p.Value);
                cmd.Parameters.AddWithValue("@PageSize", request.PageSize);
                cmd.Parameters.AddWithValue("@Offset", (request.Page - 1) * request.PageSize);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    items.Add(new BookDto
                    {
                        Id = reader.GetInt32("Id"),
                        Title = reader.GetString("Title"),
                        Author = reader.GetString("Author"),
                        Isbn = reader.GetString("Isbn"),
                        Category = reader.GetString("Category"),
                        PublishedYear = reader.GetInt32("PublishedYear"),
                        IsActive = reader.GetBoolean("IsActive"),
                        TotalCopies = reader.GetInt32("TotalCopies"),
                        AvailableCopies = reader.GetInt32("AvailableCopies"),
                        Status = reader.GetString("Status"),
                        CreatedAt = reader.GetDateTime("CreatedAt"),
                        UpdatedAt = reader.GetDateTime("UpdatedAt")
                    });
                }
            }

            return new PagedBooksResponse
            {
                Items = items,
                Total = total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        /// <inheritdoc />
        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    b.Id, b.Title, b.Author, b.Isbn, b.Category, b.PublishedYear, 
                    b.IsActive, b.CreatedAt, b.UpdatedAt,
                    COALESCE(bi.TotalCopies, 0) as TotalCopies,
                    COALESCE(bi.AvailableCopies, 0) as AvailableCopies,
                    CASE WHEN COALESCE(bi.AvailableCopies, 0) > 0 THEN 'Available' ELSE 'Unavailable' END as Status
                FROM books b 
                LEFT JOIN book_inventory bi ON b.Id = bi.BookId 
                WHERE b.Id = @Id";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new BookDto
                {
                    Id = reader.GetInt32("Id"),
                    Title = reader.GetString("Title"),
                    Author = reader.GetString("Author"),
                    Isbn = reader.GetString("Isbn"),
                    Category = reader.GetString("Category"),
                    PublishedYear = reader.GetInt32("PublishedYear"),
                    IsActive = reader.GetBoolean("IsActive"),
                    TotalCopies = reader.GetInt32("TotalCopies"),
                    AvailableCopies = reader.GetInt32("AvailableCopies"),
                    Status = reader.GetString("Status"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UpdatedAt = reader.GetDateTime("UpdatedAt")
                };
            }
            return null;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteBookAsync(int id, int actorUserId)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Assuming foreign key constraints are set to cascade deletes from 'books' to 'book_inventory'.
                var deleteSql = "DELETE FROM books WHERE Id = @Id";
                await using var cmd = new MySqlCommand(deleteSql, conn, transaction);
                cmd.Parameters.AddWithValue("@Id", id);
                
                var rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    await WriteAuditAsync(conn, transaction, actorUserId, "DeleteBook", id, "");
                    await transaction.CommitAsync();
                    return true;
                }
                else
                {
                    await transaction.RollbackAsync();
                    return false; // Book not found
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        /// <summary>
        /// Writes an entry to the audit log within a transaction.
        /// </summary>
        private static async Task WriteAuditAsync(MySqlConnection conn, MySqlTransaction trans, int actorUserId, string action, int targetId, string details)
        {
            var sql = "INSERT INTO audit_log (ActorUserId, Action, TargetUserId, Details) VALUES (@ActorUserId, @Action, @TargetId, @Details)";
            await using var cmd = new MySqlCommand(sql, conn, trans);
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);
            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@TargetId", targetId);
            cmd.Parameters.AddWithValue("@Details", details);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
