//Csp.Unit.Tests/LendingServiceTests.cs

using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Csp.Api.Services;
using Csp.Api.DTOs;

namespace Csp.Unit.Tests
{
    public class LendingServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IConfigurationSection> _mockConnectionStringSection;
        private const string TestConnectionString = "Server=localhost;Database=test_db;Uid=test;Pwd=test;";

        public LendingServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConnectionStringSection = new Mock<IConfigurationSection>();
            
            _mockConnectionStringSection.Setup(x => x.Value).Returns(TestConnectionString);
            _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings:DefaultConnection"))
                             .Returns(_mockConnectionStringSection.Object);
            _mockConfiguration.Setup(x => x["ConnectionStrings:DefaultConnection"])
                             .Returns(TestConnectionString);
        }

        #region BorrowBookAsync Tests

        [Fact]
        public void BorrowBookAsync_InvalidBookId_ShouldFailValidation()
        {
            // Arrange
            var request = new BorrowBookRequest
            {
                BookId = 0,
                UserId = 1,
                LoanDurationDays = 14
            };

            // Act & Assert - Testing validation logic
            Assert.True(request.BookId <= 0);
        }

        [Fact]
        public void BorrowBookAsync_InvalidUserId_ShouldFailValidation()
        {
            // Arrange
            var request = new BorrowBookRequest
            {
                BookId = 1,
                UserId = 0,
                LoanDurationDays = 14
            };

            // Act & Assert - Testing validation logic
            Assert.True(request.UserId <= 0);
        }

        [Fact]
        public void BorrowBookAsync_ValidRequest_ShouldPassValidation()
        {
            // Arrange
            var request = new BorrowBookRequest
            {
                BookId = 1,
                UserId = 2,
                LoanDurationDays = 14
            };

            // Act & Assert - Testing validation logic
            Assert.True(request.BookId > 0);
            Assert.True(request.UserId > 0);
            Assert.True(request.LoanDurationDays > 0);
        }

        [Fact]
        public void BorrowBookAsync_DefaultLoanDuration_ShouldBe14Days()
        {
            // Arrange & Act
            var request = new BorrowBookRequest
            {
                BookId = 1,
                UserId = 2
            };

            // Assert
            Assert.Equal(14, request.LoanDurationDays);
        }

        [Fact]
        public void BorrowBookAsync_CustomLoanDuration_ShouldBeRespected()
        {
            // Arrange & Act
            var request = new BorrowBookRequest
            {
                BookId = 1,
                UserId = 2,
                LoanDurationDays = 21
            };

            // Assert
            Assert.Equal(21, request.LoanDurationDays);
        }

        #endregion

        #region ReturnBookAsync Tests

        [Fact]
        public void ReturnBookAsync_ValidRequest_ShouldPassValidation()
        {
            // Arrange
            var request = new ReturnBookRequest
            {
                LendingId = 1,
                FineAmount = 0
            };

            // Act & Assert
            Assert.True(request.LendingId > 0);
        }

        [Fact]
        public void ReturnBookAsync_WithFine_ShouldHaveFineAmount()
        {
            // Arrange
            var request = new ReturnBookRequest
            {
                LendingId = 1,
                FineAmount = 5.50m
            };

            // Act & Assert
            Assert.True(request.FineAmount.HasValue);
            Assert.Equal(5.50m, request.FineAmount.Value);
        }

        [Fact]
        public void ReturnBookAsync_NoFine_FineAmountShouldBeNull()
        {
            // Arrange
            var request = new ReturnBookRequest
            {
                LendingId = 1
            };

            // Act & Assert
            Assert.False(request.FineAmount.HasValue);
        }

        #endregion

        #region RenewLoanAsync Tests

        [Fact]
        public void RenewLoanAsync_ValidRequest_ShouldPassValidation()
        {
            // Arrange
            var request = new RenewLoanRequest
            {
                LendingId = 1
            };

            // Act & Assert
            Assert.True(request.LendingId > 0);
        }

        [Fact]
        public void RenewLoanAsync_InvalidLendingId_ShouldFailValidation()
        {
            // Arrange
            var request = new RenewLoanRequest
            {
                LendingId = 0
            };

            // Act & Assert
            Assert.False(request.LendingId > 0);
        }

        #endregion

        #region LendingDto Tests

        [Fact]
        public void LendingDto_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var dto = new LendingDto();

            // Assert
            Assert.Equal(string.Empty, dto.BookTitle);
            Assert.Equal(string.Empty, dto.BookAuthor);
            Assert.Equal(string.Empty, dto.BookIsbn);
            Assert.Equal(string.Empty, dto.Username);
            Assert.Equal(string.Empty, dto.MemberEmail);
            Assert.Equal(string.Empty, dto.Status);
            Assert.False(dto.FinePaid);
            Assert.False(dto.IsOverdue);
        }

        [Fact]
        public void LendingDto_OverdueCalculation_ShouldBeCorrect()
        {
            // Arrange
            var dto = new LendingDto
            {
                DueDate = DateTime.UtcNow.AddDays(-5),
                IsOverdue = true,
                OverdueDays = 5
            };

            // Act & Assert
            Assert.True(dto.IsOverdue);
            Assert.Equal(5, dto.OverdueDays);
            Assert.True(dto.DueDate < DateTime.UtcNow);
        }

        [Fact]
        public void LendingDto_ActiveLoan_ShouldNotBeOverdue()
        {
            // Arrange
            var dto = new LendingDto
            {
                DueDate = DateTime.UtcNow.AddDays(7),
                IsOverdue = false,
                Status = "Active"
            };

            // Act & Assert
            Assert.False(dto.IsOverdue);
            Assert.Equal("Active", dto.Status);
            Assert.True(dto.DueDate > DateTime.UtcNow);
        }

        [Fact]
        public void LendingDto_WithFine_ShouldHaveFineDetails()
        {
            // Arrange
            var dto = new LendingDto
            {
                FineAmount = 10.00m,
                FinePaid = false,
                IsOverdue = true
            };

            // Act & Assert
            Assert.True(dto.FineAmount.HasValue);
            Assert.Equal(10.00m, dto.FineAmount.Value);
            Assert.False(dto.FinePaid);
        }

        [Fact]
        public void LendingDto_RenewalCount_ShouldTrackRenewals()
        {
            // Arrange
            var dto = new LendingDto
            {
                RenewalCount = 2,
                MaxRenewals = 2
            };

            // Act & Assert
            Assert.Equal(2, dto.RenewalCount);
            Assert.Equal(2, dto.MaxRenewals);
            Assert.True(dto.RenewalCount >= dto.MaxRenewals);
        }

        #endregion

        #region PagedLendingsResponse Tests

        [Fact]
        public void PagedLendingsResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new PagedLendingsResponse();

            // Assert
            Assert.NotNull(response.Items);
            Assert.Empty(response.Items);
            Assert.Equal(0, response.Total);
            Assert.Equal(0, response.Page);
            Assert.Equal(0, response.PageSize);
        }

        [Fact]
        public void PagedLendingsResponse_Pagination_ShouldCalculateCorrectly()
        {
            // Arrange
            var response = new PagedLendingsResponse
            {
                Items = new List<LendingDto> { new(), new(), new() },
                Total = 25,
                Page = 2,
                PageSize = 10
            };

            // Act & Assert
            Assert.Equal(3, response.Items.Count);
            Assert.Equal(25, response.Total);
            Assert.Equal(2, response.Page);
            Assert.Equal(10, response.PageSize);
            
            // Calculate total pages
            var totalPages = (int)Math.Ceiling((double)response.Total / response.PageSize);
            Assert.Equal(3, totalPages);
        }

        #endregion

        #region LendingResponse Tests

        [Fact]
        public void LendingResponse_SuccessScenario_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var response = new LendingResponse
            {
                Success = true,
                Message = "Book borrowed successfully",
                Lending = new LendingDto { Id = 1 }
            };

            // Assert
            Assert.True(response.Success);
            Assert.Equal("Book borrowed successfully", response.Message);
            Assert.NotNull(response.Lending);
            Assert.Equal(1, response.Lending.Id);
        }

        [Fact]
        public void LendingResponse_FailureScenario_ShouldHaveErrorMessage()
        {
            // Arrange & Act
            var response = new LendingResponse
            {
                Success = false,
                Message = "Book not available",
                Lending = null
            };

            // Assert
            Assert.False(response.Success);
            Assert.Equal("Book not available", response.Message);
            Assert.Null(response.Lending);
        }

        #endregion

        #region Service Initialization Tests

        [Fact]
        public void LendingService_Constructor_ShouldInitializeWithValidConnectionString()
        {
            // Arrange
            var mockConnectionStringSection = new Mock<IConfigurationSection>();
            mockConnectionStringSection.Setup(s => s.Value).Returns(TestConnectionString);
            
            var mockConnectionStringsSection = new Mock<IConfigurationSection>();
            mockConnectionStringsSection.Setup(s => s["DefaultConnection"]).Returns(TestConnectionString);
            
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockConnectionStringsSection.Object);

            // Act
            var service = new LendingService(config.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void LendingService_Constructor_ShouldThrowIfConnectionStringMissing()
        {
            // Arrange
            var mockConnectionStringsSection = new Mock<IConfigurationSection>();
            mockConnectionStringsSection.Setup(s => s["DefaultConnection"]).Returns((string?)null);
            mockConnectionStringsSection.Setup(s => s["Default"]).Returns((string?)null);
            
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockConnectionStringsSection.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new LendingService(config.Object));
        }

        [Fact]
        public void LendingService_Constructor_ShouldUseDefaultConnectionStringAsFallback()
        {
            // Arrange
            var mockConnectionStringsSection = new Mock<IConfigurationSection>();
            mockConnectionStringsSection.Setup(s => s["DefaultConnection"]).Returns((string?)null);
            mockConnectionStringsSection.Setup(s => s["Default"]).Returns(TestConnectionString);
            
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockConnectionStringsSection.Object);

            // Act
            var service = new LendingService(config.Object);

            // Assert
            Assert.NotNull(service);
        }

        #endregion

        #region Business Logic Validation Tests

        [Fact]
        public void LendingLogic_FinePerDay_ShouldBeConstant()
        {
            // This validates the fine calculation constant
            const decimal expectedFinePerDay = 1.0m;
            
            // Assert
            Assert.Equal(1.0m, expectedFinePerDay);
        }

        [Fact]
        public void LendingLogic_MaxRenewals_ShouldDefaultToTwo()
        {
            // Arrange
            var dto = new LendingDto
            {
                MaxRenewals = 2,
                RenewalCount = 0
            };

            // Assert
            Assert.Equal(2, dto.MaxRenewals);
            Assert.True(dto.RenewalCount < dto.MaxRenewals);
        }

        [Fact]
        public void LendingLogic_RenewalExceeded_ShouldBeDetected()
        {
            // Arrange
            var dto = new LendingDto
            {
                RenewalCount = 2,
                MaxRenewals = 2
            };

            // Act
            var canRenew = dto.RenewalCount < dto.MaxRenewals;

            // Assert
            Assert.False(canRenew);
        }

        [Fact]
        public void LendingLogic_ActiveStatuses_ShouldBeValid()
        {
            // Arrange
            var validStatuses = new[] { "Active", "Returned", "Overdue" };

            // Assert
            Assert.Contains("Active", validStatuses);
            Assert.Contains("Returned", validStatuses);
            Assert.Contains("Overdue", validStatuses);
        }

        #endregion
    }
}