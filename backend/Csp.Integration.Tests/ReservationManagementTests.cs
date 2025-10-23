//Csp.Integration.Tests/ReservationManagementTests.cs

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Csp.Integration.Tests
{
    public class ReservationManagementTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;

        public ReservationManagementTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
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

        private async Task<string> GetAdminTokenAsync()
        {
            var loginRequest = new { Username = "admin", Password = "admin123!" };
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return loginResponse?.Token ?? string.Empty;
        }

        private async Task<int> CreateTestBookAsync(string token, int totalCopies = 1)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var bookRequest = new
            {
                Title = "Test Book " + Guid.NewGuid().ToString("N")[..8],
                Author = "Test Author",
                Isbn = "978" + Random.Shared.Next(1000000000, int.MaxValue).ToString(),
                Category = "Fiction",
                PublishedYear = 2020,
                TotalCopies = totalCopies
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

        private async Task<int> BorrowBookAsync(string librarianToken, int bookId, int userId)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);
            var borrowRequest = new { BookId = bookId, UserId = userId, LoanDurationDays = 14 };
            var response = await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);
            var result = await response.Content.ReadFromJsonAsync<LendingResponseDto>();
            return result?.Lending?.Id ?? 0;
        }

        #endregion

        #region Create Reservation Tests

        [Fact]
        public async Task CreateReservation_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken, totalCopies: 1);
            var userId = await GetTestUserIdAsync(memberToken);

            // Borrow the book to make it unavailable
            var adminUserId = await GetTestUserIdAsync(await GetAdminTokenAsync());
            await BorrowBookAsync(librarianToken, bookId, adminUserId);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            var reservationRequest = new
            {
                BookId = bookId,
                UserId = userId
            };

            _output.WriteLine($"Creating reservation: BookId={bookId}, UserId={userId}");

            // Act
            var response = await _client.PostAsJsonAsync("/api/Reservations", reservationRequest);

            _output.WriteLine($"Response Status: {response.StatusCode}");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _output.WriteLine($"Error Response: {errorContent}");
            }

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var reservationResponse = await response.Content.ReadFromJsonAsync<ReservationResponseDto>();
            Assert.NotNull(reservationResponse);
            Assert.True(reservationResponse!.Success);
            Assert.NotNull(reservationResponse.Reservation);
            Assert.Equal(bookId, reservationResponse.Reservation!.BookId);
            Assert.Equal(userId, reservationResponse.Reservation.UserId);
            Assert.Equal("Pending", reservationResponse.Reservation.Status);
        }

        [Fact]
        public async Task CreateReservation_MemberCanOnlyReserveForSelf_OverridesUserId()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken, totalCopies: 1);
            var userId = await GetTestUserIdAsync(memberToken);

            // Borrow the book to make it unavailable
            var adminUserId = await GetTestUserIdAsync(await GetAdminTokenAsync());
            await BorrowBookAsync(librarianToken, bookId, adminUserId);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Member tries to reserve for another user (should be overridden)
            var reservationRequest = new
            {
                BookId = bookId,
                UserId = 99999
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Reservations", reservationRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ReservationResponseDto>();
            Assert.NotNull(result);
            Assert.True(result!.Success);
            // Verify the reservation was created for the actual member, not userId 99999
            Assert.Equal(userId, result.Reservation!.UserId);
        }

        [Fact]
        public async Task CreateReservation_AlreadyHasActiveLoan_ReturnsBadRequest()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken, totalCopies: 2);
            var userId = await GetTestUserIdAsync(memberToken);

            // User already has an active loan for this book
            await BorrowBookAsync(librarianToken, bookId, userId);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            var reservationRequest = new { BookId = bookId, UserId = userId };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Reservations", reservationRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateReservation_DuplicateReservation_ReturnsBadRequest()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken, totalCopies: 1);
            var userId = await GetTestUserIdAsync(memberToken);

            // Borrow the book to make it unavailable
            var adminUserId = await GetTestUserIdAsync(await GetAdminTokenAsync());
            await BorrowBookAsync(librarianToken, bookId, adminUserId);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            var reservationRequest = new { BookId = bookId, UserId = userId };

            // Create first reservation
            await _client.PostAsJsonAsync("/api/Reservations", reservationRequest);

            // Act - Try to create duplicate reservation
            var response = await _client.PostAsJsonAsync("/api/Reservations", reservationRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Cancel Reservation Tests

        [Fact]
        public async Task CancelReservation_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken, totalCopies: 1);
            var userId = await GetTestUserIdAsync(memberToken);

            // Borrow the book and create reservation
            var adminUserId = await GetTestUserIdAsync(await GetAdminTokenAsync());
            await BorrowBookAsync(librarianToken, bookId, adminUserId);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
            var reservationRequest = new { BookId = bookId, UserId = userId };
            var createResponse = await _client.PostAsJsonAsync("/api/Reservations", reservationRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ReservationResponseDto>();
            var reservationId = createResult!.Reservation!.Id;

            _output.WriteLine($"Cancelling reservation: ReservationId={reservationId}");

            // Act
            var response = await _client.DeleteAsync($"/api/Reservations/{reservationId}");

            _output.WriteLine($"Response Status: {response.StatusCode}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var cancelResult = await response.Content.ReadFromJsonAsync<ReservationResponseDto>();
            Assert.NotNull(cancelResult);
            Assert.True(cancelResult!.Success);
        }

        [Fact]
        public async Task CancelReservation_NotFound_ReturnsBadRequest()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.DeleteAsync($"/api/Reservations/99999");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Fulfill Reservation Tests

        [Fact]
        public async Task FulfillReservation_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var adminToken = await GetAdminTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken, totalCopies: 1);
            var userId = await GetTestUserIdAsync(memberToken);

            // Borrow the book and create reservation
            var adminUserId = await GetTestUserIdAsync(adminToken);
            var lendingId = await BorrowBookAsync(librarianToken, bookId, adminUserId);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
            var reservationRequest = new { BookId = bookId, UserId = userId };
            var createResponse = await _client.PostAsJsonAsync("/api/Reservations", reservationRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ReservationResponseDto>();
            var reservationId = createResult!.Reservation!.Id;

            // Return the book to make it available
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);
            var returnRequest = new { LendingId = lendingId };
            await _client.PostAsJsonAsync("/api/Lendings/return", returnRequest);

            // Wait a moment for the reservation to become available
            await Task.Delay(500);

            _output.WriteLine($"Fulfilling reservation: ReservationId={reservationId}");

            // Act
            var fulfillRequest = new { ReservationId = reservationId, LoanDurationDays = 14 };
            var response = await _client.PostAsJsonAsync("/api/Reservations/fulfill", fulfillRequest);

            _output.WriteLine($"Response Status: {response.StatusCode}");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _output.WriteLine($"Error Response: {errorContent}");
            }

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var fulfillResult = await response.Content.ReadFromJsonAsync<ReservationResponseDto>();
            Assert.NotNull(fulfillResult);
            Assert.True(fulfillResult!.Success);
        }

        [Fact]
        public async Task FulfillReservation_MemberCannotFulfill_ReturnsForbidden()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            var fulfillRequest = new { ReservationId = 1, LoanDurationDays = 14 };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Reservations/fulfill", fulfillRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion

        #region Get My Reservations Tests

        [Fact]
        public async Task GetMyReservations_ReturnsUserReservations()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken, totalCopies: 1);
            var userId = await GetTestUserIdAsync(memberToken);

            // Borrow the book and create reservation
            var adminUserId = await GetTestUserIdAsync(await GetAdminTokenAsync());
            await BorrowBookAsync(librarianToken, bookId, adminUserId);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
            var reservationRequest = new { BookId = bookId, UserId = userId };
            await _client.PostAsJsonAsync("/api/Reservations", reservationRequest);

            // Act
            var response = await _client.GetAsync("/api/Reservations/my-reservations?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedReservationsResponseDto>();
            Assert.NotNull(result);
            Assert.NotEmpty(result!.Items);
            Assert.All(result.Items, r => Assert.Equal(userId, r.UserId));
        }

        [Fact]
        public async Task GetMyReservations_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.GetAsync("/api/Reservations/my-reservations?page=2&pageSize=5");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedReservationsResponseDto>();
            Assert.NotNull(result);
            Assert.Equal(2, result!.Page);
            Assert.Equal(5, result.PageSize);
        }

        #endregion

        #region Get All Reservations Tests

        [Fact]
        public async Task GetAllReservations_AsLibrarian_ReturnsAllReservations()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken, totalCopies: 1);
            var userId = await GetTestUserIdAsync(memberToken);

            // Borrow the book and create reservation
            var adminUserId = await GetTestUserIdAsync(await GetAdminTokenAsync());
            await BorrowBookAsync(librarianToken, bookId, adminUserId);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
            var reservationRequest = new { BookId = bookId, UserId = userId };
            await _client.PostAsJsonAsync("/api/Reservations", reservationRequest);

            // Switch to librarian
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Act
            var response = await _client.GetAsync("/api/Reservations?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedReservationsResponseDto>();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAllReservations_AsMember_ReturnsForbidden()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.GetAsync("/api/Reservations?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion

        #region Get Reservation By Id Tests

        [Fact]
        public async Task GetReservation_ValidId_ReturnsReservation()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken, totalCopies: 1);
            var userId = await GetTestUserIdAsync(memberToken);

            // Borrow the book and create reservation
            var adminUserId = await GetTestUserIdAsync(await GetAdminTokenAsync());
            await BorrowBookAsync(librarianToken, bookId, adminUserId);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
            var reservationRequest = new { BookId = bookId, UserId = userId };
            var createResponse = await _client.PostAsJsonAsync("/api/Reservations", reservationRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ReservationResponseDto>();
            var reservationId = createResult!.Reservation!.Id;

            // Act
            var response = await _client.GetAsync($"/api/Reservations/{reservationId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var reservation = await response.Content.ReadFromJsonAsync<ReservationDto>();
            Assert.NotNull(reservation);
            Assert.Equal(reservationId, reservation!.Id);
        }

        [Fact]
        public async Task GetReservation_MemberViewingOthersReservation_ReturnsForbidden()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var adminToken = await GetAdminTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken, totalCopies: 1);
            
            // Create reservation for another user
            var otherUserId = await CreateTestMemberAsync(adminToken);
            var adminUserId = await GetTestUserIdAsync(adminToken);
            await BorrowBookAsync(librarianToken, bookId, adminUserId);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);
            var reservationRequest = new { BookId = bookId, UserId = otherUserId };
            var createResponse = await _client.PostAsJsonAsync("/api/Reservations", reservationRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ReservationResponseDto>();
            var reservationId = createResult!.Reservation!.Id;

            // Switch to member token (different user)
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.GetAsync($"/api/Reservations/{reservationId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetReservation_NotFound_ReturnsNotFound()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.GetAsync($"/api/Reservations/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Queue Management Tests

        [Fact]
        public async Task ReservationQueue_MultipleUsers_MaintainsQueuePosition()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var adminToken = await GetAdminTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken, totalCopies: 1);

            // Borrow the book
            var adminUserId = await GetTestUserIdAsync(adminToken);
            await BorrowBookAsync(librarianToken, bookId, adminUserId);

            // Create multiple members and reservations
            var member1Id = await CreateTestMemberAsync(adminToken);
            var member2Id = await CreateTestMemberAsync(adminToken);
            var member3Id = await CreateTestMemberAsync(adminToken);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Create reservations
            var res1 = await _client.PostAsJsonAsync("/api/Reservations", new { BookId = bookId, UserId = member1Id });
            var res1Result = await res1.Content.ReadFromJsonAsync<ReservationResponseDto>();

            var res2 = await _client.PostAsJsonAsync("/api/Reservations", new { BookId = bookId, UserId = member2Id });
            var res2Result = await res2.Content.ReadFromJsonAsync<ReservationResponseDto>();

            var res3 = await _client.PostAsJsonAsync("/api/Reservations", new { BookId = bookId, UserId = member3Id });
            var res3Result = await res3.Content.ReadFromJsonAsync<ReservationResponseDto>();

            // Assert queue positions
            Assert.Equal(1, res1Result!.Reservation!.QueuePosition);
            Assert.Equal(2, res2Result!.Reservation!.QueuePosition);
            Assert.Equal(3, res3Result!.Reservation!.QueuePosition);
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

        private class ReservationResponseDto
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public ReservationDto? Reservation { get; set; }
        }

        private class ReservationDto
        {
            public int Id { get; set; }
            public int BookId { get; set; }
            public int UserId { get; set; }
            public string Status { get; set; } = string.Empty;
            public int QueuePosition { get; set; }
        }

        private class PagedReservationsResponseDto
        {
            public List<ReservationDto> Items { get; set; } = new();
            public int Total { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
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
        }

        #endregion
    }
}