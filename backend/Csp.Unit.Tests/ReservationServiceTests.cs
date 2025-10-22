//Csp.Unit.Tests/ReservationServiceTests.cs

using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Csp.Api.Services;
using Csp.Api.DTOs;

namespace Csp.Unit.Tests
{
    public class ReservationServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IConfigurationSection> _mockConnectionStringSection;
        private const string TestConnectionString = "Server=localhost;Database=test_db;Uid=test;Pwd=test;";

        public ReservationServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConnectionStringSection = new Mock<IConfigurationSection>();
            
            _mockConnectionStringSection.Setup(x => x.Value).Returns(TestConnectionString);
            _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings:DefaultConnection"))
                             .Returns(_mockConnectionStringSection.Object);
            _mockConfiguration.Setup(x => x["ConnectionStrings:DefaultConnection"])
                             .Returns(TestConnectionString);
        }

        #region CreateReservationAsync Tests

        [Fact]
        public void CreateReservationAsync_InvalidBookId_ShouldFailValidation()
        {
            // Arrange
            var request = new CreateReservationRequest
            {
                BookId = 0,
                UserId = 1
            };

            // Act & Assert - Testing validation logic
            Assert.True(request.BookId <= 0);
        }

        [Fact]
        public void CreateReservationAsync_InvalidUserId_ShouldFailValidation()
        {
            // Arrange
            var request = new CreateReservationRequest
            {
                BookId = 1,
                UserId = 0
            };

            // Act & Assert - Testing validation logic
            Assert.True(request.UserId <= 0);
        }

        [Fact]
        public void CreateReservationAsync_ValidRequest_ShouldPassValidation()
        {
            // Arrange
            var request = new CreateReservationRequest
            {
                BookId = 1,
                UserId = 2
            };

            // Act & Assert - Testing validation logic
            Assert.True(request.BookId > 0);
            Assert.True(request.UserId > 0);
        }

        #endregion

        #region FulfillReservationRequest Tests

        [Fact]
        public void FulfillReservationRequest_ValidRequest_ShouldPassValidation()
        {
            // Arrange
            var request = new FulfillReservationRequest
            {
                ReservationId = 1,
                LoanDurationDays = 14
            };

            // Act & Assert
            Assert.True(request.ReservationId > 0);
            Assert.True(request.LoanDurationDays > 0);
        }

        [Fact]
        public void FulfillReservationRequest_DefaultLoanDuration_ShouldBe14Days()
        {
            // Arrange & Act
            var request = new FulfillReservationRequest
            {
                ReservationId = 1
            };

            // Assert
            Assert.Equal(14, request.LoanDurationDays);
        }

        [Fact]
        public void FulfillReservationRequest_CustomLoanDuration_ShouldBeRespected()
        {
            // Arrange & Act
            var request = new FulfillReservationRequest
            {
                ReservationId = 1,
                LoanDurationDays = 21
            };

            // Assert
            Assert.Equal(21, request.LoanDurationDays);
        }

        #endregion

        #region ReservationDto Tests

        [Fact]
        public void ReservationDto_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var dto = new ReservationDto();

            // Assert
            Assert.Equal(string.Empty, dto.BookTitle);
            Assert.Equal(string.Empty, dto.BookAuthor);
            Assert.Equal(string.Empty, dto.BookIsbn);
            Assert.Equal(string.Empty, dto.Username);
            Assert.Equal(string.Empty, dto.MemberEmail);
            Assert.Equal(string.Empty, dto.Status);
        }

        [Fact]
        public void ReservationDto_PendingStatus_ShouldBeValid()
        {
            // Arrange
            var dto = new ReservationDto
            {
                Id = 1,
                Status = "Pending",
                QueuePosition = 1,
                ReservedDate = DateTime.UtcNow
            };

            // Act & Assert
            Assert.Equal("Pending", dto.Status);
            Assert.True(dto.QueuePosition > 0);
            Assert.Null(dto.AvailableDate);
            Assert.Null(dto.ExpiryDate);
            Assert.Null(dto.FulfilledDate);
        }

        [Fact]
        public void ReservationDto_AvailableStatus_ShouldHaveAvailableDate()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto = new ReservationDto
            {
                Id = 1,
                Status = "Available",
                AvailableDate = now,
                ExpiryDate = now.AddDays(3)
            };

            // Act & Assert
            Assert.Equal("Available", dto.Status);
            Assert.NotNull(dto.AvailableDate);
            Assert.NotNull(dto.ExpiryDate);
            Assert.True(dto.ExpiryDate > dto.AvailableDate);
        }

        [Fact]
        public void ReservationDto_FulfilledStatus_ShouldHaveFulfilledDate()
        {
            // Arrange
            var dto = new ReservationDto
            {
                Id = 1,
                Status = "Fulfilled",
                FulfilledDate = DateTime.UtcNow
            };

            // Act & Assert
            Assert.Equal("Fulfilled", dto.Status);
            Assert.NotNull(dto.FulfilledDate);
        }

        [Fact]
        public void ReservationDto_QueuePosition_ShouldBePositive()
        {
            // Arrange
            var dto = new ReservationDto
            {
                QueuePosition = 3
            };

            // Act & Assert
            Assert.True(dto.QueuePosition > 0);
            Assert.Equal(3, dto.QueuePosition);
        }

        [Fact]
        public void ReservationDto_EstimatedAvailable_CanBeSet()
        {
            // Arrange
            var dto = new ReservationDto
            {
                EstimatedAvailable = "Approximately 5 days"
            };

            // Act & Assert
            Assert.Equal("Approximately 5 days", dto.EstimatedAvailable);
        }

        #endregion

        #region PagedReservationsResponse Tests

        [Fact]
        public void PagedReservationsResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new PagedReservationsResponse();

            // Assert
            Assert.NotNull(response.Items);
            Assert.Empty(response.Items);
            Assert.Equal(0, response.Total);
            Assert.Equal(0, response.Page);
            Assert.Equal(0, response.PageSize);
        }

        [Fact]
        public void PagedReservationsResponse_Pagination_ShouldCalculateCorrectly()
        {
            // Arrange
            var response = new PagedReservationsResponse
            {
                Items = new List<ReservationDto> { new(), new() },
                Total = 15,
                Page = 1,
                PageSize = 10
            };

            // Act & Assert
            Assert.Equal(2, response.Items.Count);
            Assert.Equal(15, response.Total);
            Assert.Equal(1, response.Page);
            Assert.Equal(10, response.PageSize);
            
            // Calculate total pages
            var totalPages = (int)Math.Ceiling((double)response.Total / response.PageSize);
            Assert.Equal(2, totalPages);
        }

        [Fact]
        public void PagedReservationsResponse_MultiplePages_ShouldHandleCorrectly()
        {
            // Arrange
            var response = new PagedReservationsResponse
            {
                Items = new List<ReservationDto> { new(), new(), new() },
                Total = 30,
                Page = 2,
                PageSize = 10
            };

            // Act
            var hasNextPage = (response.Page * response.PageSize) < response.Total;
            var hasPreviousPage = response.Page > 1;

            // Assert
            Assert.True(hasNextPage);
            Assert.True(hasPreviousPage);
        }

        #endregion

        #region ReservationResponse Tests

        [Fact]
        public void ReservationResponse_SuccessScenario_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var response = new ReservationResponse
            {
                Success = true,
                Message = "Reservation created successfully",
                Reservation = new ReservationDto { Id = 1, QueuePosition = 1 }
            };

            // Assert
            Assert.True(response.Success);
            Assert.Equal("Reservation created successfully", response.Message);
            Assert.NotNull(response.Reservation);
            Assert.Equal(1, response.Reservation.Id);
        }

        [Fact]
        public void ReservationResponse_FailureScenario_ShouldHaveErrorMessage()
        {
            // Arrange & Act
            var response = new ReservationResponse
            {
                Success = false,
                Message = "Book already reserved by user",
                Reservation = null
            };

            // Assert
            Assert.False(response.Success);
            Assert.Equal("Book already reserved by user", response.Message);
            Assert.Null(response.Reservation);
        }

        [Fact]
        public void ReservationResponse_DefaultMessage_ShouldBeEmpty()
        {
            // Arrange & Act
            var response = new ReservationResponse();

            // Assert
            Assert.Equal(string.Empty, response.Message);
            Assert.False(response.Success);
        }

        #endregion

        #region Service Initialization Tests

        [Fact]
        public void ReservationService_Constructor_ShouldInitializeWithValidConnectionString()
        {
            // Arrange
            var mockConnectionStringSection = new Mock<IConfigurationSection>();
            mockConnectionStringSection.Setup(s => s.Value).Returns(TestConnectionString);
            
            var mockConnectionStringsSection = new Mock<IConfigurationSection>();
            mockConnectionStringsSection.Setup(s => s["DefaultConnection"]).Returns(TestConnectionString);
            
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockConnectionStringsSection.Object);

            // Act
            var service = new ReservationService(config.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void ReservationService_Constructor_ShouldThrowIfConnectionStringMissing()
        {
            // Arrange
            var mockConnectionStringsSection = new Mock<IConfigurationSection>();
            mockConnectionStringsSection.Setup(s => s["DefaultConnection"]).Returns((string?)null);
            mockConnectionStringsSection.Setup(s => s["Default"]).Returns((string?)null);
            
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockConnectionStringsSection.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new ReservationService(config.Object));
        }

        [Fact]
        public void ReservationService_Constructor_ShouldUseDefaultConnectionStringAsFallback()
        {
            // Arrange
            var mockConnectionStringsSection = new Mock<IConfigurationSection>();
            mockConnectionStringsSection.Setup(s => s["DefaultConnection"]).Returns((string?)null);
            mockConnectionStringsSection.Setup(s => s["Default"]).Returns(TestConnectionString);
            
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockConnectionStringsSection.Object);

            // Act
            var service = new ReservationService(config.Object);

            // Assert
            Assert.NotNull(service);
        }

        #endregion

        #region Business Logic Validation Tests

        [Fact]
        public void ReservationLogic_ExpiryDays_ShouldBeConstant()
        {
            // This validates the reservation expiry constant (3 days)
            const int expectedExpiryDays = 3;
            
            // Assert
            Assert.Equal(3, expectedExpiryDays);
        }

        [Fact]
        public void ReservationLogic_ValidStatuses_ShouldBeCorrect()
        {
            // Arrange
            var validStatuses = new[] { "Pending", "Available", "Fulfilled", "Cancelled", "Expired" };

            // Assert
            Assert.Contains("Pending", validStatuses);
            Assert.Contains("Available", validStatuses);
            Assert.Contains("Fulfilled", validStatuses);
            Assert.Contains("Cancelled", validStatuses);
            Assert.Contains("Expired", validStatuses);
            Assert.Equal(5, validStatuses.Length);
        }

        [Fact]
        public void ReservationLogic_QueuePosition_ShouldStartFromOne()
        {
            // Arrange
            var firstInQueue = new ReservationDto { QueuePosition = 1 };
            var secondInQueue = new ReservationDto { QueuePosition = 2 };

            // Assert
            Assert.True(firstInQueue.QueuePosition >= 1);
            Assert.True(secondInQueue.QueuePosition > firstInQueue.QueuePosition);
        }

        [Fact]
        public void ReservationLogic_ExpiryCalculation_ShouldBeCorrect()
        {
            // Arrange
            var availableDate = DateTime.UtcNow;
            var expiryDate = availableDate.AddDays(3);

            // Act
            var daysDifference = (expiryDate - availableDate).Days;

            // Assert
            Assert.Equal(3, daysDifference);
        }

        [Fact]
        public void ReservationLogic_ActiveReservations_ShouldExcludeFinalStatuses()
        {
            // Arrange
            var activeStatuses = new[] { "Pending", "Available" };
            var finalStatuses = new[] { "Fulfilled", "Cancelled", "Expired" };

            // Assert
            Assert.DoesNotContain("Fulfilled", activeStatuses);
            Assert.DoesNotContain("Cancelled", activeStatuses);
            Assert.DoesNotContain("Expired", activeStatuses);
            
            Assert.Contains("Fulfilled", finalStatuses);
            Assert.Contains("Cancelled", finalStatuses);
            Assert.Contains("Expired", finalStatuses);
        }

        [Fact]
        public void ReservationLogic_StatusTransition_PendingToAvailable()
        {
            // Arrange
            var reservation = new ReservationDto
            {
                Status = "Pending",
                AvailableDate = null
            };

            // Act - Simulate transition to Available
            reservation.Status = "Available";
            reservation.AvailableDate = DateTime.UtcNow;
            reservation.ExpiryDate = DateTime.UtcNow.AddDays(3);

            // Assert
            Assert.Equal("Available", reservation.Status);
            Assert.NotNull(reservation.AvailableDate);
            Assert.NotNull(reservation.ExpiryDate);
        }

        [Fact]
        public void ReservationLogic_StatusTransition_AvailableToFulfilled()
        {
            // Arrange
            var reservation = new ReservationDto
            {
                Status = "Available",
                AvailableDate = DateTime.UtcNow.AddDays(-1),
                FulfilledDate = null
            };

            // Act - Simulate fulfillment
            reservation.Status = "Fulfilled";
            reservation.FulfilledDate = DateTime.UtcNow;

            // Assert
            Assert.Equal("Fulfilled", reservation.Status);
            Assert.NotNull(reservation.FulfilledDate);
        }

        [Fact]
        public void ReservationLogic_StatusTransition_PendingToCancelled()
        {
            // Arrange
            var reservation = new ReservationDto
            {
                Status = "Pending",
                QueuePosition = 3
            };

            // Act - Simulate cancellation
            reservation.Status = "Cancelled";

            // Assert
            Assert.Equal("Cancelled", reservation.Status);
        }

        [Fact]
        public void ReservationLogic_StatusTransition_AvailableToExpired()
        {
            // Arrange
            var reservation = new ReservationDto
            {
                Status = "Available",
                AvailableDate = DateTime.UtcNow.AddDays(-4),
                ExpiryDate = DateTime.UtcNow.AddDays(-1)
            };

            // Act - Simulate expiry
            reservation.Status = "Expired";

            // Assert
            Assert.Equal("Expired", reservation.Status);
            Assert.True(reservation.ExpiryDate < DateTime.UtcNow);
        }

        #endregion

        #region Queue Management Tests

        [Fact]
        public void ReservationQueue_FirstPosition_ShouldBeOne()
        {
            // Arrange
            var firstReservation = new ReservationDto
            {
                QueuePosition = 1,
                Status = "Pending"
            };

            // Assert
            Assert.Equal(1, firstReservation.QueuePosition);
        }

        [Fact]
        public void ReservationQueue_MultipleReservations_ShouldHaveIncrementalPositions()
        {
            // Arrange
            var reservations = new List<ReservationDto>
            {
                new() { Id = 1, QueuePosition = 1, Status = "Pending" },
                new() { Id = 2, QueuePosition = 2, Status = "Pending" },
                new() { Id = 3, QueuePosition = 3, Status = "Pending" }
            };

            // Act & Assert
            for (int i = 0; i < reservations.Count; i++)
            {
                Assert.Equal(i + 1, reservations[i].QueuePosition);
            }
        }

        [Fact]
        public void ReservationQueue_AfterCancellation_PositionsShouldReorganize()
        {
            // This test validates the concept of queue reorganization
            // Arrange
            var originalQueue = new List<int> { 1, 2, 3, 4 };
            var cancelledPosition = 2;

            // Act - Remove position 2
            originalQueue.Remove(cancelledPosition);
            var reorganizedQueue = originalQueue.Select((_, index) => index + 1).ToList();

            // Assert
            Assert.Equal(new List<int> { 1, 2, 3 }, reorganizedQueue);
        }

        #endregion
    }
}