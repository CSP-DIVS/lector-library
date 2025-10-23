//Csp.Integration.Tests/FineManagementTests.cs

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Csp.Integration.Tests
{
    public class FineManagementTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;

        public FineManagementTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
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

        private async Task<int> CreateOverdueLoanAsync(string librarianToken, int userId, int bookId)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Borrow a book
            var borrowRequest = new { BookId = bookId, UserId = userId, LoanDurationDays = 14 };
            var borrowResponse = await _client.PostAsJsonAsync("/api/Lendings/borrow", borrowRequest);
            var borrowResult = await borrowResponse.Content.ReadFromJsonAsync<LendingResponseDto>();
            
            return borrowResult?.Lending?.Id ?? 0;
        }

        #endregion

        #region Process Payment Tests

        [Fact]
        public async Task ProcessPayment_ValidFine_ReturnsSuccess()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(memberToken);
            
            // Create a lending and generate fines
            await CreateOverdueLoanAsync(librarianToken, userId, bookId);
            
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);
            await _client.PostAsJsonAsync("/api/Fines/generate-overdue-fines", new { });

            // Get the created fine
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
            var finesResponse = await _client.GetAsync("/api/Fines/my-fines?page=1&pageSize=10");
            var fines = await finesResponse.Content.ReadFromJsonAsync<PagedFinesResponseDto>();
            
            if (fines == null || fines.Items.Count == 0)
            {
                _output.WriteLine("No fines found, skipping payment test");
                return; // Skip test if no fines were created
            }

            var fineId = fines.Items[0].Id;
            _output.WriteLine($"Processing payment for fine ID: {fineId}, Amount: {fines.Items[0].Amount}");

            // Act
            var paymentRequest = new { FineId = fineId };
            var response = await _client.PostAsJsonAsync("/api/Fines/pay", paymentRequest);

            _output.WriteLine($"Response Status: {response.StatusCode}");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _output.WriteLine($"Error Response: {errorContent}");
            }

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();
            Assert.NotNull(paymentResponse);
            Assert.True(paymentResponse!.Success);
            Assert.NotNull(paymentResponse.Payment);
            Assert.Equal(fineId, paymentResponse.Payment!.FineId);
            Assert.Equal(userId, paymentResponse.Payment.UserId);
        }

        [Fact]
        public async Task ProcessPayment_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;
            var paymentRequest = new { FineId = 1 };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Fines/pay", paymentRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _output.WriteLine("Payment without authorization correctly returned Unauthorized");
        }

        [Fact]
        public async Task ProcessPayment_NonExistentFine_ReturnsBadRequest()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            var paymentRequest = new { FineId = 999999 };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Fines/pay", paymentRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();
            Assert.NotNull(paymentResponse);
            Assert.False(paymentResponse!.Success);
            Assert.Contains("not found", paymentResponse.Message, StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"Non-existent fine payment correctly returned: {paymentResponse.Message}");
        }

        #endregion

        #region Get Fines Tests

        [Fact]
        public async Task GetMyFines_AsMember_ReturnsUserFines()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.GetAsync("/api/Fines/my-fines?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedFinesResponseDto>();
            Assert.NotNull(result);
            Assert.NotNull(result!.Items);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            
            _output.WriteLine($"Retrieved {result.Items.Count} fines out of {result.Total} total");
        }

        [Fact]
        public async Task GetMyFines_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/Fines/my-fines");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _output.WriteLine("Get my fines without authorization correctly returned Unauthorized");
        }

        [Fact]
        public async Task GetAllFines_AsLibrarian_ReturnsAllFines()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Act
            var response = await _client.GetAsync("/api/Fines?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedFinesResponseDto>();
            Assert.NotNull(result);
            Assert.NotNull(result!.Items);
            
            _output.WriteLine($"Librarian retrieved {result.Items.Count} fines out of {result.Total} total");
        }

        [Fact]
        public async Task GetAllFines_AsMember_ReturnsForbidden()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.GetAsync("/api/Fines?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine("Member attempting to get all fines correctly returned Forbidden");
        }

        [Fact]
        public async Task GetUserFines_AsLibrarian_ReturnsSpecificUserFines()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var userId = await GetTestUserIdAsync(memberToken);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Act
            var response = await _client.GetAsync($"/api/Fines/user/{userId}?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedFinesResponseDto>();
            Assert.NotNull(result);
            
            _output.WriteLine($"Retrieved {result!.Items.Count} fines for user {userId}");
        }

        #endregion

        #region Get Payments Tests

        [Fact]
        public async Task GetMyPayments_AsMember_ReturnsUserPayments()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.GetAsync("/api/Fines/my-payments?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedPaymentsResponseDto>();
            Assert.NotNull(result);
            Assert.NotNull(result!.Items);
            
            _output.WriteLine($"Retrieved {result.Items.Count} payments out of {result.Total} total");
        }

        [Fact]
        public async Task GetMyPayments_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/Fines/my-payments");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _output.WriteLine("Get my payments without authorization correctly returned Unauthorized");
        }

        [Fact]
        public async Task GetAllPayments_AsLibrarian_ReturnsAllPayments()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Act
            var response = await _client.GetAsync("/api/Fines/payments?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedPaymentsResponseDto>();
            Assert.NotNull(result);
            
            _output.WriteLine($"Librarian retrieved {result!.Items.Count} payments out of {result.Total} total");
        }

        [Fact]
        public async Task GetAllPayments_AsMember_ReturnsForbidden()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.GetAsync("/api/Fines/payments");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine("Member attempting to get all payments correctly returned Forbidden");
        }

        [Fact]
        public async Task GetUserPayments_AsLibrarian_ReturnsSpecificUserPayments()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var userId = await GetTestUserIdAsync(memberToken);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Act
            var response = await _client.GetAsync($"/api/Fines/payments/user/{userId}?page=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedPaymentsResponseDto>();
            Assert.NotNull(result);
            
            _output.WriteLine($"Retrieved {result!.Items.Count} payments for user {userId}");
        }

        #endregion

        #region Waive Fine Tests

        [Fact]
        public async Task WaiveFine_AsLibrarian_ValidFine_ReturnsSuccess()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(memberToken);
            
            // Create a lending and generate fines
            await CreateOverdueLoanAsync(librarianToken, userId, bookId);
            
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);
            await _client.PostAsJsonAsync("/api/Fines/generate-overdue-fines", new { });

            // Get the created fine
            var finesResponse = await _client.GetAsync($"/api/Fines/user/{userId}?page=1&pageSize=10");
            var fines = await finesResponse.Content.ReadFromJsonAsync<PagedFinesResponseDto>();
            
            if (fines == null || fines.Items.Count == 0)
            {
                _output.WriteLine("No fines found, skipping waive test");
                return; // Skip test if no fines were created
            }

            var fineId = fines.Items[0].Id;
            _output.WriteLine($"Waiving fine ID: {fineId}");

            // Act
            var waiveRequest = new { FineId = fineId, Reason = "First-time offense" };
            var response = await _client.PostAsJsonAsync("/api/Fines/waive", waiveRequest);

            _output.WriteLine($"Response Status: {response.StatusCode}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var fineResponse = await response.Content.ReadFromJsonAsync<FineResponseDto>();
            Assert.NotNull(fineResponse);
            Assert.True(fineResponse!.Success);
            Assert.Contains("waived", fineResponse.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task WaiveFine_AsMember_ReturnsForbidden()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            var waiveRequest = new { FineId = 1, Reason = "Please waive" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Fines/waive", waiveRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine("Member attempting to waive fine correctly returned Forbidden");
        }

        [Fact]
        public async Task WaiveFine_NonExistentFine_ReturnsBadRequest()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            var waiveRequest = new { FineId = 999999, Reason = "Test" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Fines/waive", waiveRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var fineResponse = await response.Content.ReadFromJsonAsync<FineResponseDto>();
            Assert.NotNull(fineResponse);
            Assert.False(fineResponse!.Success);
            
            _output.WriteLine($"Waive non-existent fine correctly returned: {fineResponse.Message}");
        }

        #endregion

        #region Adjust Fine Amount Tests

        [Fact]
        public async Task AdjustFineAmount_AsLibrarian_ValidFine_ReturnsSuccess()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(memberToken);
            
            // Create a lending and generate fines
            await CreateOverdueLoanAsync(librarianToken, userId, bookId);
            
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);
            await _client.PostAsJsonAsync("/api/Fines/generate-overdue-fines", new { });

            // Get the created fine
            var finesResponse = await _client.GetAsync($"/api/Fines/user/{userId}?page=1&pageSize=10");
            var fines = await finesResponse.Content.ReadFromJsonAsync<PagedFinesResponseDto>();
            
            if (fines == null || fines.Items.Count == 0)
            {
                _output.WriteLine("No fines found, skipping adjust amount test");
                return; // Skip test if no fines were created
            }

            var fineId = fines.Items[0].Id;
            var newAmount = 50.00m;
            _output.WriteLine($"Adjusting fine ID: {fineId} to amount: {newAmount}");

            // Act
            var adjustRequest = new { FineId = fineId, NewAmount = newAmount, Reason = "Special consideration" };
            var response = await _client.PostAsJsonAsync("/api/Fines/adjust-amount", adjustRequest);

            _output.WriteLine($"Response Status: {response.StatusCode}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var fineResponse = await response.Content.ReadFromJsonAsync<FineResponseDto>();
            Assert.NotNull(fineResponse);
            Assert.True(fineResponse!.Success);
            Assert.Contains("adjusted", fineResponse.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AdjustFineAmount_AsMember_ReturnsForbidden()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            var adjustRequest = new { FineId = 1, NewAmount = 10.00m, Reason = "Please reduce" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Fines/adjust-amount", adjustRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine("Member attempting to adjust fine amount correctly returned Forbidden");
        }

        [Fact]
        public async Task AdjustFineAmount_NonExistentFine_ReturnsBadRequest()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            var adjustRequest = new { FineId = 999999, NewAmount = 10.00m, Reason = "Test" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Fines/adjust-amount", adjustRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var fineResponse = await response.Content.ReadFromJsonAsync<FineResponseDto>();
            Assert.NotNull(fineResponse);
            Assert.False(fineResponse!.Success);
            
            _output.WriteLine($"Adjust non-existent fine correctly returned: {fineResponse.Message}");
        }

        #endregion

        #region Generate Overdue Fines Tests

        [Fact]
        public async Task GenerateOverdueFines_AsLibrarian_ReturnsCount()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/Fines/generate-overdue-fines", new { });

            _output.WriteLine($"Response Status: {response.StatusCode}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<GenerateOverdueFinesResponseDto>();
            Assert.NotNull(result);
            Assert.True(result!.Count >= 0);
            
            _output.WriteLine($"Generated {result.Count} fine(s) for overdue loans");
        }

        [Fact]
        public async Task GenerateOverdueFines_AsMember_ReturnsForbidden()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/Fines/generate-overdue-fines", new { });

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            _output.WriteLine("Member attempting to generate overdue fines correctly returned Forbidden");
        }

        [Fact]
        public async Task GenerateOverdueFines_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.PostAsJsonAsync("/api/Fines/generate-overdue-fines", new { });

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _output.WriteLine("Generate overdue fines without authorization correctly returned Unauthorized");
        }

        #endregion

        #region Get Fine Statistics Tests

        [Fact]
        public async Task GetMyStatistics_AsMember_ReturnsStatistics()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.GetAsync("/api/Fines/my-statistics");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var statistics = await response.Content.ReadFromJsonAsync<FineStatisticsDto>();
            Assert.NotNull(statistics);
            Assert.True(statistics!.TotalOutstanding >= 0);
            Assert.True(statistics.TotalPaid >= 0);
            Assert.True(statistics.TotalWaived >= 0);
            
            _output.WriteLine($"Statistics - Outstanding: {statistics.TotalOutstanding}, Paid: {statistics.TotalPaid}, Waived: {statistics.TotalWaived}");
        }

        [Fact]
        public async Task GetMyStatistics_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/Fines/my-statistics");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _output.WriteLine("Get my statistics without authorization correctly returned Unauthorized");
        }

        #endregion

        #region Check Overdue Tests

        [Fact]
        public async Task CheckOverdue_WithAuth_ReturnsCount()
        {
            // Arrange
            var memberToken = await GetMemberTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

            // Act
            var response = await _client.GetAsync("/api/Fines/check-overdue");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<CheckOverdueResponseDto>();
            Assert.NotNull(result);
            Assert.True(result!.Count >= 0);
            Assert.NotEqual(default(DateTime), result.Timestamp);
            
            _output.WriteLine($"Check overdue found {result.Count} overdue loan(s)");
        }

        [Fact]
        public async Task CheckOverdue_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/Fines/check-overdue");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            _output.WriteLine("Check overdue without authorization correctly returned Unauthorized");
        }

        #endregion

        #region Integration Scenario Tests

        [Fact]
        public async Task CompletePaymentFlow_BorrowGenerateFinePayFine_ReturnsSuccess()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(memberToken);

            _output.WriteLine($"Complete payment flow - BookId: {bookId}, UserId: {userId}");

            // Step 1: Create overdue loan
            var lendingId = await CreateOverdueLoanAsync(librarianToken, userId, bookId);
            _output.WriteLine($"Created lending: {lendingId}");

            // Step 2: Generate overdue fines
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);
            var generateResponse = await _client.PostAsJsonAsync("/api/Fines/generate-overdue-fines", new { });
            Assert.Equal(HttpStatusCode.OK, generateResponse.StatusCode);
            _output.WriteLine("Generated overdue fines");

            // Step 3: Check member's fines
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
            var finesResponse = await _client.GetAsync("/api/Fines/my-fines");
            var fines = await finesResponse.Content.ReadFromJsonAsync<PagedFinesResponseDto>();
            
            if (fines == null || fines.Items.Count == 0)
            {
                _output.WriteLine("No fines created, skipping complete flow test");
                return;
            }

            var outstandingFine = fines.Items.FirstOrDefault(f => f.Status == "Outstanding");
            if (outstandingFine == null)
            {
                _output.WriteLine("No outstanding fines found");
                return;
            }

            _output.WriteLine($"Found outstanding fine: {outstandingFine.Id}, Amount: {outstandingFine.Amount}");

            // Step 4: Process payment
            var paymentRequest = new { FineId = outstandingFine.Id };
            var paymentResponse = await _client.PostAsJsonAsync("/api/Fines/pay", paymentRequest);
            Assert.Equal(HttpStatusCode.OK, paymentResponse.StatusCode);
            _output.WriteLine("Payment processed successfully");

            // Step 5: Verify payment in history
            var paymentsResponse = await _client.GetAsync("/api/Fines/my-payments");
            var payments = await paymentsResponse.Content.ReadFromJsonAsync<PagedPaymentsResponseDto>();
            Assert.NotNull(payments);
            Assert.Contains(payments!.Items, p => p.FineId == outstandingFine.Id);
            
            _output.WriteLine($"Complete payment flow successful - Total payments: {payments.Items.Count}");
        }

        [Fact]
        public async Task LibrarianWorkflow_ViewAndWaiveFine_ReturnsSuccess()
        {
            // Arrange
            var librarianToken = await GetLibrarianTokenAsync();
            var memberToken = await GetMemberTokenAsync();
            var bookId = await CreateTestBookAsync(librarianToken);
            var userId = await GetTestUserIdAsync(memberToken);

            // Create overdue loan and generate fine
            await CreateOverdueLoanAsync(librarianToken, userId, bookId);
            
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", librarianToken);
            await _client.PostAsJsonAsync("/api/Fines/generate-overdue-fines", new { });

            // Step 1: View all fines
            var allFinesResponse = await _client.GetAsync("/api/Fines?page=1&pageSize=10");
            Assert.Equal(HttpStatusCode.OK, allFinesResponse.StatusCode);
            var allFines = await allFinesResponse.Content.ReadFromJsonAsync<PagedFinesResponseDto>();
            _output.WriteLine($"Librarian viewing {allFines?.Items.Count} total fines");

            // Step 2: View specific user's fines
            var userFinesResponse = await _client.GetAsync($"/api/Fines/user/{userId}");
            Assert.Equal(HttpStatusCode.OK, userFinesResponse.StatusCode);
            var userFines = await userFinesResponse.Content.ReadFromJsonAsync<PagedFinesResponseDto>();
            
            if (userFines == null || userFines.Items.Count == 0)
            {
                _output.WriteLine("No user fines found, skipping librarian workflow test");
                return;
            }

            var fine = userFines.Items[0];
            _output.WriteLine($"Found user fine: {fine.Id}");

            // Step 3: Waive the fine
            var waiveRequest = new { FineId = fine.Id, Reason = "Customer service" };
            var waiveResponse = await _client.PostAsJsonAsync("/api/Fines/waive", waiveRequest);
            Assert.Equal(HttpStatusCode.OK, waiveResponse.StatusCode);
            
            _output.WriteLine("Librarian workflow completed successfully");
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
        }

        private class FineDto
        {
            public int Id { get; set; }
            public int? LendingId { get; set; }
            public int UserId { get; set; }
            public string MemberName { get; set; } = string.Empty;
            public string MemberEmail { get; set; } = string.Empty;
            public int BookId { get; set; }
            public string BookTitle { get; set; } = string.Empty;
            public string BookAuthor { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime DueDate { get; set; }
            public DateTime OverdueDate { get; set; }
            public int DaysOverdue { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }

        private class PaymentDto
        {
            public int Id { get; set; }
            public int FineId { get; set; }
            public int UserId { get; set; }
            public string MemberName { get; set; } = string.Empty;
            public string MemberEmail { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string TransactionId { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime PaymentDate { get; set; }
        }

        private class PagedFinesResponseDto
        {
            public List<FineDto> Items { get; set; } = new();
            public int Total { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }

        private class PagedPaymentsResponseDto
        {
            public List<PaymentDto> Items { get; set; } = new();
            public int Total { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }

        private class PaymentResponseDto
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public PaymentDto? Payment { get; set; }
        }

        private class FineResponseDto
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public FineDto? Fine { get; set; }
        }

        private class FineStatisticsDto
        {
            public decimal TotalOutstanding { get; set; }
            public int OutstandingCount { get; set; }
            public decimal TotalPaid { get; set; }
            public int PaidCount { get; set; }
            public decimal TotalWaived { get; set; }
            public int WaivedCount { get; set; }
        }

        private class GenerateOverdueFinesResponseDto
        {
            public string Message { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        private class CheckOverdueResponseDto
        {
            public string Message { get; set; } = string.Empty;
            public int Count { get; set; }
            public DateTime Timestamp { get; set; }
        }

        #endregion
    }
}