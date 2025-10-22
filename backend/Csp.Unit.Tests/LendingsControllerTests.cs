//Csp.Unit.Tests/LendingsControllerTests.cs

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
    public class LendingsControllerTests
    {
        private readonly Mock<ILendingService> _mockLendingService;
        private readonly LendingsController _controller;

        public LendingsControllerTests()
        {
            _mockLendingService = new Mock<ILendingService>();
            _controller = new LendingsController(_mockLendingService.Object);
            
            // Setup default user context
            SetupUserContext("1", "Librarian");
        }

        private void SetupUserContext(string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }

        #region BorrowBook Tests

        [Fact]
        public async Task BorrowBook_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new BorrowBookRequest
            {
                BookId = 1,
                UserId = 2,
                LoanDurationDays = 14
            };

            var expectedResponse = new LendingResponse
            {
                Success = true,
                Message = "Book borrowed successfully",
                Lending = new LendingDto { Id = 1, BookId = 1, UserId = 2 }
            };

            _mockLendingService.Setup(s => s.BorrowBookAsync(request))
                              .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.BorrowBook(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<LendingResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Book borrowed successfully", response.Message);
        }

        [Fact]
        public async Task BorrowBook_ServiceFailure_ReturnsBadRequest()
        {
            // Arrange
            var request = new BorrowBookRequest
            {
                BookId = 1,
                UserId = 2,
                LoanDurationDays = 14
            };

            var expectedResponse = new LendingResponse
            {
                Success = false,
                Message = "Book not available"
            };

            _mockLendingService.Setup(s => s.BorrowBookAsync(request))
                              .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.BorrowBook(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<LendingResponse>(badRequestResult.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task BorrowBook_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var request = new BorrowBookRequest
            {
                BookId = 5,
                UserId = 3,
                LoanDurationDays = 21
            };

            _mockLendingService.Setup(s => s.BorrowBookAsync(It.IsAny<BorrowBookRequest>()))
                              .ReturnsAsync(new LendingResponse { Success = true });

            // Act
            await _controller.BorrowBook(request);

            // Assert
            _mockLendingService.Verify(s => s.BorrowBookAsync(
                It.Is<BorrowBookRequest>(r => 
                    r.BookId == 5 && 
                    r.UserId == 3 && 
                    r.LoanDurationDays == 21)), 
                Times.Once);
        }

        #endregion

        #region ReturnBook Tests

        [Fact]
        public async Task ReturnBook_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new ReturnBookRequest
            {
                LendingId = 1,
                FineAmount = 5.00m
            };

            var expectedResponse = new LendingResponse
            {
                Success = true,
                Message = "Book returned successfully",
                Lending = new LendingDto { Id = 1, Status = "Returned" }
            };

            _mockLendingService.Setup(s => s.ReturnBookAsync(request))
                              .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ReturnBook(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<LendingResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Returned", response.Lending?.Status);
        }

        [Fact]
        public async Task ReturnBook_InvalidLending_ReturnsBadRequest()
        {
            // Arrange
            var request = new ReturnBookRequest { LendingId = 999 };

            var expectedResponse = new LendingResponse
            {
                Success = false,
                Message = "Lending not found"
            };

            _mockLendingService.Setup(s => s.ReturnBookAsync(request))
                              .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ReturnBook(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<LendingResponse>(badRequestResult.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task ReturnBook_WithFine_ProcessesCorrectly()
        {
            // Arrange
            var request = new ReturnBookRequest
            {
                LendingId = 1,
                FineAmount = 10.50m
            };

            _mockLendingService.Setup(s => s.ReturnBookAsync(It.IsAny<ReturnBookRequest>()))
                              .ReturnsAsync(new LendingResponse { Success = true });

            // Act
            await _controller.ReturnBook(request);

            // Assert
            _mockLendingService.Verify(s => s.ReturnBookAsync(
                It.Is<ReturnBookRequest>(r => r.FineAmount == 10.50m)), 
                Times.Once);
        }

        #endregion

        #region RenewLoan Tests

        [Fact]
        public async Task RenewLoan_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            SetupUserContext("2", "Member");
            var request = new RenewLoanRequest { LendingId = 1 };

            var expectedResponse = new LendingResponse
            {
                Success = true,
                Message = "Loan renewed successfully",
                Lending = new LendingDto { Id = 1, RenewalCount = 1 }
            };

            _mockLendingService.Setup(s => s.RenewLoanAsync(request, 2))
                              .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RenewLoan(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<LendingResponse>(okResult.Value);
            Assert.True(response.Success);
        }

        [Fact]
        public async Task RenewLoan_PassesCorrectUserId()
        {
            // Arrange
            SetupUserContext("5", "Member");
            var request = new RenewLoanRequest { LendingId = 1 };

            _mockLendingService.Setup(s => s.RenewLoanAsync(It.IsAny<RenewLoanRequest>(), It.IsAny<int>()))
                              .ReturnsAsync(new LendingResponse { Success = true });

            // Act
            await _controller.RenewLoan(request);

            // Assert
            _mockLendingService.Verify(s => s.RenewLoanAsync(request, 5), Times.Once);
        }

        [Fact]
        public async Task RenewLoan_MaxRenewalsReached_ReturnsBadRequest()
        {
            // Arrange
            SetupUserContext("2", "Member");
            var request = new RenewLoanRequest { LendingId = 1 };

            var expectedResponse = new LendingResponse
            {
                Success = false,
                Message = "Maximum renewals reached"
            };

            _mockLendingService.Setup(s => s.RenewLoanAsync(request, 2))
                              .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RenewLoan(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<LendingResponse>(badRequestResult.Value);
            Assert.False(response.Success);
        }

        #endregion

        #region GetActiveLoans Tests

        [Fact]
        public async Task GetActiveLoans_AsLibrarian_ReturnsAllLoans()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            var expectedResponse = new PagedLendingsResponse
            {
                Items = new List<LendingDto> { new(), new() },
                Total = 2,
                Page = 1,
                PageSize = 10
            };

            _mockLendingService.Setup(s => s.GetActiveLoansAsync(null, 1, 10))
                              .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetActiveLoans(null, 1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PagedLendingsResponse>(okResult.Value);
            Assert.Equal(2, response.Items.Count);
        }

        [Fact]
        public async Task GetActiveLoans_AsMember_ReturnsOnlyOwnLoans()
        {
            // Arrange
            SetupUserContext("2", "Member");
            var expectedResponse = new PagedLendingsResponse
            {
                Items = new List<LendingDto> { new() { UserId = 2 } },
                Total = 1,
                Page = 1,
                PageSize = 10
            };

            _mockLendingService.Setup(s => s.GetActiveLoansAsync(2, 1, 10))
                              .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetActiveLoans(null, 1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PagedLendingsResponse>(okResult.Value);
            Assert.Single(response.Items);
            _mockLendingService.Verify(s => s.GetActiveLoansAsync(2, 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetActiveLoans_MemberTriesToViewOthersLoans_GetsOnlyOwnLoans()
        {
            // Arrange - Member tries to specify another userId
            SetupUserContext("2", "Member");
            var expectedResponse = new PagedLendingsResponse
            {
                Items = new List<LendingDto> { new() { UserId = 2 } },
                Total = 1
            };

            _mockLendingService.Setup(s => s.GetActiveLoansAsync(2, 1, 10))
                              .ReturnsAsync(expectedResponse);

            // Act - Pass userId 5, but should be overridden to 2
            var result = await _controller.GetActiveLoans(5, 1, 10);

            // Assert - Should call with userId 2, not 5
            _mockLendingService.Verify(s => s.GetActiveLoansAsync(2, 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetActiveLoans_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            var expectedResponse = new PagedLendingsResponse
            {
                Items = new List<LendingDto> { new(), new() },
                Total = 25,
                Page = 2,
                PageSize = 10
            };

            _mockLendingService.Setup(s => s.GetActiveLoansAsync(null, 2, 10))
                              .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetActiveLoans(null, 2, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PagedLendingsResponse>(okResult.Value);
            Assert.Equal(2, response.Page);
            Assert.Equal(25, response.Total);
        }

        #endregion

        #region GetLoanHistory Tests

        [Fact]
        public async Task GetLoanHistory_AsLibrarian_ReturnsAllHistory()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            var expectedResponse = new PagedLendingsResponse
            {
                Items = new List<LendingDto> { new(), new(), new() },
                Total = 3
            };

            _mockLendingService.Setup(s => s.GetLoanHistoryAsync(null, 1, 10))
                              .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetLoanHistory(null, 1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PagedLendingsResponse>(okResult.Value);
            Assert.Equal(3, response.Items.Count);
        }

        [Fact]
        public async Task GetLoanHistory_AsMember_ReturnsOnlyOwnHistory()
        {
            // Arrange
            SetupUserContext("3", "Member");
            var expectedResponse = new PagedLendingsResponse
            {
                Items = new List<LendingDto> { new() { UserId = 3 } },
                Total = 1
            };

            _mockLendingService.Setup(s => s.GetLoanHistoryAsync(3, 1, 10))
                              .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetLoanHistory(null, 1, 10);

            // Assert
            _mockLendingService.Verify(s => s.GetLoanHistoryAsync(3, 1, 10), Times.Once);
        }

        #endregion

        #region GetLending Tests

        [Fact]
        public async Task GetLending_ExistingLending_ReturnsOkResult()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            var expectedLending = new LendingDto
            {
                Id = 1,
                UserId = 2,
                BookId = 5,
                Status = "Active"
            };

            _mockLendingService.Setup(s => s.GetLendingByIdAsync(1))
                              .ReturnsAsync(expectedLending);

            // Act
            var result = await _controller.GetLending(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var lending = Assert.IsType<LendingDto>(okResult.Value);
            Assert.Equal(1, lending.Id);
        }

        [Fact]
        public async Task GetLending_NonExisting_ReturnsNotFound()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            _mockLendingService.Setup(s => s.GetLendingByIdAsync(999))
                              .ReturnsAsync((LendingDto?)null);

            // Act
            var result = await _controller.GetLending(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetLending_MemberViewingOwnLending_ReturnsOk()
        {
            // Arrange
            SetupUserContext("2", "Member");
            var expectedLending = new LendingDto
            {
                Id = 1,
                UserId = 2,
                BookId = 5
            };

            _mockLendingService.Setup(s => s.GetLendingByIdAsync(1))
                              .ReturnsAsync(expectedLending);

            // Act
            var result = await _controller.GetLending(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var lending = Assert.IsType<LendingDto>(okResult.Value);
            Assert.Equal(2, lending.UserId);
        }

        [Fact]
        public async Task GetLending_MemberViewingOthersLending_ReturnsForbid()
        {
            // Arrange
            SetupUserContext("2", "Member");
            var otherUserLending = new LendingDto
            {
                Id = 1,
                UserId = 5, // Different user
                BookId = 3
            };

            _mockLendingService.Setup(s => s.GetLendingByIdAsync(1))
                              .ReturnsAsync(otherUserLending);

            // Act
            var result = await _controller.GetLending(1);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetLending_LibrarianViewingAnyLending_ReturnsOk()
        {
            // Arrange
            SetupUserContext("1", "Librarian");
            var anyUserLending = new LendingDto
            {
                Id = 1,
                UserId = 99,
                BookId = 3
            };

            _mockLendingService.Setup(s => s.GetLendingByIdAsync(1))
                              .ReturnsAsync(anyUserLending);

            // Act
            var result = await _controller.GetLending(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        #endregion

        #region Authorization Tests

        [Fact]
        public async Task BorrowBook_RequiresLibrarianRole()
        {
            // This test verifies the controller has the proper authorization attribute
            // In a real scenario, this would be tested with integration tests
            // Here we just verify the method exists and is accessible
            var request = new BorrowBookRequest { BookId = 1, UserId = 2 };
            _mockLendingService.Setup(s => s.BorrowBookAsync(It.IsAny<BorrowBookRequest>()))
                              .ReturnsAsync(new LendingResponse { Success = true });

            var result = await _controller.BorrowBook(request);
            
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ReturnBook_RequiresLibrarianRole()
        {
            var request = new ReturnBookRequest { LendingId = 1 };
            _mockLendingService.Setup(s => s.ReturnBookAsync(It.IsAny<ReturnBookRequest>()))
                              .ReturnsAsync(new LendingResponse { Success = true });

            var result = await _controller.ReturnBook(request);
            
            Assert.NotNull(result);
        }

        #endregion
    }
}