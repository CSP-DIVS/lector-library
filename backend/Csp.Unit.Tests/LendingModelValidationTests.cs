//Csp.Unit.Tests/LendingModelValidationTests

using Xunit;
using Csp.Api.Models;

namespace Csp.Unit.Tests
{
    public class LendingModelValidationTests
    {
        #region Lending Model Tests

        [Fact]
        public void Lending_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var lending = new Lending();

            // Assert
            Assert.Equal(0, lending.Id);
            Assert.Equal(0, lending.BookId);
            Assert.Equal(0, lending.UserId);
            Assert.Equal("Active", lending.Status);
            Assert.Null(lending.ReturnDate);
            Assert.Null(lending.FineAmount);
            Assert.False(lending.FinePaid);
            Assert.Equal(0, lending.RenewalCount);
            Assert.Equal(2, lending.MaxRenewals);
        }

        [Fact]
        public void Lending_BorrowDate_ShouldBeSetOnCreation()
        {
            // Arrange & Act
            var lending = new Lending();
            var now = DateTime.UtcNow;

            // Assert
            Assert.True(lending.BorrowDate <= now);
            Assert.True(lending.BorrowDate >= now.AddSeconds(-5)); // Within last 5 seconds
        }

        [Fact]
        public void Lending_DueDate_CanBeSet()
        {
            // Arrange
            var lending = new Lending();
            var dueDate = DateTime.UtcNow.AddDays(14);

            // Act
            lending.DueDate = dueDate;

            // Assert
            Assert.Equal(dueDate, lending.DueDate);
        }

        [Fact]
        public void Lending_Status_CanBeUpdated()
        {
            // Arrange
            var lending = new Lending { Status = "Active" };

            // Act
            lending.Status = "Returned";

            // Assert
            Assert.Equal("Returned", lending.Status);
        }

        [Fact]
        public void Lending_ReturnDate_CanBeSet()
        {
            // Arrange
            var lending = new Lending();
            var returnDate = DateTime.UtcNow;

            // Act
            lending.ReturnDate = returnDate;

            // Assert
            Assert.NotNull(lending.ReturnDate);
            Assert.Equal(returnDate, lending.ReturnDate.Value);
        }

        [Fact]
        public void Lending_FineAmount_CanBeCalculated()
        {
            // Arrange
            var lending = new Lending
            {
                DueDate = DateTime.UtcNow.AddDays(-5),
                ReturnDate = DateTime.UtcNow
            };

            // Act
            var overdueDays = (lending.ReturnDate.Value - lending.DueDate).Days;
            var fineAmount = overdueDays * 1.0m; // $1 per day
            lending.FineAmount = fineAmount;

            // Assert
            Assert.Equal(5.0m, lending.FineAmount.Value);
        }

        [Fact]
        public void Lending_FinePaid_CanBeUpdated()
        {
            // Arrange
            var lending = new Lending
            {
                FineAmount = 10.0m,
                FinePaid = false
            };

            // Act
            lending.FinePaid = true;

            // Assert
            Assert.True(lending.FinePaid);
        }

        [Fact]
        public void Lending_RenewalCount_CanBeIncremented()
        {
            // Arrange
            var lending = new Lending
            {
                RenewalCount = 0,
                MaxRenewals = 2
            };

            // Act
            lending.RenewalCount++;

            // Assert
            Assert.Equal(1, lending.RenewalCount);
            Assert.True(lending.RenewalCount < lending.MaxRenewals);
        }

        [Fact]
        public void Lending_MaxRenewals_IsLimitedToTwo()
        {
            // Arrange
            var lending = new Lending
            {
                RenewalCount = 2,
                MaxRenewals = 2
            };

            // Act
            var canRenew = lending.RenewalCount < lending.MaxRenewals;

            // Assert
            Assert.False(canRenew);
        }

        [Fact]
        public void Lending_CreatedAt_ShouldBeSetOnCreation()
        {
            // Arrange & Act
            var lending = new Lending();
            var now = DateTime.UtcNow;

            // Assert
            Assert.True(lending.CreatedAt <= now);
            Assert.True(lending.CreatedAt >= now.AddSeconds(-5));
        }

        [Fact]
        public void Lending_UpdatedAt_ShouldBeSetOnCreation()
        {
            // Arrange & Act
            var lending = new Lending();
            var now = DateTime.UtcNow;

            // Assert
            Assert.True(lending.UpdatedAt <= now);
            Assert.True(lending.UpdatedAt >= now.AddSeconds(-5));
        }

        [Fact]
        public void Lending_AllProperties_CanBeSet()
        {
            // Arrange & Act
            var lending = new Lending
            {
                Id = 1,
                BookId = 5,
                UserId = 10,
                BorrowDate = DateTime.UtcNow.AddDays(-7),
                DueDate = DateTime.UtcNow.AddDays(7),
                ReturnDate = DateTime.UtcNow,
                Status = "Returned",
                FineAmount = 5.0m,
                FinePaid = true,
                RenewalCount = 1,
                MaxRenewals = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(1, lending.Id);
            Assert.Equal(5, lending.BookId);
            Assert.Equal(10, lending.UserId);
            Assert.Equal("Returned", lending.Status);
            Assert.NotNull(lending.ReturnDate);
            Assert.Equal(5.0m, lending.FineAmount);
            Assert.True(lending.FinePaid);
            Assert.Equal(1, lending.RenewalCount);
            Assert.Equal(2, lending.MaxRenewals);
        }

        [Fact]
        public void Lending_ValidStatuses_ShouldBeAccepted()
        {
            // Arrange
            var validStatuses = new[] { "Active", "Returned", "Overdue" };

            // Act & Assert
            foreach (var status in validStatuses)
            {
                var lending = new Lending { Status = status };
                Assert.Equal(status, lending.Status);
            }
        }

        [Fact]
        public void Lending_IsOverdue_CanBeDetermined()
        {
            // Arrange
            var overdueLending = new Lending
            {
                DueDate = DateTime.UtcNow.AddDays(-1),
                Status = "Active"
            };

            var activeLending = new Lending
            {
                DueDate = DateTime.UtcNow.AddDays(5),
                Status = "Active"
            };

            // Act
            var isOverdue1 = overdueLending.DueDate < DateTime.UtcNow && overdueLending.Status == "Active";
            var isOverdue2 = activeLending.DueDate < DateTime.UtcNow && activeLending.Status == "Active";

            // Assert
            Assert.True(isOverdue1);
            Assert.False(isOverdue2);
        }

        #endregion

        #region Reservation Model Tests

        [Fact]
        public void Reservation_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var reservation = new Reservation();

            // Assert
            Assert.Equal(0, reservation.Id);
            Assert.Equal(0, reservation.BookId);
            Assert.Equal(0, reservation.UserId);
            Assert.Equal("Pending", reservation.Status);
            Assert.Equal(0, reservation.QueuePosition);
            Assert.Null(reservation.AvailableDate);
            Assert.Null(reservation.ExpiryDate);
            Assert.Null(reservation.FulfilledDate);
        }

        [Fact]
        public void Reservation_ReservedDate_ShouldBeSetOnCreation()
        {
            // Arrange & Act
            var reservation = new Reservation();
            var now = DateTime.UtcNow;

            // Assert
            Assert.True(reservation.ReservedDate <= now);
            Assert.True(reservation.ReservedDate >= now.AddSeconds(-5));
        }

        [Fact]
        public void Reservation_QueuePosition_CanBeSet()
        {
            // Arrange
            var reservation = new Reservation();

            // Act
            reservation.QueuePosition = 3;

            // Assert
            Assert.Equal(3, reservation.QueuePosition);
        }

        [Fact]
        public void Reservation_Status_CanBeUpdated()
        {
            // Arrange
            var reservation = new Reservation { Status = "Pending" };

            // Act
            reservation.Status = "Available";

            // Assert
            Assert.Equal("Available", reservation.Status);
        }

        [Fact]
        public void Reservation_AvailableDate_CanBeSet()
        {
            // Arrange
            var reservation = new Reservation();
            var availableDate = DateTime.UtcNow;

            // Act
            reservation.AvailableDate = availableDate;

            // Assert
            Assert.NotNull(reservation.AvailableDate);
            Assert.Equal(availableDate, reservation.AvailableDate.Value);
        }

        [Fact]
        public void Reservation_ExpiryDate_CanBeCalculated()
        {
            // Arrange
            var reservation = new Reservation();
            var availableDate = DateTime.UtcNow;

            // Act
            reservation.AvailableDate = availableDate;
            reservation.ExpiryDate = availableDate.AddDays(3);

            // Assert
            Assert.NotNull(reservation.ExpiryDate);
            Assert.True(reservation.ExpiryDate > reservation.AvailableDate);
            Assert.Equal(3, (reservation.ExpiryDate.Value - reservation.AvailableDate.Value).Days);
        }

        [Fact]
        public void Reservation_FulfilledDate_CanBeSet()
        {
            // Arrange
            var reservation = new Reservation { Status = "Available" };
            var fulfilledDate = DateTime.UtcNow;

            // Act
            reservation.Status = "Fulfilled";
            reservation.FulfilledDate = fulfilledDate;

            // Assert
            Assert.Equal("Fulfilled", reservation.Status);
            Assert.NotNull(reservation.FulfilledDate);
            Assert.Equal(fulfilledDate, reservation.FulfilledDate.Value);
        }

        [Fact]
        public void Reservation_CreatedAt_ShouldBeSetOnCreation()
        {
            // Arrange & Act
            var reservation = new Reservation();
            var now = DateTime.UtcNow;

            // Assert
            Assert.True(reservation.CreatedAt <= now);
            Assert.True(reservation.CreatedAt >= now.AddSeconds(-5));
        }

        [Fact]
        public void Reservation_UpdatedAt_ShouldBeSetOnCreation()
        {
            // Arrange & Act
            var reservation = new Reservation();
            var now = DateTime.UtcNow;

            // Assert
            Assert.True(reservation.UpdatedAt <= now);
            Assert.True(reservation.UpdatedAt >= now.AddSeconds(-5));
        }

        [Fact]
        public void Reservation_AllProperties_CanBeSet()
        {
            // Arrange & Act
            var now = DateTime.UtcNow;
            var reservation = new Reservation
            {
                Id = 1,
                BookId = 5,
                UserId = 10,
                ReservedDate = now.AddDays(-3),
                QueuePosition = 2,
                Status = "Available",
                AvailableDate = now,
                ExpiryDate = now.AddDays(3),
                FulfilledDate = null,
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now
            };

            // Assert
            Assert.Equal(1, reservation.Id);
            Assert.Equal(5, reservation.BookId);
            Assert.Equal(10, reservation.UserId);
            Assert.Equal(2, reservation.QueuePosition);
            Assert.Equal("Available", reservation.Status);
            Assert.NotNull(reservation.AvailableDate);
            Assert.NotNull(reservation.ExpiryDate);
            Assert.Null(reservation.FulfilledDate);
        }

        [Fact]
        public void Reservation_ValidStatuses_ShouldBeAccepted()
        {
            // Arrange
            var validStatuses = new[] { "Pending", "Available", "Fulfilled", "Cancelled", "Expired" };

            // Act & Assert
            foreach (var status in validStatuses)
            {
                var reservation = new Reservation { Status = status };
                Assert.Equal(status, reservation.Status);
            }
        }

        [Fact]
        public void Reservation_IsExpired_CanBeDetermined()
        {
            // Arrange
            var expiredReservation = new Reservation
            {
                Status = "Available",
                AvailableDate = DateTime.UtcNow.AddDays(-5),
                ExpiryDate = DateTime.UtcNow.AddDays(-1)
            };

            var activeReservation = new Reservation
            {
                Status = "Available",
                AvailableDate = DateTime.UtcNow.AddDays(-1),
                ExpiryDate = DateTime.UtcNow.AddDays(2)
            };

            // Act
            var isExpired1 = expiredReservation.ExpiryDate.HasValue && 
                           expiredReservation.ExpiryDate.Value < DateTime.UtcNow;
            var isExpired2 = activeReservation.ExpiryDate.HasValue && 
                           activeReservation.ExpiryDate.Value < DateTime.UtcNow;

            // Assert
            Assert.True(isExpired1);
            Assert.False(isExpired2);
        }

        [Fact]
        public void Reservation_StatusTransition_PendingToAvailable_IsValid()
        {
            // Arrange
            var reservation = new Reservation
            {
                Status = "Pending",
                QueuePosition = 1
            };

            // Act
            reservation.Status = "Available";
            reservation.AvailableDate = DateTime.UtcNow;
            reservation.ExpiryDate = DateTime.UtcNow.AddDays(3);

            // Assert
            Assert.Equal("Available", reservation.Status);
            Assert.NotNull(reservation.AvailableDate);
            Assert.NotNull(reservation.ExpiryDate);
        }

        [Fact]
        public void Reservation_StatusTransition_AvailableToFulfilled_IsValid()
        {
            // Arrange
            var reservation = new Reservation
            {
                Status = "Available",
                AvailableDate = DateTime.UtcNow.AddDays(-1)
            };

            // Act
            reservation.Status = "Fulfilled";
            reservation.FulfilledDate = DateTime.UtcNow;

            // Assert
            Assert.Equal("Fulfilled", reservation.Status);
            Assert.NotNull(reservation.FulfilledDate);
        }

        [Fact]
        public void Reservation_StatusTransition_PendingToCancelled_IsValid()
        {
            // Arrange
            var reservation = new Reservation
            {
                Status = "Pending",
                QueuePosition = 3
            };

            // Act
            reservation.Status = "Cancelled";

            // Assert
            Assert.Equal("Cancelled", reservation.Status);
        }

        [Fact]
        public void Reservation_StatusTransition_AvailableToExpired_IsValid()
        {
            // Arrange
            var reservation = new Reservation
            {
                Status = "Available",
                AvailableDate = DateTime.UtcNow.AddDays(-4),
                ExpiryDate = DateTime.UtcNow.AddDays(-1)
            };

            // Act
            reservation.Status = "Expired";

            // Assert
            Assert.Equal("Expired", reservation.Status);
        }

        #endregion

        #region Business Rule Validation Tests

        [Fact]
        public void BusinessRule_LendingDuration_ShouldBe14Days()
        {
            // Arrange
            var borrowDate = DateTime.UtcNow;
            var dueDate = borrowDate.AddDays(14);
            var lending = new Lending
            {
                BorrowDate = borrowDate,
                DueDate = dueDate
            };

            // Act
            var duration = (lending.DueDate - lending.BorrowDate).Days;

            // Assert
            Assert.Equal(14, duration);
        }

        [Fact]
        public void BusinessRule_ReservationExpiry_ShouldBe3Days()
        {
            // Arrange
            var availableDate = DateTime.UtcNow;
            var expiryDate = availableDate.AddDays(3);
            var reservation = new Reservation
            {
                AvailableDate = availableDate,
                ExpiryDate = expiryDate
            };

            // Act
            var expiryPeriod = (reservation.ExpiryDate.Value - reservation.AvailableDate.Value).Days;

            // Assert
            Assert.Equal(3, expiryPeriod);
        }

        [Fact]
        public void BusinessRule_FinePerDay_ShouldBe1Dollar()
        {
            // Arrange
            const decimal finePerDay = 1.0m;
            var overdueDays = 5;

            // Act
            var totalFine = overdueDays * finePerDay;

            // Assert
            Assert.Equal(5.0m, totalFine);
        }

        [Fact]
        public void BusinessRule_MaxRenewals_ShouldBe2()
        {
            // Arrange
            var lending = new Lending();

            // Assert
            Assert.Equal(2, lending.MaxRenewals);
        }

        [Fact]
        public void BusinessRule_InitialRenewalCount_ShouldBeZero()
        {
            // Arrange
            var lending = new Lending();

            // Assert
            Assert.Equal(0, lending.RenewalCount);
        }

        [Fact]
        public void BusinessRule_InitialQueuePosition_ShouldBePositive()
        {
            // Arrange
            var reservation = new Reservation { QueuePosition = 1 };

            // Assert
            Assert.True(reservation.QueuePosition > 0);
        }

        #endregion
    }
}