using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using System.Text.Json;
using System.Text;

namespace Csp.Integration.Tests
{
    public class BookManagementTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;

        public BookManagementTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
            _client = _factory.CreateClient();
        }

        #region Helper Methods

        private async Task<string> GetLibrarianTokenAsync()
        {
            var loginRequest = new { Username = "librarian", Password = "lib123!" };
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
            
            if (response.StatusCode != HttpStatusCode.OK)
            {
                _output.WriteLine($"Failed to get librarian token. Status: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                _output.WriteLine($"Error content: {errorContent}");
                return string.Empty;
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return loginResponse?.Token ?? string.Empty;
        }

        private async Task<string> GetMemberTokenAsync()
        {
            var loginRequest = new { Username = "testuser", Password = "password123" };
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
            
            if (response.StatusCode != HttpStatusCode.OK)
            {
                _output.WriteLine($"Failed to get member token. Status: {response.StatusCode}");
                return string.Empty;
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return loginResponse?.Token ?? string.Empty;
        }

        private CreateBookRequest CreateValidBookRequest()
        {
            return new CreateBookRequest
            {
                Title = "Test Book " + Guid.NewGuid().ToString("N")[..8],
                Author = "Test Author",
                Isbn = "978" + Random.Shared.Next(1000000000, int.MaxValue).ToString(),
                Category = "Fiction",
                PublishedYear = 2020,
                TotalCopies = 5
            };
        }

        #endregion

        #region Positive Test Scenarios

        [Fact]
        public async Task CreateBook_ValidData_ReturnsCreated()
        {
            // Arrange
            var token = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var request = CreateValidBookRequest();
            
            _output.WriteLine($"Creating book with data: Title={request.Title}, Author={request.Author}, ISBN={request.Isbn}");

            // Act
            var response = await _client.PostAsJsonAsync("/api/Books", request);
            
            _output.WriteLine($"Response Status: {response.StatusCode}");
            
            if (response.StatusCode != HttpStatusCode.Created)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _output.WriteLine($"Error Response: {errorContent}");
            }

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var bookResponse = await response.Content.ReadFromJsonAsync<BookResponse>();
            Assert.NotNull(bookResponse);
            Assert.True(bookResponse.Success);
            Assert.NotNull(bookResponse.Book);
            Assert.Equal(request.Title, bookResponse.Book.Title);
            Assert.Equal(request.Author, bookResponse.Book.Author);
            Assert.Equal(request.Isbn, bookResponse.Book.Isbn);
            Assert.True(bookResponse.Book.IsActive);
            
            _output.WriteLine($"Successfully created book with ID: {bookResponse.Book.Id}");
        }

        [Fact]
        public async Task GetBooks_NoFilters_ReturnsPagedResults()
        {
            // Arrange - Create a test book first
            var token = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var createRequest = CreateValidBookRequest();
            await _client.PostAsJsonAsync("/api/Books", createRequest);

            // Act
            _client.DefaultRequestHeaders.Authorization = null; // Public endpoint
            var response = await _client.GetAsync("/api/Books?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var pagedResponse = await response.Content.ReadFromJsonAsync<PagedBooksResponse>();
            Assert.NotNull(pagedResponse);
            Assert.NotNull(pagedResponse.Items);
            Assert.True(pagedResponse.Total >= 0);
            Assert.Equal(1, pagedResponse.Page);
            Assert.Equal(10, pagedResponse.PageSize);
            
            _output.WriteLine($"Retrieved {pagedResponse.Items.Count} books out of {pagedResponse.Total} total");
        }

        [Fact]
        public async Task GetBooks_WithSearch_ReturnsFilteredResults()
        {
            // Arrange
            var token = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var uniqueTitle = "SearchableBook" + Guid.NewGuid().ToString("N")[..8];
            var createRequest = CreateValidBookRequest();
            createRequest.Title = uniqueTitle;
            
            var createResponse = await _client.PostAsJsonAsync("/api/Books", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            
            var createdBook = await createResponse.Content.ReadFromJsonAsync<BookResponse>();
            Assert.NotNull(createdBook);
            Assert.NotNull(createdBook.Book);

            // Act
            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync($"/api/Books?search={uniqueTitle}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var pagedResponse = await response.Content.ReadFromJsonAsync<PagedBooksResponse>();
            Assert.NotNull(pagedResponse);
            Assert.Contains(pagedResponse.Items, book => book.Title.Contains(uniqueTitle));
            
            _output.WriteLine($"Search for '{uniqueTitle}' returned {pagedResponse.Items.Count} results");
        }

        [Fact]
        public async Task GetBookById_ExistingBook_ReturnsBook()
        {
            // Arrange - Create a test book first
            var token = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var createRequest = CreateValidBookRequest();
            var createResponse = await _client.PostAsJsonAsync("/api/Books", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            
            var createdBook = await createResponse.Content.ReadFromJsonAsync<BookResponse>();
            Assert.NotNull(createdBook);
            Assert.NotNull(createdBook.Book);

            // Act
            _client.DefaultRequestHeaders.Authorization = null; // Public endpoint
            var response = await _client.GetAsync($"/api/Books/{createdBook.Book.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var book = await response.Content.ReadFromJsonAsync<BookDto>();
            Assert.NotNull(book);
            Assert.Equal(createdBook.Book.Id, book.Id);
            Assert.Equal(createRequest.Title, book.Title);
            
            _output.WriteLine($"Successfully retrieved book: {book.Title}");
        }

        [Fact]
        public async Task UpdateBook_ValidData_ReturnsUpdated()
        {
            // Arrange - Create a test book first
            var token = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var createRequest = CreateValidBookRequest();
            var createResponse = await _client.PostAsJsonAsync("/api/Books", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            
            var createdBook = await createResponse.Content.ReadFromJsonAsync<BookResponse>();
            Assert.NotNull(createdBook);
            Assert.NotNull(createdBook.Book);

            var updateRequest = new UpdateBookRequest
            {
                Title = "Updated " + createRequest.Title,
                Author = "Updated " + createRequest.Author,
                Isbn = createRequest.Isbn,
                Category = "Updated Category",
                PublishedYear = 2021,
                TotalCopies = 10
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/Books/{createdBook.Book.Id}", updateRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var bookResponse = await response.Content.ReadFromJsonAsync<BookResponse>();
            Assert.NotNull(bookResponse);
            Assert.True(bookResponse.Success);
            Assert.NotNull(bookResponse.Book);
            Assert.Equal(updateRequest.Title, bookResponse.Book.Title);
            Assert.Equal(updateRequest.Author, bookResponse.Book.Author);
            Assert.Equal(updateRequest.Category, bookResponse.Book.Category);
            
            _output.WriteLine($"Successfully updated book to: {bookResponse.Book.Title}");
        }

        [Fact]
        public async Task UpdateBookStatus_ValidData_ReturnsUpdated()
        {
            // Arrange - Create a test book first
            var token = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var createRequest = CreateValidBookRequest();
            var createResponse = await _client.PostAsJsonAsync("/api/Books", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            
            var createdBook = await createResponse.Content.ReadFromJsonAsync<BookResponse>();
            Assert.NotNull(createdBook);
            Assert.NotNull(createdBook.Book);

            // Act - Deactivate the book
            var response = await _client.PutAsJsonAsync($"/api/Books/{createdBook.Book.Id}/status", false);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var bookResponse = await response.Content.ReadFromJsonAsync<BookResponse>();
            Assert.NotNull(bookResponse);
            Assert.True(bookResponse.Success);
            Assert.NotNull(bookResponse.Book);
            Assert.False(bookResponse.Book.IsActive);
            
            _output.WriteLine($"Successfully deactivated book: {bookResponse.Book.Title}");
        }

        [Fact]
        public async Task DeleteBook_ExistingBook_ReturnsNoContent()
        {
            // Arrange - Create a test book first
            var token = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var createRequest = CreateValidBookRequest();
            var createResponse = await _client.PostAsJsonAsync("/api/Books", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            
            var createdBook = await createResponse.Content.ReadFromJsonAsync<BookResponse>();
            Assert.NotNull(createdBook);
            Assert.NotNull(createdBook.Book);

            // Act
            var response = await _client.DeleteAsync($"/api/Books/{createdBook.Book.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            // Verify book is deleted
            _client.DefaultRequestHeaders.Authorization = null;
            var getResponse = await _client.GetAsync($"/api/Books/{createdBook.Book.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
            
            _output.WriteLine($"Successfully deleted book with ID: {createdBook.Book.Id}");
        }

        #endregion

        #region Negative Test Scenarios

        [Fact]
        public async Task CreateBook_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;
            var request = CreateValidBookRequest();

            // Act
            var response = await _client.PostAsJsonAsync("/api/Books", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _output.WriteLine("Create book without authorization correctly returned Unauthorized");
        }

        [Fact]
        public async Task CreateBook_MemberRole_ReturnsForbidden()
        {
            // Arrange
            var token = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var request = CreateValidBookRequest();

            // Act
            var response = await _client.PostAsJsonAsync("/api/Books", request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine("Create book with member role correctly returned Forbidden");
        }

        [Fact]
        public async Task CreateBook_EmptyTitle_ReturnsBadRequest()
        {
            // Arrange
            var token = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var request = CreateValidBookRequest();
            request.Title = "";

            // Act
            var response = await _client.PostAsJsonAsync("/api/Books", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var bookResponse = await response.Content.ReadFromJsonAsync<BookResponse>();
            Assert.NotNull(bookResponse);
            Assert.False(bookResponse.Success);
            Assert.Contains("required", bookResponse.Message, StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"Empty title validation error: {bookResponse.Message}");
        }

        [Fact]
        public async Task CreateBook_InvalidIsbn_ReturnsBadRequest()
        {
            // Arrange
            var token = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var request = CreateValidBookRequest();
            request.Isbn = "123"; // Too short

            // Act
            var response = await _client.PostAsJsonAsync("/api/Books", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var bookResponse = await response.Content.ReadFromJsonAsync<BookResponse>();
            Assert.NotNull(bookResponse);
            Assert.False(bookResponse.Success);
            Assert.Contains("ISBN", bookResponse.Message);
            
            _output.WriteLine($"Invalid ISBN validation error: {bookResponse.Message}");
        }

        [Fact]
        public async Task CreateBook_InvalidPublishedYear_ReturnsBadRequest()
        {
            // Arrange
            var token = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var request = CreateValidBookRequest();
            request.PublishedYear = 500; // Too old

            // Act
            var response = await _client.PostAsJsonAsync("/api/Books", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var bookResponse = await response.Content.ReadFromJsonAsync<BookResponse>();
            Assert.NotNull(bookResponse);
            Assert.False(bookResponse.Success);
            Assert.Contains("Published year", bookResponse.Message);
            
            _output.WriteLine($"Invalid published year validation error: {bookResponse.Message}");
        }

        [Fact]
        public async Task CreateBook_DuplicateIsbn_ReturnsBadRequest()
        {
            // Arrange
            var token = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var request1 = CreateValidBookRequest();
            var request2 = CreateValidBookRequest();
            request2.Isbn = request1.Isbn; // Same ISBN

            // Act - Create first book
            var response1 = await _client.PostAsJsonAsync("/api/Books", request1);
            Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

            // Act - Try to create second book with same ISBN
            var response2 = await _client.PostAsJsonAsync("/api/Books", request2);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
            
            var bookResponse = await response2.Content.ReadFromJsonAsync<BookResponse>();
            Assert.NotNull(bookResponse);
            Assert.False(bookResponse.Success);
            Assert.Contains("ISBN already exists", bookResponse.Message);
            
            _output.WriteLine($"Duplicate ISBN validation error: {bookResponse.Message}");
        }

        [Fact]
        public async Task GetBookById_NonExistentBook_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/Books/{nonExistentId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            _output.WriteLine($"Non-existent book ID {nonExistentId} correctly returned NotFound");
        }

        [Fact]
        public async Task UpdateBook_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;
            var updateRequest = new UpdateBookRequest
            {
                Title = "Updated Title",
                Author = "Updated Author",
                Isbn = "9781234567890",
                Category = "Updated Category",
                PublishedYear = 2021,
                TotalCopies = 10
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/Books/1", updateRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _output.WriteLine("Update book without authorization correctly returned Unauthorized");
        }

        [Fact]
        public async Task UpdateBook_NonExistentBook_ReturnsBadRequest()
        {
            // Arrange
            var token = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var updateRequest = new UpdateBookRequest
            {
                Title = "Updated Title",
                Author = "Updated Author",
                Isbn = "9781234567890",
                Category = "Updated Category",
                PublishedYear = 2021,
                TotalCopies = 10
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/Books/999999", updateRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _output.WriteLine("Update non-existent book correctly returned BadRequest");
        }

        [Fact]
        public async Task DeleteBook_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.DeleteAsync("/api/Books/1");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _output.WriteLine("Delete book without authorization correctly returned Unauthorized");
        }

        [Fact]
        public async Task DeleteBook_NonExistentBook_ReturnsNotFound()
        {
            // Arrange
            var token = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync("/api/Books/999999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            _output.WriteLine("Delete non-existent book correctly returned NotFound");
        }

        [Fact]
        public async Task GetBooks_InvalidPagination_HandlesGracefully()
        {
            // Act
            var response = await _client.GetAsync("/api/Books?page=-1&pageSize=0");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Should handle gracefully
            
            var pagedResponse = await response.Content.ReadFromJsonAsync<PagedBooksResponse>();
            Assert.NotNull(pagedResponse);
            _output.WriteLine($"Invalid pagination handled gracefully, returned {pagedResponse.Items.Count} items");
        }

        #endregion

        #region Helper DTOs

        private sealed class LoginResponseDto
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public UserDto? User { get; set; }
            public string? Token { get; set; }
        }

        private sealed class UserDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }

        private sealed class BookDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string Isbn { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public int PublishedYear { get; set; }
            public bool IsActive { get; set; }
            public int TotalCopies { get; set; }
            public int AvailableCopies { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        private sealed class CreateBookRequest
        {
            public string Title { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string Isbn { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public int PublishedYear { get; set; }
            public int TotalCopies { get; set; } = 1;
        }

        private sealed class UpdateBookRequest
        {
            public string Title { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string Isbn { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public int PublishedYear { get; set; }
            public int TotalCopies { get; set; }
        }

        private sealed class PagedBooksResponse
        {
            public List<BookDto> Items { get; set; } = new();
            public int Total { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }

        private sealed class BookResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public BookDto? Book { get; set; }
        }

        #endregion
    }
}