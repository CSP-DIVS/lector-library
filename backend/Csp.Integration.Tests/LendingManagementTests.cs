//Csp.Integration.Tests/LendingManagementTests.cs

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Csp.Integration.Tests
{
    public class LendingManagementTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;

        public LendingManagementTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
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

        private async Task<int> CreateTestBookAsync(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var bookRequest = new
            {
                Title = "Test Book " + Guid.NewGuid().ToString("N")[..8],
                Author = "Test Author",
                Isbn = "978" + Random.Shared.Next(1000000000, int.MaxValue).ToString(),
                Category = "Fiction",
                PublishedYear = 2020,
                TotalCopies = 5
            };

            var response = await _client.PostAsJsonAsync("/api/Books", bookRequest);
            var bookResponse = await response.Content.ReadFromJsonAsync<BookResponseDto>();
            return bookResponse?.Book?.Id ?? 0;
        }

        private async Task<int> GetTestUserIdAsync(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.GetAsync("/api/users/my-profile");
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            return user?.Id ?? 0;
        }

        #endregion

        #region Borrow Book Tests

        [Fact]
        public async Task BorrowBook_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(await GetMemberTokenAsync());

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            var borrowRequest = new
            {
                BookId = bookId,
                UserId = userId,
                LoanDurationDays = 14
            };

            _output.WriteLine($"Borrowing book: BookId={bookId}, UserId={userId}");

            // Act
            var response = await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);

            _output.WriteLine($"Response Status: {response.StatusCode}");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _output.WriteLine($"Error Response: {errorContent}");
            }

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var lendingResponse = await response.Content.ReadFromJsonAsync<LendingResponseDto>();
            Assert.NotNull(lendingResponse);
            Assert.True(lendingResponse!.Success);
            Assert.NotNull(lendingResponse.Lending);
            Assert.Equal(bookId, lendingResponse.Lending!.BookId);
            Assert.Equal(userId, lendingResponse.Lending.UserId);
            Assert.Equal("Active", lendingResponse.Lending.Status);
        }

        [Fact]
        public async Task BorrowBook_MemberCannotBorrow_ReturnsForbidden()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            var librarianToken = await GetLibrarianTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(memberToken);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            var borrowRequest = new
            {
                BookId = bookId,
                UserId = userId,
                LoanDurationDays = 14
            };

            _output.WriteLine($"Member attempting to borrow book: BookId={bookId}");

            // Act
            var response = await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);

            _output.WriteLine($"Response Status: {response.StatusCode}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task BorrowBook_InvalidBookId_ReturnsBadRequest()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var userId = await GetTestUserIdAsync(await GetMemberTokenAsync());

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            var borrowRequest = new
            {
                BookId = 99999,
                UserId = userId,
                LoanDurationDays = 14
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Return Book Tests

        [Fact]
        public async Task ReturnBook_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(await GetMemberTokenAsync());

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // First, borrow the book
            var borrowRequest = new { BookId = bookId, UserId = userId, LoanDurationDays = 14 };
            var borrowResponse = await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);
            var borrowResult = await borrowResponse.Content.ReadFromJsonAsync<LendingResponseDto>();
            var lendingId = borrowResult!.Lending!.Id;

            _output.WriteLine($"Returning book: LendingId={lendingId}");

            // Act
            var returnRequest = new { LendingId = lendingId };
            var response = await _client.PostAsJsonAsync("/api/Lendings/return", returnRequest);

            _output.WriteLine($"Response Status: {response.StatusCode}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var returnResult = await response.Content.ReadFromJsonAsync<LendingResponseDto>();
            Assert.NotNull(returnResult);
            Assert.True(returnResult!.Success);
            Assert.NotNull(returnResult.Lending);
            Assert.Equal("Returned", returnResult.Lending!.Status);
        }

        [Fact]
        public async Task ReturnBook_WithFine_ProcessesCorrectly()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(await GetMemberTokenAsync());

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // First, borrow the book
            var borrowRequest = new { BookId = bookId, UserId = userId, LoanDurationDays = 14 };
            var borrowResponse = await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);
            var borrowResult = await borrowResponse.Content.ReadFromJsonAsync<LendingResponseDto>();
            var lendingId = borrowResult!.Lending!.Id;

            // Act - Return book without fine (not overdue)
            var returnRequest = new { LendingId = lendingId };
            var response = await _client.PostAsJsonAsync("/api/Lendings/return", returnRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var returnResult = await response.Content.ReadFromJsonAsync<LendingResponseDto>();
            Assert.NotNull(returnResult);
            Assert.True(returnResult!.Success);
            // Book is not overdue, so fine should be null
            Assert.Null(returnResult.Lending!.FineAmount);
        }

        #endregion

        #region Renew Loan Tests

        [Fact]
        public async Task RenewLoan_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(memberToken);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // First, borrow the book
            var borrowRequest = new { BookId = bookId, UserId = userId, LoanDurationDays = 14 };
            var borrowResponse = await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);
            var borrowResult = await borrowResponse.Content.ReadFromJsonAsync<LendingResponseDto>();
            var lendingId = borrowResult!.Lending!.Id;

            _output.WriteLine($"Renewing loan: LendingId={lendingId}");

            // Switch to member token (members can renew their own loans)
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var renewRequest = new { LendingId = lendingId };
            var response = await _client.PostAsJsonAsync("/api/Lendings/renew", renewRequest);

            _output.WriteLine($"Response Status: {response.StatusCode}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var renewResult = await response.Content.ReadFromJsonAsync<LendingResponseDto>();
            Assert.NotNull(renewResult);
            Assert.True(renewResult!.Success);
            Assert.Equal(1, renewResult.Lending!.RenewalCount);
        }

        [Fact]
        public async Task RenewLoan_ExceedsMaxRenewals_ReturnsBadRequest()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(memberToken);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Borrow the book
            var borrowRequest = new { BookId = bookId, UserId = userId, LoanDurationDays = 14 };
            var borrowResponse = await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);
            var borrowResult = await borrowResponse.Content.ReadFromJsonAsync<LendingResponseDto>();
            var lendingId = borrowResult!.Lending!.Id;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Renew first time
            var renewRequest = new { LendingId = lendingId };
            await _client.PostAsJsonAsync("/api/Lendings/renew", renewRequest);

            // Renew second time
            await _client.PostAsJsonAsync("/api/Lendings/renew", renewRequest);

            // Act - Try to renew third time (should fail)
            var response = await _client.PostAsJsonAsync("/api/Lendings/renew", renewRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Get Active Loans Tests

        [Fact]
        public async Task GetActiveLoans_AsLibrarian_ReturnsAllLoans()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(await GetMemberTokenAsync());

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Create a loan
            var borrowRequest = new { BookId = bookId, UserId = userId, LoanDurationDays = 14 };
            await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);

            // Act
            var response = await _client.GetAsync("/api/Lendings/active?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedLendingsResponseDto>();
            Assert.NotNull(result);
            Assert.NotEmpty(result!.Items);
        }

        [Fact]
        public async Task GetActiveLoans_AsMember_ReturnsOnlyOwnLoans()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(memberToken);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Create a loan
            var borrowRequest = new { BookId = bookId, UserId = userId, LoanDurationDays = 14 };
            await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);

            // Switch to member token
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.GetAsync("/api/Lendings/active?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedLendingsResponseDto>();
            Assert.NotNull(result);
            Assert.All(result!.Items, loan => Assert.Equal(userId, loan.UserId));
        }

        #endregion

        #region Get Loan History Tests

        [Fact]
        public async Task GetLoanHistory_ReturnsReturnedLoans()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(await GetMemberTokenAsync());

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Borrow and return a book
            var borrowRequest = new { BookId = bookId, UserId = userId, LoanDurationDays = 14 };
            var borrowResponse = await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);
            var borrowResult = await borrowResponse.Content.ReadFromJsonAsync<LendingResponseDto>();
            var lendingId = borrowResult!.Lending!.Id;

            var returnRequest = new { LendingId = lendingId };
            await _client.PostAsJsonAsync("/api/Lendings/return", returnRequest);

            // Act
            var response = await _client.GetAsync("/api/Lendings/history?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedLendingsResponseDto>();
            Assert.NotNull(result);
            Assert.Contains(result!.Items, loan => loan.Id == lendingId && loan.Status == "Returned");
        }

        #endregion

        #region Get Lending By Id Tests

        [Fact]
        public async Task GetLending_ValidId_ReturnsLending()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(await GetMemberTokenAsync());

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Create a loan
            var borrowRequest = new { BookId = bookId, UserId = userId, LoanDurationDays = 14 };
            var borrowResponse = await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);
            var borrowResult = await borrowResponse.Content.ReadFromJsonAsync<LendingResponseDto>();
            var lendingId = borrowResult!.Lending!.Id;

            // Act
            var response = await _client.GetAsync($"/api/Lendings/{lendingId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var lending = await response.Content.ReadFromJsonAsync<LendingDto>();
            Assert.NotNull(lending);
            Assert.Equal(lendingId, lending!.Id);
        }

        [Fact]
        public async Task GetLending_MemberViewingOthersLoan_ReturnsForbidden()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            
            // Create a member and get their user ID
            var adminToken = await GetAdminTokenAsync();
            var otherUserId = await CreateTestMemberAsync(adminToken);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Create a loan for a different user
            var borrowRequest = new { BookId = bookId, UserId = otherUserId, LoanDurationDays = 14 };
            var borrowResponse = await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);
            var borrowResult = await borrowResponse.Content.ReadFromJsonAsync<LendingResponseDto>();
            var lendingId = borrowResult!.Lending!.Id;

            // Switch to member token (different user)
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.GetAsync($"/api/Lendings/{lendingId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion

        #region Helper Methods for Complex Scenarios

        private async Task<string> GetAdminTokenAsync()
        {
            var loginRequest = new { Username = "admin", Password = "admin123!" };
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return loginResponse?.Token ?? string.Empty;
        }

        private async Task<int> CreateTestMemberAsync(string adminToken)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            var unique = Guid.NewGuid().ToString("N")[..8];
            var memberRequest = new
            {
                Username = $"testmember_{unique}",
                Email = $"testmember_{unique}@test.com",
                Password = "Test123!"
            };

            var response = await _client.PostAsJsonAsync("/api/users/members", memberRequest);
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return result?.User?.Id ?? 0;
        }

        #endregion

        #region DTOs

        private class LoginResponseDto
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string? Token { get; set; }
            public UserDto? User { get; set; }
        }

        private class UserDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        private class BookResponseDto
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public BookDto? Book { get; set; }
        }

        private class BookDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
        }

        private class LendingResponseDto
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public LendingDto? Lending { get; set; }
        }

        private class LendingDto
        {
            public int Id { get; set; }
            public int BookId { get; set; }
            public int UserId { get; set; }
            public string Status { get; set; } = string.Empty;
            public int RenewalCount { get; set; }
            public decimal? FineAmount { get; set; }
        }

        private class PagedLendingsResponseDto
        {
            public List<LendingDto> Items { get; set; } = new();
            public int Total { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }

        #endregion
    }
}