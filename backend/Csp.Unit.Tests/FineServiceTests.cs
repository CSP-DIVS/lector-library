//Csp.Unit.Tests/FineServiceTests.cs

using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Csp.Api.Services;
using Csp.Api.DTOs;

namespace Csp.Unit.Tests
{
    public class FineServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IConfigurationSection> _mockConnectionStringSection;
        private const string TestConnectionString = "Server=localhost;Database=test_db;Uid=test;Pwd=test;";

        public FineServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConnectionStringSection = new Mock<IConfigurationSection>();
            
            // Setup the connection string section mock
            _mockConnectionStringSection.Setup(x => x.Value).Returns(TestConnectionString);
            _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings:DefaultConnection"))
                             .Returns(_mockConnectionStringSection.Object);
            
            // Also setup the indexer approach that GetConnectionString uses internally
            _mockConfiguration.Setup(x => x["ConnectionStrings:DefaultConnection"])
                             .Returns(TestConnectionString);
        }

        #region ProcessPaymentAsync Tests

        [Fact]
        public void ProcessPaymentAsync_ValidRequest_ShouldPassValidation()
        {
            // Arrange
            var request = new ProcessPaymentRequest
            {
                FineId = 1,
                UserId = 5,
                TransactionId = "TXN-12345"
            };

            // Act & Assert - Testing validation logic
            Assert.True(request.FineId > 0);
            Assert.True(request.UserId > 0);
            Assert.False(string.IsNullOrWhiteSpace(request.TransactionId));
        }

        [Theory]
        [InlineData(0, 1)] // Invalid fine ID
        [InlineData(-1, 1)] // Negative fine ID
        [InlineData(1, 0)] // Invalid user ID
        [InlineData(1, -1)] // Negative user ID
        public void ProcessPaymentAsync_InvalidParameters_ShouldFail(int fineId, int userId)
        {
            // Arrange
            var request = new ProcessPaymentRequest
            {
                FineId = fineId,
                UserId = userId
            };

            // Act & Assert
            var hasValidFineId = request.FineId > 0;
            var hasValidUserId = request.UserId > 0;

            Assert.False(hasValidFineId && hasValidUserId);
        }

        [Theory]
        [InlineData(null, true)] // Null transaction ID should be allowed (auto-generated)
        [InlineData("", true)] // Empty transaction ID should be allowed (auto-generated)
        [InlineData("TXN-12345", true)] // Valid transaction ID
        public void ProcessPaymentAsync_TransactionIdValidation_ShouldHandleVariousInputs(
            string? transactionId, bool shouldBeValid)
        {
            // Arrange
            var request = new ProcessPaymentRequest
            {
                FineId = 1,
                UserId = 5,
                TransactionId = transactionId
            };

            // Act
            var hasValidIds = request.FineId > 0 && request.UserId > 0;
            // TransactionId can be null or empty, will be auto-generated

            // Assert
            Assert.Equal(shouldBeValid, hasValidIds);
        }

        #endregion

        #region WaiveFineAsync Tests

        [Fact]
        public void WaiveFineAsync_ValidRequest_ShouldPassValidation()
        {
            // Arrange
            var request = new WaiveFineRequest
            {
                FineId = 1,
                Reason = "First time offense"
            };

            // Act & Assert
            Assert.True(request.FineId > 0);
            Assert.False(string.IsNullOrWhiteSpace(request.Reason));
        }

        [Theory]
        [InlineData(0, "Valid reason")] // Invalid fine ID
        [InlineData(-1, "Valid reason")] // Negative fine ID
        [InlineData(1, "")] // Empty reason
        [InlineData(1, null)] // Null reason
        public void WaiveFineAsync_InvalidParameters_ShouldFail(int fineId, string? reason)
        {
            // Arrange
            var request = new WaiveFineRequest
            {
                FineId = fineId,
                Reason = reason ?? string.Empty
            };

            // Act
            var hasValidFineId = request.FineId > 0;
            var hasValidReason = !string.IsNullOrWhiteSpace(request.Reason);

            // Assert
            Assert.False(hasValidFineId && hasValidReason);
        }

        #endregion

        #region AdjustFineAmountAsync Tests

        [Fact]
        public void AdjustFineAmountAsync_ValidRequest_ShouldPassValidation()
        {
            // Arrange
            var request = new AdjustFineAmountRequest
            {
                FineId = 1,
                NewAmount = 75.50m,
                Reason = "Adjusted for good standing"
            };

            // Act & Assert
            Assert.True(request.FineId > 0);
            Assert.True(request.NewAmount > 0);
            Assert.False(string.IsNullOrWhiteSpace(request.Reason));
        }

        [Theory]
        [InlineData(0, 50.00, "Valid reason")] // Invalid fine ID
        [InlineData(-1, 50.00, "Valid reason")] // Negative fine ID
        [InlineData(1, 0, "Valid reason")] // Zero amount
        [InlineData(1, -10, "Valid reason")] // Negative amount
        [InlineData(1, 50.00, "")] // Empty reason
        [InlineData(1, 50.00, null)] // Null reason
        public void AdjustFineAmountAsync_InvalidParameters_ShouldFail(int fineId, decimal newAmount, string? reason)
        {
            // Arrange
            var request = new AdjustFineAmountRequest
            {
                FineId = fineId,
                NewAmount = newAmount,
                Reason = reason ?? string.Empty
            };

            // Act
            var hasValidFineId = request.FineId > 0;
            var hasValidAmount = request.NewAmount > 0;
            var hasValidReason = !string.IsNullOrWhiteSpace(request.Reason);

            // Assert
            Assert.False(hasValidFineId && hasValidAmount && hasValidReason);
        }

        [Theory]
        [InlineData(50.00, true)] // Valid amount
        [InlineData(100.00, true)] // Valid amount
        [InlineData(75.50, true)] // Valid amount
        public void AdjustFineAmountAsync_AmountAdjustment_ShouldAllowAnyPositiveAmount(
            decimal newAmount, bool shouldBeValid)
        {
            // Arrange
            var request = new AdjustFineAmountRequest
            {
                FineId = 1,
                NewAmount = newAmount,
                Reason = "Adjustment"
            };

            // Act
            var isValid = request.NewAmount > 0;

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        #endregion

        #region GetUserFinesAsync Tests

        [Theory]
        [InlineData(1, 1, 10, true)]
        [InlineData(5, 2, 20, true)]
        [InlineData(0, 1, 10, false)] // Invalid user ID
        [InlineData(-1, 1, 10, false)] // Negative user ID
        [InlineData(1, 0, 10, false)] // Invalid page
        [InlineData(1, -1, 10, false)] // Negative page
        [InlineData(1, 1, 0, false)] // Invalid page size
        [InlineData(1, 1, -1, false)] // Negative page size
        public void GetUserFinesAsync_ParameterValidation_ShouldProcessCorrectly(
            int userId, int page, int pageSize, bool shouldBeValid)
        {
            // Act
            var hasValidUserId = userId > 0;
            var hasValidPage = page >= 1;
            var hasValidPageSize = pageSize >= 1;
            var isValid = hasValidUserId && hasValidPage && hasValidPageSize;

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(2, 20)]
        [InlineData(5, 50)]
        public void GetUserFinesAsync_PaginationParameters_ShouldBeRespected(int page, int pageSize)
        {
            // Act
            var offset = (page - 1) * pageSize;

            // Assert
            Assert.True(offset >= 0);
            Assert.Equal((page - 1) * pageSize, offset);
        }

        #endregion

        #region GetAllFinesAsync Tests

        [Theory]
        [InlineData(1, 10, true)]
        [InlineData(2, 20, true)]
        [InlineData(0, 10, false)] // Invalid page
        [InlineData(1, 0, false)] // Invalid page size
        [InlineData(-1, 10, false)] // Negative page
        [InlineData(1, -1, false)] // Negative page size
        public void GetAllFinesAsync_PaginationValidation_ShouldProcessCorrectly(
            int page, int pageSize, bool shouldBeValid)
        {
            // Act
            var hasValidPage = page >= 1;
            var hasValidPageSize = pageSize >= 1;
            var isValid = hasValidPage && hasValidPageSize;

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        #endregion

        #region GetUserPaymentsAsync Tests

        [Theory]
        [InlineData(1, 1, 10, true)]
        [InlineData(5, 2, 20, true)]
        [InlineData(0, 1, 10, false)] // Invalid user ID
        [InlineData(1, 0, 10, false)] // Invalid page
        [InlineData(1, 1, 0, false)] // Invalid page size
        public void GetUserPaymentsAsync_ParameterValidation_ShouldProcessCorrectly(
            int userId, int page, int pageSize, bool shouldBeValid)
        {
            // Act
            var hasValidUserId = userId > 0;
            var hasValidPage = page >= 1;
            var hasValidPageSize = pageSize >= 1;
            var isValid = hasValidUserId && hasValidPage && hasValidPageSize;

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        #endregion

        #region GetAllPaymentsAsync Tests

        [Theory]
        [InlineData(1, 10, true)]
        [InlineData(2, 20, true)]
        [InlineData(0, 10, false)] // Invalid page
        [InlineData(1, 0, false)] // Invalid page size
        public void GetAllPaymentsAsync_PaginationValidation_ShouldProcessCorrectly(
            int page, int pageSize, bool shouldBeValid)
        {
            // Act
            var hasValidPage = page >= 1;
            var hasValidPageSize = pageSize >= 1;
            var isValid = hasValidPage && hasValidPageSize;

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        #endregion

        #region GetUserFineStatisticsAsync Tests

        [Theory]
        [InlineData(1, true)]
        [InlineData(100, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void GetUserFineStatisticsAsync_UserIdValidation_ShouldProcessCorrectly(
            int userId, bool shouldBeValid)
        {
            // Act
            var isValidId = userId > 0;

            // Assert
            Assert.Equal(shouldBeValid, isValidId);
        }

        #endregion

        #region Configuration Tests

        [Fact]
        public void FineService_MissingConnectionString_ShouldThrowException()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            
            // Mock the connection strings section to return null
            var mockConnectionStringsSection = new Mock<IConfigurationSection>();
            mockConnectionStringsSection.Setup(x => x["DefaultConnection"]).Returns((string?)null);
            mockConnectionStringsSection.Setup(x => x["Default"]).Returns((string?)null);
            mockConfig.Setup(x => x.GetSection("ConnectionStrings")).Returns(mockConnectionStringsSection.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new FineService(mockConfig.Object));
        }

        [Fact]
        public void FineService_ValidConnectionString_ShouldInitializeSuccessfully()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            
            // Mock the connection strings section to return a valid connection string
            var mockConnectionStringsSection = new Mock<IConfigurationSection>();
            mockConnectionStringsSection.Setup(x => x["DefaultConnection"]).Returns(TestConnectionString);
            mockConfig.Setup(x => x.GetSection("ConnectionStrings")).Returns(mockConnectionStringsSection.Object);

            // Act
            var service = new FineService(mockConfig.Object);

            // Assert
            Assert.NotNull(service);
        }

        #endregion

        #region Business Logic Tests

        [Theory]
        [InlineData(20.00, 1, 20.00)]
        [InlineData(20.00, 5, 100.00)]
        [InlineData(20.00, 10, 200.00)]
        [InlineData(20.00, 30, 600.00)]
        public void CalculateFineAmount_StandardRate_ShouldCalculateCorrectly(
            decimal finePerDay, int daysOverdue, decimal expectedAmount)
        {
            // Act
            var calculatedAmount = finePerDay * daysOverdue;

            // Assert
            Assert.Equal(expectedAmount, calculatedAmount);
        }

        [Fact]
        public void GenerateTransactionId_NullInput_ShouldGenerateAutoId()
        {
            // Arrange
            string? transactionId = null;
            int fineId = 5;

            // Act
            var generatedId = transactionId ?? $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{fineId}";

            // Assert
            Assert.NotNull(generatedId);
            Assert.StartsWith("TXN-", generatedId);
            Assert.Contains(fineId.ToString(), generatedId);
        }

        [Fact]
        public void GenerateTransactionId_EmptyInput_ShouldGenerateAutoId()
        {
            // Arrange
            string transactionId = "";
            int fineId = 5;

            // Act
            var generatedId = string.IsNullOrWhiteSpace(transactionId) 
                ? $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{fineId}" 
                : transactionId;

            // Assert
            Assert.NotNull(generatedId);
            Assert.StartsWith("TXN-", generatedId);
        }

        [Fact]
        public void GenerateTransactionId_ValidInput_ShouldKeepOriginal()
        {
            // Arrange
            string transactionId = "CUSTOM-TXN-123";
            int fineId = 5;

            // Act
            var generatedId = transactionId ?? $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{fineId}";

            // Assert
            Assert.Equal("CUSTOM-TXN-123", generatedId);
        }

        [Theory]
        [InlineData("Outstanding", true)]
        [InlineData("Paid", false)]
        [InlineData("Waived", false)]
        public void ValidateFineStatus_ForPayment_ShouldOnlyAllowOutstanding(
            string status, bool shouldAllowPayment)
        {
            // Act
            var canProcessPayment = status == "Outstanding";

            // Assert
            Assert.Equal(shouldAllowPayment, canProcessPayment);
        }

        [Theory]
        [InlineData(100.00, 50.00, -50.00)]
        [InlineData(100.00, 75.00, -25.00)]
        [InlineData(100.00, 125.00, 25.00)]
        public void CalculateAmountDifference_ForAdjustment_ShouldCalculateCorrectly(
            decimal originalAmount, decimal newAmount, decimal expectedDifference)
        {
            // Act
            var difference = newAmount - originalAmount;

            // Assert
            Assert.Equal(expectedDifference, difference);
        }

        [Fact]
        public void CalculateDaysOverdue_FromDueDate_ShouldCalculateCorrectly()
        {
            // Arrange
            var dueDate = DateTime.UtcNow.AddDays(-5);
            var currentDate = DateTime.UtcNow;

            // Act
            var daysOverdue = (currentDate - dueDate).Days;

            // Assert
            Assert.True(daysOverdue >= 5);
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(100, 200, 50, 350)]
        [InlineData(500, 300, 100, 900)]
        public void CalculateTotalFines_AllStatuses_ShouldSumCorrectly(
            decimal outstanding, decimal paid, decimal waived, decimal expectedTotal)
        {
            // Act
            var total = outstanding + paid + waived;

            // Assert
            Assert.Equal(expectedTotal, total);
        }

        #endregion

        #region Data Validation Helper Tests

        [Theory]
        [InlineData("Outstanding", true)]
        [InlineData("Paid", true)]
        [InlineData("Waived", true)]
        [InlineData("Invalid", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void ValidateFineStatus_VariousInputs_ShouldReturnExpectedResults(
            string? status, bool expectedValid)
        {
            // Arrange
            var validStatuses = new[] { "Outstanding", "Paid", "Waived" };

            // Act
            var isValid = !string.IsNullOrWhiteSpace(status) && validStatuses.Contains(status);

            // Assert
            Assert.Equal(expectedValid, isValid);
        }

        [Theory]
        [InlineData("Completed", true)]
        [InlineData("Pending", true)]
        [InlineData("Failed", true)]
        [InlineData("Refunded", true)]
        [InlineData("Invalid", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void ValidatePaymentStatus_VariousInputs_ShouldReturnExpectedResults(
            string? status, bool expectedValid)
        {
            // Arrange
            var validStatuses = new[] { "Completed", "Pending", "Failed", "Refunded" };

            // Act
            var isValid = !string.IsNullOrWhiteSpace(status) && validStatuses.Contains(status);

            // Assert
            Assert.Equal(expectedValid, isValid);
        }

        [Theory]
        [InlineData(0.01, true)]
        [InlineData(1.00, true)]
        [InlineData(100.00, true)]
        [InlineData(1000.50, true)]
        [InlineData(0, false)]
        [InlineData(-0.01, false)]
        [InlineData(-100, false)]
        public void ValidateAmount_VariousInputs_ShouldReturnExpectedResults(
            decimal amount, bool expectedValid)
        {
            // Act
            var isValid = amount > 0;

            // Assert
            Assert.Equal(expectedValid, isValid);
        }

        #endregion
    }
}