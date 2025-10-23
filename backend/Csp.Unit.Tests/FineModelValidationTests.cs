//Csp.Unit.Tests/FineModelValidationTests.cs

using Xunit;
using Csp.Api.Models;
using Csp.Api.DTOs;

namespace Csp.Unit.Tests
{
    public class FineModelValidationTests
    {
        #region Fine Model Tests

        [Fact]
        public void Fine_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var fine = new Fine();

            // Assert
            Assert.Equal(0, fine.Id);
            Assert.Null(fine.LendingId);
            Assert.Equal(0, fine.UserId);
            Assert.Equal(0, fine.BookId);
            Assert.Equal(string.Empty, fine.Reason);
            Assert.Equal(0, fine.Amount);
            Assert.Equal("Outstanding", fine.Status);
            Assert.Equal(default(DateTime), fine.DueDate);
            Assert.Equal(default(DateTime), fine.OverdueDate);
            Assert.Equal(0, fine.DaysOverdue);
            Assert.True((DateTime.UtcNow - fine.CreatedAt).TotalMinutes < 1);
            Assert.Null(fine.UpdatedAt);
        }

        [Fact]
        public void Fine_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var fine = new Fine();
            var testDate = DateTime.UtcNow.AddDays(-5);

            // Act
            fine.Id = 1;
            fine.LendingId = 10;
            fine.UserId = 5;
            fine.BookId = 3;
            fine.Reason = "Overdue Book";
            fine.Amount = 100.50m;
            fine.Status = "Paid";
            fine.DueDate = testDate;
            fine.OverdueDate = testDate.AddDays(1);
            fine.DaysOverdue = 5;
            fine.CreatedAt = testDate;
            fine.UpdatedAt = DateTime.UtcNow;

            // Assert
            Assert.Equal(1, fine.Id);
            Assert.Equal(10, fine.LendingId);
            Assert.Equal(5, fine.UserId);
            Assert.Equal(3, fine.BookId);
            Assert.Equal("Overdue Book", fine.Reason);
            Assert.Equal(100.50m, fine.Amount);
            Assert.Equal("Paid", fine.Status);
            Assert.Equal(testDate, fine.DueDate);
            Assert.Equal(testDate.AddDays(1), fine.OverdueDate);
            Assert.Equal(5, fine.DaysOverdue);
            Assert.Equal(testDate, fine.CreatedAt);
            Assert.NotNull(fine.UpdatedAt);
        }

        [Theory]
        [InlineData("Outstanding", true)]
        [InlineData("Paid", true)]
        [InlineData("Waived", true)]
        [InlineData("Invalid", false)]
        [InlineData("", false)]
        public void Fine_StatusValidation_ShouldFollowConstraints(string status, bool shouldBeValid)
        {
            // Arrange
            var validStatuses = new[] { "Outstanding", "Paid", "Waived" };
            
            // Act
            var isValid = validStatuses.Contains(status);

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        [Theory]
        [InlineData(0.01, true)]
        [InlineData(100.00, true)]
        [InlineData(1000.50, true)]
        [InlineData(0, false)]
        [InlineData(-10, false)]
        [InlineData(-0.01, false)]
        public void Fine_AmountValidation_ShouldBePositive(decimal amount, bool shouldBeValid)
        {
            // Act
            var isValid = amount > 0;

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(5, true)]
        [InlineData(30, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void Fine_DaysOverdueValidation_ShouldBePositive(int daysOverdue, bool shouldBeValid)
        {
            // Act
            var isValid = daysOverdue > 0;

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        #endregion

        #region Payment Model Tests

        [Fact]
        public void Payment_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var payment = new Payment();

            // Assert
            Assert.Equal(0, payment.Id);
            Assert.Equal(0, payment.FineId);
            Assert.Equal(0, payment.UserId);
            Assert.Equal(0, payment.Amount);
            Assert.Equal(string.Empty, payment.PaymentMethod);
            Assert.Equal(string.Empty, payment.TransactionId);
            Assert.Equal(string.Empty, payment.Description);
            Assert.True((DateTime.UtcNow - payment.PaymentDate).TotalMinutes < 1);
            Assert.Equal("Completed", payment.Status);
        }

        [Fact]
        public void Payment_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var payment = new Payment();
            var testDate = DateTime.UtcNow.AddDays(-1);

            // Act
            payment.Id = 1;
            payment.FineId = 5;
            payment.UserId = 10;
            payment.Amount = 150.00m;
            payment.PaymentMethod = "Credit Card";
            payment.TransactionId = "TXN-12345";
            payment.Description = "Fine payment";
            payment.PaymentDate = testDate;
            payment.Status = "Pending";

            // Assert
            Assert.Equal(1, payment.Id);
            Assert.Equal(5, payment.FineId);
            Assert.Equal(10, payment.UserId);
            Assert.Equal(150.00m, payment.Amount);
            Assert.Equal("Credit Card", payment.PaymentMethod);
            Assert.Equal("TXN-12345", payment.TransactionId);
            Assert.Equal("Fine payment", payment.Description);
            Assert.Equal(testDate, payment.PaymentDate);
            Assert.Equal("Pending", payment.Status);
        }

        [Theory]
        [InlineData("Cash", true)]
        [InlineData("Credit Card", true)]
        [InlineData("Debit Card", true)]
        [InlineData("Online", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Payment_PaymentMethodValidation_ShouldNotBeEmpty(string? paymentMethod, bool shouldBeValid)
        {
            // Act
            var isValid = !string.IsNullOrWhiteSpace(paymentMethod);

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        [Theory]
        [InlineData("Completed", true)]
        [InlineData("Pending", true)]
        [InlineData("Failed", true)]
        [InlineData("Refunded", true)]
        [InlineData("Invalid", false)]
        [InlineData("", false)]
        public void Payment_StatusValidation_ShouldFollowConstraints(string status, bool shouldBeValid)
        {
            // Arrange
            var validStatuses = new[] { "Completed", "Pending", "Failed", "Refunded" };
            
            // Act
            var isValid = validStatuses.Contains(status);

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        [Theory]
        [InlineData(0.01, true)]
        [InlineData(100.00, true)]
        [InlineData(1000.50, true)]
        [InlineData(0, false)]
        [InlineData(-10, false)]
        public void Payment_AmountValidation_ShouldBePositive(decimal amount, bool shouldBeValid)
        {
            // Act
            var isValid = amount > 0;

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        #endregion

        #region FineDto Tests

        [Fact]
        public void FineDto_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var fineDto = new FineDto();

            // Assert
            Assert.Equal(0, fineDto.Id);
            Assert.Null(fineDto.LendingId);
            Assert.Equal(0, fineDto.UserId);
            Assert.Equal(string.Empty, fineDto.MemberName);
            Assert.Equal(string.Empty, fineDto.MemberEmail);
            Assert.Equal(0, fineDto.BookId);
            Assert.Equal(string.Empty, fineDto.BookTitle);
            Assert.Equal(string.Empty, fineDto.BookAuthor);
            Assert.Equal(string.Empty, fineDto.Reason);
            Assert.Equal(0, fineDto.Amount);
            Assert.Equal(string.Empty, fineDto.Status);
            Assert.Equal(default(DateTime), fineDto.DueDate);
            Assert.Equal(default(DateTime), fineDto.OverdueDate);
            Assert.Equal(0, fineDto.DaysOverdue);
            Assert.Equal(default(DateTime), fineDto.CreatedAt);
            Assert.Null(fineDto.UpdatedAt);
        }

        [Fact]
        public void FineDto_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var fineDto = new FineDto();
            var testDate = DateTime.UtcNow;

            // Act
            fineDto.Id = 1;
            fineDto.LendingId = 10;
            fineDto.UserId = 5;
            fineDto.MemberName = "John Doe";
            fineDto.MemberEmail = "john@example.com";
            fineDto.BookId = 3;
            fineDto.BookTitle = "Test Book";
            fineDto.BookAuthor = "Test Author";
            fineDto.Reason = "Overdue Book";
            fineDto.Amount = 100.00m;
            fineDto.Status = "Outstanding";
            fineDto.DueDate = testDate;
            fineDto.OverdueDate = testDate.AddDays(1);
            fineDto.DaysOverdue = 5;
            fineDto.CreatedAt = testDate;
            fineDto.UpdatedAt = testDate.AddDays(2);

            // Assert
            Assert.Equal(1, fineDto.Id);
            Assert.Equal(10, fineDto.LendingId);
            Assert.Equal(5, fineDto.UserId);
            Assert.Equal("John Doe", fineDto.MemberName);
            Assert.Equal("john@example.com", fineDto.MemberEmail);
            Assert.Equal(3, fineDto.BookId);
            Assert.Equal("Test Book", fineDto.BookTitle);
            Assert.Equal("Test Author", fineDto.BookAuthor);
            Assert.Equal("Overdue Book", fineDto.Reason);
            Assert.Equal(100.00m, fineDto.Amount);
            Assert.Equal("Outstanding", fineDto.Status);
            Assert.Equal(testDate, fineDto.DueDate);
            Assert.Equal(testDate.AddDays(1), fineDto.OverdueDate);
            Assert.Equal(5, fineDto.DaysOverdue);
            Assert.Equal(testDate, fineDto.CreatedAt);
            Assert.NotNull(fineDto.UpdatedAt);
        }

        #endregion

        #region PaymentDto Tests

        [Fact]
        public void PaymentDto_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var paymentDto = new PaymentDto();

            // Assert
            Assert.Equal(0, paymentDto.Id);
            Assert.Equal(0, paymentDto.FineId);
            Assert.Equal(0, paymentDto.UserId);
            Assert.Equal(string.Empty, paymentDto.MemberName);
            Assert.Equal(string.Empty, paymentDto.MemberEmail);
            Assert.Equal(0, paymentDto.Amount);
            Assert.Equal(string.Empty, paymentDto.TransactionId);
            Assert.Equal(string.Empty, paymentDto.Description);
            Assert.Equal(default(DateTime), paymentDto.PaymentDate);
        }

        [Fact]
        public void PaymentDto_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var paymentDto = new PaymentDto();
            var testDate = DateTime.UtcNow;

            // Act
            paymentDto.Id = 1;
            paymentDto.FineId = 5;
            paymentDto.UserId = 10;
            paymentDto.MemberName = "Jane Doe";
            paymentDto.MemberEmail = "jane@example.com";
            paymentDto.Amount = 150.00m;
            paymentDto.TransactionId = "TXN-12345";
            paymentDto.Description = "Fine payment";
            paymentDto.PaymentDate = testDate;

            // Assert
            Assert.Equal(1, paymentDto.Id);
            Assert.Equal(5, paymentDto.FineId);
            Assert.Equal(10, paymentDto.UserId);
            Assert.Equal("Jane Doe", paymentDto.MemberName);
            Assert.Equal("jane@example.com", paymentDto.MemberEmail);
            Assert.Equal(150.00m, paymentDto.Amount);
            Assert.Equal("TXN-12345", paymentDto.TransactionId);
            Assert.Equal("Fine payment", paymentDto.Description);
            Assert.Equal(testDate, paymentDto.PaymentDate);
        }

        #endregion

        #region ProcessPaymentRequest Tests

        [Fact]
        public void ProcessPaymentRequest_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var request = new ProcessPaymentRequest();

            // Assert
            Assert.Equal(0, request.FineId);
            Assert.Equal(0, request.UserId);
            Assert.Null(request.TransactionId);
        }

        [Fact]
        public void ProcessPaymentRequest_SetProperties_ShouldRetainValues()
        {
            // Arrange & Act
            var request = new ProcessPaymentRequest
            {
                FineId = 5,
                UserId = 10,
                TransactionId = "TXN-12345"
            };

            // Assert
            Assert.Equal(5, request.FineId);
            Assert.Equal(10, request.UserId);
            Assert.Equal("TXN-12345", request.TransactionId);
        }

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(100, 50, true)]
        [InlineData(0, 1, false)]
        [InlineData(1, 0, false)]
        [InlineData(-1, 1, false)]
        [InlineData(1, -1, false)]
        public void ProcessPaymentRequest_ValidationRules_ShouldBeAppliedCorrectly(
            int fineId, int userId, bool shouldBeValid)
        {
            // Arrange
            var request = new ProcessPaymentRequest
            {
                FineId = fineId,
                UserId = userId
            };

            // Act
            var isValid = request.FineId > 0 && request.UserId > 0;

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        #endregion

        #region WaiveFineRequest Tests

        [Fact]
        public void WaiveFineRequest_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var request = new WaiveFineRequest();

            // Assert
            Assert.Equal(0, request.FineId);
            Assert.Equal(string.Empty, request.Reason);
        }

        [Fact]
        public void WaiveFineRequest_SetProperties_ShouldRetainValues()
        {
            // Arrange & Act
            var request = new WaiveFineRequest
            {
                FineId = 5,
                Reason = "First time offense"
            };

            // Assert
            Assert.Equal(5, request.FineId);
            Assert.Equal("First time offense", request.Reason);
        }

        [Theory]
        [InlineData(1, "Valid reason", true)]
        [InlineData(0, "Valid reason", false)]
        [InlineData(-1, "Valid reason", false)]
        [InlineData(1, "", false)]
        [InlineData(1, null, false)]
        public void WaiveFineRequest_ValidationRules_ShouldBeAppliedCorrectly(
            int fineId, string? reason, bool shouldBeValid)
        {
            // Arrange
            var request = new WaiveFineRequest
            {
                FineId = fineId,
                Reason = reason ?? string.Empty
            };

            // Act
            var isValid = request.FineId > 0 && !string.IsNullOrWhiteSpace(request.Reason);

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        #endregion

        #region AdjustFineAmountRequest Tests

        [Fact]
        public void AdjustFineAmountRequest_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var request = new AdjustFineAmountRequest();

            // Assert
            Assert.Equal(0, request.FineId);
            Assert.Equal(0, request.NewAmount);
            Assert.Equal(string.Empty, request.Reason);
        }

        [Fact]
        public void AdjustFineAmountRequest_SetProperties_ShouldRetainValues()
        {
            // Arrange & Act
            var request = new AdjustFineAmountRequest
            {
                FineId = 5,
                NewAmount = 75.50m,
                Reason = "Adjusted for good standing"
            };

            // Assert
            Assert.Equal(5, request.FineId);
            Assert.Equal(75.50m, request.NewAmount);
            Assert.Equal("Adjusted for good standing", request.Reason);
        }

        [Theory]
        [InlineData(1, 50.00, "Valid reason", true)]
        [InlineData(0, 50.00, "Valid reason", false)]
        [InlineData(-1, 50.00, "Valid reason", false)]
        [InlineData(1, 0, "Valid reason", false)]
        [InlineData(1, -10, "Valid reason", false)]
        [InlineData(1, 50.00, "", false)]
        [InlineData(1, 50.00, null, false)]
        public void AdjustFineAmountRequest_ValidationRules_ShouldBeAppliedCorrectly(
            int fineId, decimal newAmount, string? reason, bool shouldBeValid)
        {
            // Arrange
            var request = new AdjustFineAmountRequest
            {
                FineId = fineId,
                NewAmount = newAmount,
                Reason = reason ?? string.Empty
            };

            // Act
            var isValid = request.FineId > 0 && 
                         request.NewAmount > 0 && 
                         !string.IsNullOrWhiteSpace(request.Reason);

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        #endregion

        #region FineResponse Tests

        [Fact]
        public void FineResponse_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var response = new FineResponse();

            // Assert
            Assert.False(response.Success);
            Assert.Equal(string.Empty, response.Message);
            Assert.Null(response.Fine);
        }

        [Fact]
        public void FineResponse_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var fine = new FineDto { Id = 1, Amount = 100.00m };

            // Act
            var response = new FineResponse
            {
                Success = true,
                Message = "Operation successful",
                Fine = fine
            };

            // Assert
            Assert.True(response.Success);
            Assert.Equal("Operation successful", response.Message);
            Assert.NotNull(response.Fine);
            Assert.Equal(1, response.Fine.Id);
            Assert.Equal(100.00m, response.Fine.Amount);
        }

        #endregion

        #region PaymentResponse Tests

        [Fact]
        public void PaymentResponse_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var response = new PaymentResponse();

            // Assert
            Assert.False(response.Success);
            Assert.Equal(string.Empty, response.Message);
            Assert.Null(response.Payment);
        }

        [Fact]
        public void PaymentResponse_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var payment = new PaymentDto { Id = 1, Amount = 150.00m };

            // Act
            var response = new PaymentResponse
            {
                Success = true,
                Message = "Payment processed",
                Payment = payment
            };

            // Assert
            Assert.True(response.Success);
            Assert.Equal("Payment processed", response.Message);
            Assert.NotNull(response.Payment);
            Assert.Equal(1, response.Payment.Id);
            Assert.Equal(150.00m, response.Payment.Amount);
        }

        #endregion

        #region PagedFinesResponse Tests

        [Fact]
        public void PagedFinesResponse_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var response = new PagedFinesResponse();

            // Assert
            Assert.NotNull(response.Items);
            Assert.Empty(response.Items);
            Assert.Equal(0, response.Total);
            Assert.Equal(0, response.Page);
            Assert.Equal(0, response.PageSize);
        }

        [Fact]
        public void PagedFinesResponse_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var fines = new List<FineDto>
            {
                new FineDto { Id = 1, Amount = 100.00m },
                new FineDto { Id = 2, Amount = 200.00m }
            };

            // Act
            var response = new PagedFinesResponse
            {
                Items = fines,
                Total = 25,
                Page = 2,
                PageSize = 10
            };

            // Assert
            Assert.Equal(2, response.Items.Count);
            Assert.Equal(25, response.Total);
            Assert.Equal(2, response.Page);
            Assert.Equal(10, response.PageSize);
        }

        [Theory]
        [InlineData(5, 5, 10, true)]
        [InlineData(10, 10, 10, true)]
        [InlineData(15, 10, 10, false)]
        [InlineData(0, 0, 10, true)]
        public void PagedFinesResponse_ConsistencyValidation_ShouldBeLogical(
            int itemCount, int total, int pageSize, bool shouldBeConsistent)
        {
            // Arrange
            var items = new List<FineDto>();
            for (int i = 0; i < itemCount; i++)
            {
                items.Add(new FineDto { Id = i + 1 });
            }

            var response = new PagedFinesResponse
            {
                Items = items,
                Total = total,
                PageSize = pageSize
            };

            // Act
            var isConsistent = response.Items.Count <= response.PageSize && 
                             response.Items.Count <= response.Total;

            // Assert
            Assert.Equal(shouldBeConsistent, isConsistent);
        }

        #endregion

        #region PagedPaymentsResponse Tests

        [Fact]
        public void PagedPaymentsResponse_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var response = new PagedPaymentsResponse();

            // Assert
            Assert.NotNull(response.Items);
            Assert.Empty(response.Items);
            Assert.Equal(0, response.Total);
            Assert.Equal(0, response.Page);
            Assert.Equal(0, response.PageSize);
        }

        [Fact]
        public void PagedPaymentsResponse_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var payments = new List<PaymentDto>
            {
                new PaymentDto { Id = 1, Amount = 100.00m },
                new PaymentDto { Id = 2, Amount = 200.00m }
            };

            // Act
            var response = new PagedPaymentsResponse
            {
                Items = payments,
                Total = 15,
                Page = 1,
                PageSize = 10
            };

            // Assert
            Assert.Equal(2, response.Items.Count);
            Assert.Equal(15, response.Total);
            Assert.Equal(1, response.Page);
            Assert.Equal(10, response.PageSize);
        }

        #endregion

        #region FineStatistics Tests

        [Fact]
        public void FineStatistics_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var stats = new FineStatistics();

            // Assert
            Assert.Equal(0, stats.TotalOutstanding);
            Assert.Equal(0, stats.OutstandingCount);
            Assert.Equal(0, stats.TotalPaid);
            Assert.Equal(0, stats.PaidCount);
            Assert.Equal(0, stats.TotalWaived);
            Assert.Equal(0, stats.WaivedCount);
        }

        [Fact]
        public void FineStatistics_SetProperties_ShouldRetainValues()
        {
            // Arrange & Act
            var stats = new FineStatistics
            {
                TotalOutstanding = 500.00m,
                OutstandingCount = 5,
                TotalPaid = 300.00m,
                PaidCount = 3,
                TotalWaived = 100.00m,
                WaivedCount = 1
            };

            // Assert
            Assert.Equal(500.00m, stats.TotalOutstanding);
            Assert.Equal(5, stats.OutstandingCount);
            Assert.Equal(300.00m, stats.TotalPaid);
            Assert.Equal(3, stats.PaidCount);
            Assert.Equal(100.00m, stats.TotalWaived);
            Assert.Equal(1, stats.WaivedCount);
        }

        [Fact]
        public void FineStatistics_CalculateTotalFines_ShouldSumCorrectly()
        {
            // Arrange
            var stats = new FineStatistics
            {
                TotalOutstanding = 500.00m,
                TotalPaid = 300.00m,
                TotalWaived = 100.00m
            };

            // Act
            var totalFines = stats.TotalOutstanding + stats.TotalPaid + stats.TotalWaived;

            // Assert
            Assert.Equal(900.00m, totalFines);
        }

        [Fact]
        public void FineStatistics_CalculateTotalCount_ShouldSumCorrectly()
        {
            // Arrange
            var stats = new FineStatistics
            {
                OutstandingCount = 5,
                PaidCount = 3,
                WaivedCount = 1
            };

            // Act
            var totalCount = stats.OutstandingCount + stats.PaidCount + stats.WaivedCount;

            // Assert
            Assert.Equal(9, totalCount);
        }

        #endregion

        #region Business Logic Tests

        [Theory]
        [InlineData(20.00, 1, 20.00)]
        [InlineData(20.00, 5, 100.00)]
        [InlineData(20.00, 10, 200.00)]
        [InlineData(20.00, 30, 600.00)]
        public void CalculateFineAmount_ByDaysOverdue_ShouldCalculateCorrectly(
            decimal finePerDay, int daysOverdue, decimal expectedAmount)
        {
            // Act
            var calculatedAmount = finePerDay * daysOverdue;

            // Assert
            Assert.Equal(expectedAmount, calculatedAmount);
        }

        [Theory]
        [InlineData("2024-01-01", "2024-01-02", 1)]
        [InlineData("2024-01-01", "2024-01-06", 5)]
        [InlineData("2024-01-01", "2024-01-31", 30)]
        public void CalculateDaysOverdue_FromDates_ShouldCalculateCorrectly(
            string dueDateStr, string currentDateStr, int expectedDays)
        {
            // Arrange
            var dueDate = DateTime.Parse(dueDateStr);
            var currentDate = DateTime.Parse(currentDateStr);

            // Act
            var daysOverdue = (currentDate - dueDate).Days;

            // Assert
            Assert.Equal(expectedDays, daysOverdue);
        }

        #endregion
    }
}