//Csp.Unit.Tests/FinesControllerTests.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using System.Security.Claims;
using Csp.Api.Controllers;
using Csp.Api.Services;
using Csp.Api.DTOs;

namespace Csp.Unit.Tests
{
    public class FinesControllerTests
    {
        private readonly Mock<IFineService> _mockFineService;
        private readonly FinesController _controller;

        public FinesControllerTests()
        {
            _mockFineService = new Mock<IFineService>();
            _controller = new FinesController(_mockFineService.Object);
            
            // Setup default user context
            SetupUserContext("1", "Member");
        }

        #region GetMyFines Tests

        [Fact]
        public async Task GetMyFines_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var expectedResponse = new PagedFinesResponse
            {
                Items = new List<FineDto>
                {
                    new FineDto { Id = 1, Amount = 100.00m, Status = "Outstanding" }
                },
                Total = 1,
                Page = 1,
                PageSize = 10
            };

            _mockFineService.Setup(s => s.CreateFinesForOverdueLoansAsync())
                           .ReturnsAsync(0);
            _mockFineService.Setup(s => s.GetUserFinesAsync(1, 1, 10))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetMyFines(1, 10);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedFinesResponse>(actionResult.Value);
            Assert.Single(returnValue.Items);
            Assert.Equal(1, returnValue.Total);
            _mockFineService.Verify(s => s.CreateFinesForOverdueLoansAsync(), Times.Once);
            _mockFineService.Verify(s => s.GetUserFinesAsync(1, 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetMyFines_InvalidUserToken_ReturnsUnauthorized()
        {
            // Arrange
            SetupUserContext("invalid", "Member");

            // Act
            var result = await _controller.GetMyFines();

            // Assert
            var actionResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(actionResult.Value);
        }

        [Fact]
        public async Task GetMyFines_WithPagination_PassesCorrectParameters()
        {
            // Arrange
            var page = 2;
            var pageSize = 20;
            var expectedResponse = new PagedFinesResponse
            {
                Items = new List<FineDto>(),
                Total = 0,
                Page = page,
                PageSize = pageSize
            };

            _mockFineService.Setup(s => s.CreateFinesForOverdueLoansAsync())
                           .ReturnsAsync(0);
            _mockFineService.Setup(s => s.GetUserFinesAsync(1, page, pageSize))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetMyFines(page, pageSize);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedFinesResponse>(actionResult.Value);
            Assert.Equal(page, returnValue.Page);
            Assert.Equal(pageSize, returnValue.PageSize);
            _mockFineService.Verify(s => s.GetUserFinesAsync(1, page, pageSize), Times.Once);
        }

        #endregion

        #region GetAllFines Tests

        [Fact]
        public async Task GetAllFines_LibrarianRole_ReturnsOkResult()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            var expectedResponse = new PagedFinesResponse
            {
                Items = new List<FineDto>
                {
                    new FineDto { Id = 1, Amount = 100.00m },
                    new FineDto { Id = 2, Amount = 200.00m }
                },
                Total = 2,
                Page = 1,
                PageSize = 10
            };

            _mockFineService.Setup(s => s.CreateFinesForOverdueLoansAsync())
                           .ReturnsAsync(0);
            _mockFineService.Setup(s => s.GetAllFinesAsync(1, 10))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAllFines(1, 10);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedFinesResponse>(actionResult.Value);
            Assert.Equal(2, returnValue.Items.Count);
            Assert.Equal(2, returnValue.Total);
            _mockFineService.Verify(s => s.CreateFinesForOverdueLoansAsync(), Times.Once);
            _mockFineService.Verify(s => s.GetAllFinesAsync(1, 10), Times.Once);
        }

        #endregion

        #region GetUserFines Tests

        [Fact]
        public async Task GetUserFines_LibrarianRole_ReturnsOkResult()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            var userId = 5;
            var expectedResponse = new PagedFinesResponse
            {
                Items = new List<FineDto>
                {
                    new FineDto { Id = 1, UserId = userId, Amount = 100.00m }
                },
                Total = 1,
                Page = 1,
                PageSize = 10
            };

            _mockFineService.Setup(s => s.GetUserFinesAsync(userId, 1, 10))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUserFines(userId, 1, 10);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedFinesResponse>(actionResult.Value);
            Assert.Single(returnValue.Items);
            Assert.Equal(userId, returnValue.Items.First().UserId);
            _mockFineService.Verify(s => s.GetUserFinesAsync(userId, 1, 10), Times.Once);
        }

        #endregion

        #region GetMyPayments Tests

        [Fact]
        public async Task GetMyPayments_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var expectedResponse = new PagedPaymentsResponse
            {
                Items = new List<PaymentDto>
                {
                    new PaymentDto { Id = 1, Amount = 100.00m }
                },
                Total = 1,
                Page = 1,
                PageSize = 10
            };

            _mockFineService.Setup(s => s.GetUserPaymentsAsync(1, 1, 10))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetMyPayments(1, 10);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedPaymentsResponse>(actionResult.Value);
            Assert.Single(returnValue.Items);
            Assert.Equal(1, returnValue.Total);
            _mockFineService.Verify(s => s.GetUserPaymentsAsync(1, 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetMyPayments_InvalidUserToken_ReturnsUnauthorized()
        {
            // Arrange
            SetupUserContext("invalid", "Member");

            // Act
            var result = await _controller.GetMyPayments();

            // Assert
            var actionResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(actionResult.Value);
        }

        #endregion

        #region GetAllPayments Tests

        [Fact]
        public async Task GetAllPayments_LibrarianRole_ReturnsOkResult()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            var expectedResponse = new PagedPaymentsResponse
            {
                Items = new List<PaymentDto>
                {
                    new PaymentDto { Id = 1, Amount = 100.00m },
                    new PaymentDto { Id = 2, Amount = 200.00m }
                },
                Total = 2,
                Page = 1,
                PageSize = 10
            };

            _mockFineService.Setup(s => s.GetAllPaymentsAsync(1, 10))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAllPayments(1, 10);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedPaymentsResponse>(actionResult.Value);
            Assert.Equal(2, returnValue.Items.Count);
            Assert.Equal(2, returnValue.Total);
            _mockFineService.Verify(s => s.GetAllPaymentsAsync(1, 10), Times.Once);
        }

        #endregion

        #region GetUserPayments Tests

        [Fact]
        public async Task GetUserPayments_LibrarianRole_ReturnsOkResult()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            var userId = 5;
            var expectedResponse = new PagedPaymentsResponse
            {
                Items = new List<PaymentDto>
                {
                    new PaymentDto { Id = 1, UserId = userId, Amount = 100.00m }
                },
                Total = 1,
                Page = 1,
                PageSize = 10
            };

            _mockFineService.Setup(s => s.GetUserPaymentsAsync(userId, 1, 10))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUserPayments(userId, 1, 10);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedPaymentsResponse>(actionResult.Value);
            Assert.Single(returnValue.Items);
            Assert.Equal(userId, returnValue.Items.First().UserId);
            _mockFineService.Verify(s => s.GetUserPaymentsAsync(userId, 1, 10), Times.Once);
        }

        #endregion

        #region GetMyStatistics Tests

        [Fact]
        public async Task GetMyStatistics_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var expectedStats = new FineStatistics
            {
                TotalOutstanding = 500.00m,
                OutstandingCount = 5,
                TotalPaid = 300.00m,
                PaidCount = 3,
                TotalWaived = 100.00m,
                WaivedCount = 1
            };

            _mockFineService.Setup(s => s.GetUserFineStatisticsAsync(1))
                           .ReturnsAsync(expectedStats);

            // Act
            var result = await _controller.GetMyStatistics();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<FineStatistics>(actionResult.Value);
            Assert.Equal(500.00m, returnValue.TotalOutstanding);
            Assert.Equal(5, returnValue.OutstandingCount);
            Assert.Equal(300.00m, returnValue.TotalPaid);
            Assert.Equal(3, returnValue.PaidCount);
            _mockFineService.Verify(s => s.GetUserFineStatisticsAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetMyStatistics_InvalidUserToken_ReturnsUnauthorized()
        {
            // Arrange
            SetupUserContext("invalid", "Member");

            // Act
            var result = await _controller.GetMyStatistics();

            // Assert
            var actionResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(actionResult.Value);
        }

        #endregion

        #region ProcessPayment Tests

        [Fact]
        public async Task ProcessPayment_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new ProcessPaymentRequest
            {
                FineId = 1,
                TransactionId = "TXN-12345"
            };

            var expectedResponse = new PaymentResponse
            {
                Success = true,
                Message = "Payment processed successfully",
                Payment = new PaymentDto { Id = 1, Amount = 100.00m }
            };

            _mockFineService.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>()))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ProcessPayment(request);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaymentResponse>(actionResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal("Payment processed successfully", returnValue.Message);
            _mockFineService.Verify(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPayment_FailedPayment_ReturnsBadRequest()
        {
            // Arrange
            var request = new ProcessPaymentRequest
            {
                FineId = 1,
                TransactionId = "TXN-12345"
            };

            var expectedResponse = new PaymentResponse
            {
                Success = false,
                Message = "Fine not found"
            };

            _mockFineService.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>()))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ProcessPayment(request);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<PaymentResponse>(actionResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Fine not found", returnValue.Message);
        }

        [Fact]
        public async Task ProcessPayment_InvalidUserToken_ReturnsUnauthorized()
        {
            // Arrange
            SetupUserContext("invalid", "Member");
            var request = new ProcessPaymentRequest
            {
                FineId = 1,
                TransactionId = "TXN-12345"
            };

            // Act
            var result = await _controller.ProcessPayment(request);

            // Assert
            var actionResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(actionResult.Value);
        }

        [Fact]
        public async Task ProcessPayment_SetsUserIdFromToken_CorrectlyAssignsUserId()
        {
            // Arrange
            SetupUserContext("5", "Member");
            var request = new ProcessPaymentRequest
            {
                FineId = 1,
                UserId = 999 // This should be overridden
            };

            var expectedResponse = new PaymentResponse
            {
                Success = true,
                Message = "Payment processed successfully",
                Payment = new PaymentDto { Id = 1, UserId = 5 }
            };

            _mockFineService.Setup(s => s.ProcessPaymentAsync(It.Is<ProcessPaymentRequest>(
                r => r.UserId == 5 && r.FineId == 1)))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ProcessPayment(request);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaymentResponse>(actionResult.Value);
            Assert.True(returnValue.Success);
            Assert.NotNull(returnValue.Payment);
            Assert.Equal(5, returnValue.Payment.UserId);
        }

        #endregion

        #region WaiveFine Tests

        [Fact]
        public async Task WaiveFine_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            var request = new WaiveFineRequest
            {
                FineId = 1,
                Reason = "First time offense"
            };

            var expectedResponse = new FineResponse
            {
                Success = true,
                Message = "Fine waived successfully"
            };

            _mockFineService.Setup(s => s.WaiveFineAsync(request))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.WaiveFine(request);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<FineResponse>(actionResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal("Fine waived successfully", returnValue.Message);
            _mockFineService.Verify(s => s.WaiveFineAsync(request), Times.Once);
        }

        [Fact]
        public async Task WaiveFine_FineNotFound_ReturnsBadRequest()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            var request = new WaiveFineRequest
            {
                FineId = 999,
                Reason = "First time offense"
            };

            var expectedResponse = new FineResponse
            {
                Success = false,
                Message = "Fine not found"
            };

            _mockFineService.Setup(s => s.WaiveFineAsync(request))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.WaiveFine(request);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<FineResponse>(actionResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Fine not found", returnValue.Message);
        }

        #endregion

        #region AdjustFineAmount Tests

        [Fact]
        public async Task AdjustFineAmount_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            var request = new AdjustFineAmountRequest
            {
                FineId = 1,
                NewAmount = 75.50m,
                Reason = "Adjusted for good standing"
            };

            var expectedResponse = new FineResponse
            {
                Success = true,
                Message = "Fine amount adjusted to ₹75.50"
            };

            _mockFineService.Setup(s => s.AdjustFineAmountAsync(request))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.AdjustFineAmount(request);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<FineResponse>(actionResult.Value);
            Assert.True(returnValue.Success);
            Assert.Contains("75.50", returnValue.Message);
            _mockFineService.Verify(s => s.AdjustFineAmountAsync(request), Times.Once);
        }

        [Fact]
        public async Task AdjustFineAmount_FineNotFound_ReturnsBadRequest()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            var request = new AdjustFineAmountRequest
            {
                FineId = 999,
                NewAmount = 75.50m,
                Reason = "Adjusted for good standing"
            };

            var expectedResponse = new FineResponse
            {
                Success = false,
                Message = "Fine not found"
            };

            _mockFineService.Setup(s => s.AdjustFineAmountAsync(request))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.AdjustFineAmount(request);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<FineResponse>(actionResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Fine not found", returnValue.Message);
        }

        #endregion

        #region GenerateOverdueFines Tests

        [Fact]
        public async Task GenerateOverdueFines_LibrarianRole_ReturnsOkResult()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            _mockFineService.Setup(s => s.CreateFinesForOverdueLoansAsync())
                           .ReturnsAsync(5);

            // Act
            var result = await _controller.GenerateOverdueFines();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(actionResult.Value);
            _mockFineService.Verify(s => s.CreateFinesForOverdueLoansAsync(), Times.Once);
        }

        [Fact]
        public async Task GenerateOverdueFines_NoOverdueLoans_ReturnsZeroCount()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            _mockFineService.Setup(s => s.CreateFinesForOverdueLoansAsync())
                           .ReturnsAsync(0);

            // Act
            var result = await _controller.GenerateOverdueFines();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(actionResult.Value);
            _mockFineService.Verify(s => s.CreateFinesForOverdueLoansAsync(), Times.Once);
        }

        #endregion

        #region CheckOverdueLoans Tests

        [Fact]
        public async Task CheckOverdueLoans_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            _mockFineService.Setup(s => s.CreateFinesForOverdueLoansAsync())
                           .ReturnsAsync(3);

            // Act
            var result = await _controller.CheckOverdueLoans();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(actionResult.Value);
            _mockFineService.Verify(s => s.CreateFinesForOverdueLoansAsync(), Times.Once);
        }

        #endregion

        #region Helper Methods

        private void SetupUserContext(string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        #endregion
    }
}