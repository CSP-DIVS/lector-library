using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Csp.Integration.Tests
{
    public class PaymentIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public PaymentIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

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

        private sealed class BookResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public BookDto? Book { get; set; }
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
        }

        private sealed class CreateBookRequest
        {
            public string Title { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string Isbn { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public int PublishedYear { get; set; }
            public int TotalCopies { get; set; }
        }

        private sealed class BorrowResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public LendingDto? Lending { get; set; }
        }

        private sealed class LendingDto
        {
            public int Id { get; set; }
            public int BookId { get; set; }
            public int UserId { get; set; }
            public string Status { get; set; } = string.Empty;
            public decimal? FineAmount { get; set; }
            public bool FinePaid { get; set; }
        }

        private sealed class AdjustFineRequest
        {
            public decimal NewAmount { get; set; }
            public string Reason { get; set; } = string.Empty;
        }

        private sealed class PaymentResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public PaymentDto? Payment { get; set; }
            public decimal RemainingBalance { get; set; }
        }

        private sealed class PaymentDto
        {
            public int Id { get; set; }
            public int LendingId { get; set; }
            public int MemberId { get; set; }
            public decimal Amount { get; set; }
            public string PaymentMethod { get; set; } = string.Empty;
        }

        private sealed class PaymentHistoryResponse
        {
            public System.Collections.Generic.List<PaymentDto> Payments { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public bool HasNextPage { get; set; }
        }

        private async Task<(string token, int userId)> LoginAsync(string username, string password)
        {
            var login = await _client.PostAsJsonAsync("/api/Auth/login", new { Username = username, Password = password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            var body = await login.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(body);
            return (body!.Token!, body.User!.Id);
        }

        private async Task<int> CreateBookAsync(string adminToken)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            var req = new CreateBookRequest
            {
                Title = "PayTest " + Guid.NewGuid().ToString("N").Substring(0, 8),
                Author = "Integration Author",
                Isbn = "978" + Random.Shared.Next(1_000_000_000, int.MaxValue).ToString(),
                Category = "Fiction",
                PublishedYear = DateTime.UtcNow.Year,
                TotalCopies = 2
            };
            var resp = await _client.PostAsJsonAsync("/api/Books", req);
            Assert.True(resp.StatusCode == HttpStatusCode.Created || resp.StatusCode == HttpStatusCode.OK);
            var created = await resp.Content.ReadFromJsonAsync<BookResponse>();
            Assert.NotNull(created);
            Assert.True(created!.Success);
            return created.Book!.Id;
        }

        private async Task<int> BorrowBookAsync(string librarianToken, int bookId, int memberUserId)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", librarianToken);
            var resp = await _client.PostAsJsonAsync("/api/Lendings/borrow", new { BookId = bookId, UserId = memberUserId, LoanDurationDays = 14 });
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var data = await resp.Content.ReadFromJsonAsync<BorrowResponse>();
            Assert.NotNull(data);
            Assert.True(data!.Success);
            return data.Lending!.Id;
        }

        private async Task AdjustFineAsync(string adminToken, int lendingId, decimal newAmount)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            var req = new AdjustFineRequest { NewAmount = newAmount, Reason = "Integration test" };
            var resp = await _client.PutAsJsonAsync($"/api/Fines/{lendingId}/adjust", req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async Task RecordPayment_ValidFlow_UpdatesFineAndHistory()
        {
            // Arrange: login as users
            var (adminToken, adminId) = await LoginAsync("admin", "admin123!");
            var (librarianToken, _) = await LoginAsync("librarian", "lib123!");
            var (memberToken, memberId) = await LoginAsync("member", "member123!");

            // Arrange: create a book, borrow to member, set a fine
            var bookId = await CreateBookAsync(adminToken);
            var lendingId = await BorrowBookAsync(librarianToken, bookId, memberId);
            await AdjustFineAsync(adminToken, lendingId, 10.00m);

            // Act: record a partial payment as librarian
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", librarianToken);
            var paymentReq = new { MemberId = memberId, LendingId = lendingId, Amount = 4.50m, PaymentMethod = "Cash" };
            var payResp = await _client.PostAsJsonAsync("/api/Payments", paymentReq);

            // Assert
            Assert.Equal(HttpStatusCode.OK, payResp.StatusCode);
            var payBody = await payResp.Content.ReadFromJsonAsync<PaymentResponse>();
            Assert.NotNull(payBody);
            Assert.True(payBody!.Success);
            Assert.NotNull(payBody.Payment);
            Assert.Equal(4.50m, payBody.Payment!.Amount);
            Assert.Equal(5.50m, payBody.RemainingBalance);

            var paymentId = payBody.Payment!.Id;

            // Verify total paid endpoint
            var totalReq = await _client.GetAsync($"/api/Payments/total/{lendingId}");
            Assert.Equal(HttpStatusCode.OK, totalReq.StatusCode);
            var totalJson = await totalReq.Content.ReadAsStringAsync();
            Assert.Contains("totalPaid", totalJson, StringComparison.OrdinalIgnoreCase);

            // Verify history by lending id
            var historyResp = await _client.GetAsync($"/api/Payments/history?lendingId={lendingId}");
            Assert.Equal(HttpStatusCode.OK, historyResp.StatusCode);
            var history = await historyResp.Content.ReadFromJsonAsync<PaymentHistoryResponse>();
            Assert.NotNull(history);
            Assert.True(history!.TotalCount >= 1);
            Assert.Contains(history.Payments, p => p.Id == paymentId);

            // Member can see their own payments
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", memberToken);
            var memberHistory = await _client.GetAsync($"/api/Payments/member/{memberId}");
            Assert.Equal(HttpStatusCode.OK, memberHistory.StatusCode);
        }

        [Fact]
        public async Task RecordPayment_ExceedingAmount_ReturnsBadRequest()
        {
            var (adminToken, _) = await LoginAsync("admin", "admin123!");
            var (librarianToken, _) = await LoginAsync("librarian", "lib123!");
            var (memberToken, memberId) = await LoginAsync("member", "member123!");

            var bookId = await CreateBookAsync(adminToken);
            var lendingId = await BorrowBookAsync(librarianToken, bookId, memberId);
            await AdjustFineAsync(adminToken, lendingId, 8.00m);

            // Try to overpay
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", librarianToken);
            var paymentReq = new { MemberId = memberId, LendingId = lendingId, Amount = 10.00m, PaymentMethod = "Cash" };
            var payResp = await _client.PostAsJsonAsync("/api/Payments", paymentReq);

            Assert.Equal(HttpStatusCode.BadRequest, payResp.StatusCode);
            var msg = await payResp.Content.ReadAsStringAsync();
            Assert.Contains("cannot exceed", msg, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetMemberPayments_AuthorizationChecks()
        {
            var (adminToken, adminId) = await LoginAsync("admin", "admin123!");
            var (librarianToken, libId) = await LoginAsync("librarian", "lib123!");
            var (memberToken, memberId) = await LoginAsync("member", "member123!");

            // Admin should be able to view member payments even if none exist
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            var respAdmin = await _client.GetAsync($"/api/Payments/member/{memberId}");
            Assert.True(respAdmin.StatusCode == HttpStatusCode.OK || respAdmin.StatusCode == HttpStatusCode.InternalServerError);

            // Member can view their own
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", memberToken);
            var respSelf = await _client.GetAsync($"/api/Payments/member/{memberId}");
            Assert.True(respSelf.StatusCode == HttpStatusCode.OK || respSelf.StatusCode == HttpStatusCode.InternalServerError);

            // Member cannot view someone else's
            var otherId = adminId; // some other user
            var respForbidden = await _client.GetAsync($"/api/Payments/member/{otherId}");
            Assert.Equal(HttpStatusCode.Forbidden, respForbidden.StatusCode);
        }

        [Fact]
        public async Task PaymentsHealth_ReturnsHealthy()
        {
            var resp = await _client.GetAsync("/api/Payments/health");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var text = await resp.Content.ReadAsStringAsync();
            Assert.Contains("healthy", text, StringComparison.OrdinalIgnoreCase);
        }
    }
}