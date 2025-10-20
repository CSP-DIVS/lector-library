using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Csp.Api.DTOs;
using Xunit;

namespace Csp.Unit.Tests
{
    public class PaymentDtoValidationTests
    {
        #region RecordPaymentRequest Tests

        [Fact]
        public void RecordPaymentRequest_DefaultValues_ShouldBeSetCorrectly()
        {
            // Act
            var request = new RecordPaymentRequest();

            // Assert
            Assert.Equal(0, request.MemberId);
            Assert.Equal(0, request.LendingId);
            Assert.Equal(0, request.Amount);
            Assert.Equal("Cash", request.PaymentMethod);
            Assert.Null(request.PaymentDate);
        }

        [Fact]
        public void RecordPaymentRequest_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var request = new RecordPaymentRequest();
            var now = DateTime.UtcNow;

            // Act
            request.MemberId = 1;
            request.LendingId = 2;
            request.Amount = 25.50m;
            request.PaymentMethod = "Credit Card";
            request.PaymentDate = now;

            // Assert
            Assert.Equal(1, request.MemberId);
            Assert.Equal(2, request.LendingId);
            Assert.Equal(25.50m, request.Amount);
            Assert.Equal("Credit Card", request.PaymentMethod);
            Assert.Equal(now, request.PaymentDate);
        }

        [Theory]
        [InlineData(1, 1, 0.01, "Cash", true)]
        [InlineData(1, 1, 999999.99, "Credit Card", true)]
        [InlineData(0, 1, 10.00, "Cash", false)] // MemberId required
        [InlineData(1, 0, 10.00, "Cash", false)] // LendingId required
        [InlineData(1, 1, 0.00, "Cash", false)] // Amount too low
        [InlineData(1, 1, -1.00, "Cash", false)] // Negative amount
        [InlineData(1, 1, 1000000.00, "Cash", false)] // Amount too high
        [InlineData(1, 1, 10.00, "", false)] // PaymentMethod required
        [InlineData(1, 1, 10.00, "   ", false)] // PaymentMethod required
        public void RecordPaymentRequest_ValidationRules_ShouldBeAppliedCorrectly(
            int memberId, int lendingId, decimal amount, string paymentMethod, bool shouldBeValid)
        {
            // Arrange
            var request = new RecordPaymentRequest
            {
                MemberId = memberId,
                LendingId = lendingId,
                Amount = amount,
                PaymentMethod = paymentMethod ?? string.Empty
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.Equal(shouldBeValid, isValid);
            if (!shouldBeValid)
            {
                Assert.NotEmpty(validationResults);
            }
        }

        [Theory]
        [InlineData("Cash", true)]
        [InlineData("Credit Card", true)]
        [InlineData("Debit Card", true)]
        [InlineData("Bank Transfer", true)]
        [InlineData("Check", true)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("c", true)] // Single character
        [InlineData("Very Long Payment Method Name", true)] // Long string
        public void RecordPaymentRequest_PaymentMethodValidation_ShouldBeAppliedCorrectly(
            string paymentMethod, bool shouldBeValid)
        {
            // Arrange
            var request = new RecordPaymentRequest
            {
                MemberId = 1,
                LendingId = 1,
                Amount = 10.00m,
                PaymentMethod = paymentMethod
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.Equal(shouldBeValid, isValid);
            if (!shouldBeValid)
            {
                Assert.NotEmpty(validationResults);
                Assert.Contains(validationResults, vr => vr.MemberNames.Contains("PaymentMethod"));
            }
        }

        #endregion

        #region PaymentResponse Tests

        [Fact]
        public void PaymentResponse_DefaultValues_ShouldBeSetCorrectly()
        {
            // Act
            var response = new PaymentResponse();

            // Assert
            Assert.False(response.Success);
            Assert.Equal(string.Empty, response.Message);
            Assert.Null(response.Payment);
            Assert.Equal(0, response.RemainingBalance);
        }

        [Fact]
        public void PaymentResponse_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var response = new PaymentResponse();
            var payment = new PaymentDto
            {
                Id = 1,
                LendingId = 2,
                MemberId = 3,
                Amount = 25.50m,
                PaymentMethod = "Cash",
                PaymentDate = DateTime.UtcNow,
                RecordedBy = 4,
                RecordedByName = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            response.Success = true;
            response.Message = "Payment successful";
            response.Payment = payment;
            response.RemainingBalance = 5.00m;

            // Assert
            Assert.True(response.Success);
            Assert.Equal("Payment successful", response.Message);
            Assert.Equal(payment, response.Payment);
            Assert.Equal(5.00m, response.RemainingBalance);
        }

        [Theory]
        [InlineData(true, "Success", true)]
        [InlineData(false, "Error", false)]
        [InlineData(true, "", true)]
        [InlineData(false, "", false)]
        public void PaymentResponse_StateValidation_ShouldBeConsistent(
            bool success, string message, bool shouldHavePayment)
        {
            // Arrange
            var response = new PaymentResponse
            {
                Success = success,
                Message = message,
                Payment = shouldHavePayment ? new PaymentDto() : null,
                RemainingBalance = 0
            };

            // Act & Assert
            Assert.Equal(success, response.Success);
            Assert.Equal(message, response.Message);
            if (shouldHavePayment)
            {
                Assert.NotNull(response.Payment);
            }
            else
            {
                Assert.Null(response.Payment);
            }
        }

        #endregion

        #region PaymentDto Tests

        [Fact]
        public void PaymentDto_DefaultValues_ShouldBeSetCorrectly()
        {
            // Act
            var dto = new PaymentDto();

            // Assert
            Assert.Equal(0, dto.Id);
            Assert.Equal(0, dto.LendingId);
            Assert.Equal(0, dto.MemberId);
            Assert.Equal(0, dto.Amount);
            Assert.Equal(string.Empty, dto.PaymentMethod);
            Assert.Equal(DateTime.MinValue, dto.PaymentDate);
            Assert.Equal(0, dto.RecordedBy);
            Assert.Equal(string.Empty, dto.RecordedByName);
            Assert.Equal(DateTime.MinValue, dto.CreatedAt);
        }

        [Fact]
        public void PaymentDto_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var dto = new PaymentDto();
            var now = DateTime.UtcNow;

            // Act
            dto.Id = 1;
            dto.LendingId = 2;
            dto.MemberId = 3;
            dto.Amount = 25.50m;
            dto.PaymentMethod = "Credit Card";
            dto.PaymentDate = now;
            dto.RecordedBy = 4;
            dto.RecordedByName = "Admin User";
            dto.CreatedAt = now.AddMinutes(-10);

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal(2, dto.LendingId);
            Assert.Equal(3, dto.MemberId);
            Assert.Equal(25.50m, dto.Amount);
            Assert.Equal("Credit Card", dto.PaymentMethod);
            Assert.Equal(now, dto.PaymentDate);
            Assert.Equal(4, dto.RecordedBy);
            Assert.Equal("Admin User", dto.RecordedByName);
            Assert.Equal(now.AddMinutes(-10), dto.CreatedAt);
        }

        #endregion

        #region GetPaymentHistoryRequest Tests

        [Fact]
        public void GetPaymentHistoryRequest_DefaultValues_ShouldBeSetCorrectly()
        {
            // Act
            var request = new GetPaymentHistoryRequest();

            // Assert
            Assert.Null(request.MemberId);
            Assert.Null(request.LendingId);
            Assert.Null(request.FromDate);
            Assert.Null(request.ToDate);
            Assert.Equal(1, request.Page);
            Assert.Equal(10, request.PageSize);
        }

        [Fact]
        public void GetPaymentHistoryRequest_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var request = new GetPaymentHistoryRequest();
            var fromDate = DateTime.UtcNow.AddDays(-30);
            var toDate = DateTime.UtcNow;

            // Act
            request.MemberId = 1;
            request.LendingId = 2;
            request.FromDate = fromDate;
            request.ToDate = toDate;
            request.Page = 2;
            request.PageSize = 25;

            // Assert
            Assert.Equal(1, request.MemberId);
            Assert.Equal(2, request.LendingId);
            Assert.Equal(fromDate, request.FromDate);
            Assert.Equal(toDate, request.ToDate);
            Assert.Equal(2, request.Page);
            Assert.Equal(25, request.PageSize);
        }

        [Theory]
        [InlineData(1, null, null, null, 1, 10)]
        [InlineData(null, 1, null, null, 1, 10)]
        [InlineData(1, 1, "2023-01-01", "2023-12-31", 2, 25)]
        [InlineData(null, null, null, null, 1, 10)]
        public void GetPaymentHistoryRequest_ValidCombinations_ShouldBeValid(
            int? memberId, int? lendingId, string? fromDateStr, string? toDateStr, int page, int pageSize)
        {
            // Arrange
            var request = new GetPaymentHistoryRequest
            {
                MemberId = memberId,
                LendingId = lendingId,
                FromDate = fromDateStr != null ? DateTime.Parse(fromDateStr) : null,
                ToDate = toDateStr != null ? DateTime.Parse(toDateStr) : null,
                Page = page,
                PageSize = pageSize
            };

            // Act & Assert
            Assert.Equal(memberId, request.MemberId);
            Assert.Equal(lendingId, request.LendingId);
            Assert.Equal(page, request.Page);
            Assert.Equal(pageSize, request.PageSize);
        }

        #endregion

        #region PaymentHistoryResponse Tests

        [Fact]
        public void PaymentHistoryResponse_DefaultValues_ShouldBeSetCorrectly()
        {
            // Act
            var response = new PaymentHistoryResponse();

            // Assert
            Assert.NotNull(response.Payments);
            Assert.Empty(response.Payments);
            Assert.Equal(0, response.TotalCount);
            Assert.Equal(0, response.Page);
            Assert.Equal(0, response.PageSize);
            Assert.False(response.HasNextPage);
        }

        [Fact]
        public void PaymentHistoryResponse_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var response = new PaymentHistoryResponse();
            var payments = new List<PaymentDto>
            {
                new PaymentDto { Id = 1, Amount = 10.00m },
                new PaymentDto { Id = 2, Amount = 15.50m }
            };

            // Act
            response.Payments = payments;
            response.TotalCount = 2;
            response.Page = 1;
            response.PageSize = 10;
            response.HasNextPage = false;

            // Assert
            Assert.Equal(payments, response.Payments);
            Assert.Equal(2, response.TotalCount);
            Assert.Equal(1, response.Page);
            Assert.Equal(10, response.PageSize);
            Assert.False(response.HasNextPage);
        }

        [Theory]
        [InlineData(10, 1, 10, false)] // 1 * 10 = 10, not < 10, so no next page
        [InlineData(5, 1, 10, false)] // 1 * 10 = 10, not < 5, so no next page
        [InlineData(0, 1, 10, false)] // 1 * 10 = 10, not < 0, so no next page
        [InlineData(15, 1, 10, true)] // 1 * 10 = 10, < 15, so has next page
        public void PaymentHistoryResponse_HasNextPage_ShouldBeCalculatedCorrectly(
            int totalCount, int page, int pageSize, bool expectedHasNextPage)
        {
            // Arrange
            var response = new PaymentHistoryResponse
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            // Act
            response.HasNextPage = (page * pageSize) < totalCount;

            // Assert
            Assert.Equal(expectedHasNextPage, response.HasNextPage);
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void RecordPaymentRequest_MinimumValidValues_ShouldPassValidation()
        {
            // Arrange
            var request = new RecordPaymentRequest
            {
                MemberId = 1,
                LendingId = 1,
                Amount = 0.01m,
                PaymentMethod = "C" // Minimum length
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void RecordPaymentRequest_MaximumValidValues_ShouldPassValidation()
        {
            // Arrange
            var request = new RecordPaymentRequest
            {
                MemberId = int.MaxValue,
                LendingId = int.MaxValue,
                Amount = 999999.99m,
                PaymentMethod = new string('A', 50) // Maximum length
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void RecordPaymentRequest_ExceedsMaximumValues_ShouldFailValidation()
        {
            // Arrange
            var request = new RecordPaymentRequest
            {
                MemberId = 1,
                LendingId = 1,
                Amount = 1000000.00m, // Exceeds maximum
                PaymentMethod = "Cash"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Amount"));
        }

        [Fact]
        public void RecordPaymentRequest_PaymentMethodExceedsMaxLength_ShouldFailValidation()
        {
            // Arrange
            var request = new RecordPaymentRequest
            {
                MemberId = 1,
                LendingId = 1,
                Amount = 10.00m,
                PaymentMethod = new string('A', 51) // Exceeds maximum length
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("PaymentMethod"));
        }

        #endregion

        #region DateTime Handling Tests

        [Fact]
        public void RecordPaymentRequest_PaymentDate_CanBeNull()
        {
            // Arrange
            var request = new RecordPaymentRequest
            {
                MemberId = 1,
                LendingId = 1,
                Amount = 10.00m,
                PaymentMethod = "Cash",
                PaymentDate = null
            };

            // Act & Assert
            Assert.Null(request.PaymentDate);
        }

        [Fact]
        public void RecordPaymentRequest_PaymentDate_CanBeSet()
        {
            // Arrange
            var request = new RecordPaymentRequest
            {
                MemberId = 1,
                LendingId = 1,
                Amount = 10.00m,
                PaymentMethod = "Cash",
                PaymentDate = DateTime.UtcNow
            };

            // Act & Assert
            Assert.NotNull(request.PaymentDate);
            Assert.True(request.PaymentDate <= DateTime.UtcNow);
        }

        [Fact]
        public void PaymentDto_DateTimeProperties_CanHandleDifferentValues()
        {
            // Arrange
            var dto = new PaymentDto();
            var now = DateTime.UtcNow;
            var pastDate = now.AddDays(-1);
            var futureDate = now.AddDays(1);

            // Act
            dto.PaymentDate = pastDate;
            dto.CreatedAt = futureDate;

            // Assert
            Assert.Equal(pastDate, dto.PaymentDate);
            Assert.Equal(futureDate, dto.CreatedAt);
        }

        #endregion

        #region Collection Tests

        [Fact]
        public void PaymentHistoryResponse_Payments_CanBeEmpty()
        {
            // Arrange
            var response = new PaymentHistoryResponse();

            // Act & Assert
            Assert.NotNull(response.Payments);
            Assert.Empty(response.Payments);
        }

        [Fact]
        public void PaymentHistoryResponse_Payments_CanContainMultipleItems()
        {
            // Arrange
            var response = new PaymentHistoryResponse();
            var payments = new List<PaymentDto>
            {
                new PaymentDto { Id = 1, Amount = 10.00m },
                new PaymentDto { Id = 2, Amount = 20.00m },
                new PaymentDto { Id = 3, Amount = 30.00m }
            };

            // Act
            response.Payments = payments;

            // Assert
            Assert.Equal(3, response.Payments.Count);
            Assert.Equal(1, response.Payments[0].Id);
            Assert.Equal(2, response.Payments[1].Id);
            Assert.Equal(3, response.Payments[2].Id);
        }

        #endregion
    }
}
