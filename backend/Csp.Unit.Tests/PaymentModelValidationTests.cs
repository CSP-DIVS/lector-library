using System;
using System.ComponentModel.DataAnnotations;
using Csp.Api.Models;
using Xunit;

namespace Csp.Unit.Tests
{
    public class PaymentModelValidationTests
    {
        #region Payment Model Tests

        [Fact]
        public void Payment_DefaultValues_ShouldBeSetCorrectly()
        {
            // Act
            var payment = new Payment();

            // Assert
            Assert.Equal(0, payment.Id);
            Assert.Equal(0, payment.LendingId);
            Assert.Equal(0, payment.MemberId);
            Assert.Equal(0, payment.Amount);
            Assert.Equal("Cash", payment.PaymentMethod);
            Assert.True(payment.PaymentDate <= DateTime.UtcNow);
            Assert.Equal(0, payment.RecordedBy);
            Assert.True(payment.CreatedAt <= DateTime.UtcNow);
            Assert.True(payment.UpdatedAt <= DateTime.UtcNow);
            Assert.Null(payment.Member);
            Assert.Null(payment.RecordedByUser);
            Assert.Null(payment.Lending);
        }

        [Fact]
        public void Payment_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var payment = new Payment();
            var now = DateTime.UtcNow;

            // Act
            payment.Id = 1;
            payment.LendingId = 2;
            payment.MemberId = 3;
            payment.Amount = 25.50m;
            payment.PaymentMethod = "Credit Card";
            payment.PaymentDate = now;
            payment.RecordedBy = 4;
            payment.CreatedAt = now.AddMinutes(-10);
            payment.UpdatedAt = now;

            // Assert
            Assert.Equal(1, payment.Id);
            Assert.Equal(2, payment.LendingId);
            Assert.Equal(3, payment.MemberId);
            Assert.Equal(25.50m, payment.Amount);
            Assert.Equal("Credit Card", payment.PaymentMethod);
            Assert.Equal(now, payment.PaymentDate);
            Assert.Equal(4, payment.RecordedBy);
            Assert.Equal(now.AddMinutes(-10), payment.CreatedAt);
            Assert.Equal(now, payment.UpdatedAt);
        }

        #endregion

        #region Data Annotations Validation Tests

        [Theory]
        [InlineData(1, 1, 0.01, "Cash", true)]
        [InlineData(1, 1, 999999.99, "Credit Card", true)]
        [InlineData(1, 1, 0.00, "Cash", false)] // Amount too low
        [InlineData(1, 1, -1.00, "Cash", false)] // Negative amount
        [InlineData(1, 1, 1000000.00, "Cash", false)] // Amount too high
        [InlineData(0, 1, 10.00, "Cash", true)] // LendingId = 0 is valid (no Range validation)
        [InlineData(1, 0, 10.00, "Cash", true)] // MemberId = 0 is valid (no Range validation)
        [InlineData(1, 1, 10.00, "", false)] // PaymentMethod required
        [InlineData(1, 1, 10.00, null, false)] // PaymentMethod required
        public void Payment_ValidationRules_ShouldBeAppliedCorrectly(
            int lendingId, int memberId, decimal amount, string paymentMethod, bool shouldBeValid)
        {
            // Arrange
            var payment = new Payment
            {
                LendingId = lendingId,
                MemberId = memberId,
                Amount = amount,
                PaymentMethod = paymentMethod ?? string.Empty,
                PaymentDate = DateTime.UtcNow,
                RecordedBy = 1
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(payment);
            var isValid = Validator.TryValidateObject(payment, validationContext, validationResults, true);

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
        [InlineData("Invalid Method", true)] // Model allows any string, just validates length
        [InlineData("c", true)] // Single character
        [InlineData("Very Long Payment Method Name That Exceeds Normal Limits", false)] // Long string exceeds StringLength(50)
        public void Payment_PaymentMethodValidation_ShouldBeAppliedCorrectly(
            string paymentMethod, bool shouldBeValid)
        {
            // Arrange
            var payment = new Payment
            {
                LendingId = 1,
                MemberId = 1,
                Amount = 10.00m,
                PaymentMethod = paymentMethod,
                PaymentDate = DateTime.UtcNow,
                RecordedBy = 1
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(payment);
            var isValid = Validator.TryValidateObject(payment, validationContext, validationResults, true);

            // Assert
            Assert.Equal(shouldBeValid, isValid);
            if (!shouldBeValid)
            {
                Assert.NotEmpty(validationResults);
                Assert.Contains(validationResults, vr => vr.MemberNames.Contains("PaymentMethod"));
            }
        }

        [Theory]
        [InlineData(0.01, true)]
        [InlineData(1.00, true)]
        [InlineData(999999.99, true)]
        [InlineData(0.00, false)]
        [InlineData(-0.01, false)]
        [InlineData(-100.00, false)]
        [InlineData(1000000.00, false)]
        public void Payment_AmountRangeValidation_ShouldBeAppliedCorrectly(
            decimal amount, bool shouldBeValid)
        {
            // Arrange
            var payment = new Payment
            {
                LendingId = 1,
                MemberId = 1,
                Amount = amount,
                PaymentMethod = "Cash",
                PaymentDate = DateTime.UtcNow,
                RecordedBy = 1
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(payment);
            var isValid = Validator.TryValidateObject(payment, validationContext, validationResults, true);

            // Assert
            Assert.Equal(shouldBeValid, isValid);
            if (!shouldBeValid)
            {
                Assert.NotEmpty(validationResults);
                Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Amount"));
            }
        }

        #endregion

        #region Business Logic Validation Tests

        [Theory]
        [InlineData(1, 1, 10.00, "Cash", true)]
        [InlineData(1, 1, 0.01, "Credit Card", true)]
        [InlineData(1, 1, 999999.99, "Bank Transfer", true)]
        [InlineData(0, 1, 10.00, "Cash", true)] // LendingId = 0 is valid (no Range validation)
        [InlineData(1, 0, 10.00, "Cash", true)] // MemberId = 0 is valid (no Range validation)
        [InlineData(1, 1, 0.00, "Cash", false)] // Invalid Amount
        [InlineData(1, 1, 10.00, "", false)] // Invalid PaymentMethod
        public void Payment_CompleteValidation_ShouldBeAppliedCorrectly(
            int lendingId, int memberId, decimal amount, string paymentMethod, bool shouldBeValid)
        {
            // Arrange
            var payment = new Payment
            {
                LendingId = lendingId,
                MemberId = memberId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                PaymentDate = DateTime.UtcNow,
                RecordedBy = 1
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(payment);
            var isValid = Validator.TryValidateObject(payment, validationContext, validationResults, true);

            // Assert
            Assert.Equal(shouldBeValid, isValid);
            if (!shouldBeValid)
            {
                Assert.NotEmpty(validationResults);
            }
        }

        [Fact]
        public void Payment_RequiredFields_ShouldBeValidated()
        {
            // Arrange
            var payment = new Payment(); // All required fields are default values

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(payment);
            var isValid = Validator.TryValidateObject(payment, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            // Check that we have validation errors for the required fields
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Amount"));
            // Note: LendingId and MemberId have default values of 0, which pass Required validation
            // Amount = 0 fails Range validation, and PaymentMethod = "Cash" passes Required validation
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void Payment_MinimumValidValues_ShouldPassValidation()
        {
            // Arrange
            var payment = new Payment
            {
                LendingId = 1,
                MemberId = 1,
                Amount = 0.01m,
                PaymentMethod = "C", // Minimum length
                PaymentDate = DateTime.UtcNow,
                RecordedBy = 1
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(payment);
            var isValid = Validator.TryValidateObject(payment, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Payment_MaximumValidValues_ShouldPassValidation()
        {
            // Arrange
            var payment = new Payment
            {
                LendingId = int.MaxValue,
                MemberId = int.MaxValue,
                Amount = 999999.99m,
                PaymentMethod = new string('A', 50), // Maximum length
                PaymentDate = DateTime.UtcNow,
                RecordedBy = int.MaxValue
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(payment);
            var isValid = Validator.TryValidateObject(payment, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Payment_ExceedsMaximumValues_ShouldFailValidation()
        {
            // Arrange
            var payment = new Payment
            {
                LendingId = 1,
                MemberId = 1,
                Amount = 1000000.00m, // Exceeds maximum
                PaymentMethod = "Cash",
                PaymentDate = DateTime.UtcNow,
                RecordedBy = 1
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(payment);
            var isValid = Validator.TryValidateObject(payment, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Amount"));
        }

        [Fact]
        public void Payment_PaymentMethodExceedsMaxLength_ShouldFailValidation()
        {
            // Arrange
            var payment = new Payment
            {
                LendingId = 1,
                MemberId = 1,
                Amount = 10.00m,
                PaymentMethod = new string('A', 51), // Exceeds maximum length
                PaymentDate = DateTime.UtcNow,
                RecordedBy = 1
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(payment);
            var isValid = Validator.TryValidateObject(payment, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("PaymentMethod"));
        }

        #endregion

        #region Navigation Properties Tests

        [Fact]
        public void Payment_NavigationProperties_CanBeSet()
        {
            // Arrange
            var payment = new Payment
            {
                LendingId = 1,
                MemberId = 1,
                Amount = 10.00m,
                PaymentMethod = "Cash",
                PaymentDate = DateTime.UtcNow,
                RecordedBy = 1
            };

            var member = new User { Id = 1, Username = "testuser" };
            var recordedByUser = new User { Id = 1, Username = "admin" };
            var lending = new Lending { Id = 1, UserId = 1, BookId = 1 };

            // Act
            payment.Member = member;
            payment.RecordedByUser = recordedByUser;
            payment.Lending = lending;

            // Assert
            Assert.Equal(member, payment.Member);
            Assert.Equal(recordedByUser, payment.RecordedByUser);
            Assert.Equal(lending, payment.Lending);
        }

        [Fact]
        public void Payment_NavigationProperties_CanBeNull()
        {
            // Arrange
            var payment = new Payment
            {
                LendingId = 1,
                MemberId = 1,
                Amount = 10.00m,
                PaymentMethod = "Cash",
                PaymentDate = DateTime.UtcNow,
                RecordedBy = 1
            };

            // Act & Assert
            Assert.Null(payment.Member);
            Assert.Null(payment.RecordedByUser);
            Assert.Null(payment.Lending);
        }

        #endregion

        #region DateTime Properties Tests

        [Fact]
        public void Payment_DateTimeProperties_ShouldBeSetCorrectly()
        {
            // Arrange
            var payment = new Payment();
            var now = DateTime.UtcNow;
            var pastDate = now.AddDays(-1);
            var futureDate = now.AddDays(1);

            // Act
            payment.PaymentDate = pastDate;
            payment.CreatedAt = pastDate;
            payment.UpdatedAt = futureDate;

            // Assert
            Assert.Equal(pastDate, payment.PaymentDate);
            Assert.Equal(pastDate, payment.CreatedAt);
            Assert.Equal(futureDate, payment.UpdatedAt);
        }

        [Fact]
        public void Payment_DateTimeProperties_CanHandleDifferentTimeZones()
        {
            // Arrange
            var payment = new Payment();
            var utcNow = DateTime.UtcNow;
            var localNow = DateTime.Now;

            // Act
            payment.PaymentDate = utcNow;
            payment.CreatedAt = localNow;

            // Assert
            Assert.Equal(utcNow, payment.PaymentDate);
            Assert.Equal(localNow, payment.CreatedAt);
        }

        #endregion
    }
}
