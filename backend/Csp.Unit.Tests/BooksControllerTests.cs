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
    public class BooksControllerTests
    {
        private readonly Mock<IBookService> _mockBookService;
        private readonly BooksController _controller;

        public BooksControllerTests()
        {
            _mockBookService = new Mock<IBookService>();
            _controller = new BooksController(_mockBookService.Object);
            
            // Setup default user context
            SetupUserContext("1", "Librarian");
        }

        #region CreateBook Tests

        [Fact]
        public async Task CreateBook_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var request = new CreateBookRequest
            {
                Title = "Test Book",
                Author = "Test Author",
                Isbn = "9781234567890",
                Category = "Fiction",
                PublishedYear = 2023,
                TotalCopies = 5
            };

            var expectedResponse = new BookResponse
            {
                Success = true,
                Message = "Book created successfully",
                Book = new BookDto { Id = 1, Title = "Test Book" }
            };

            _mockBookService.Setup(s => s.CreateBookAsync(request, 1))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateBook(request);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<BookResponse>(actionResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal("Book created successfully", returnValue.Message);
            _mockBookService.Verify(s => s.CreateBookAsync(request, 1), Times.Once);
        }

        [Fact]
        public async Task CreateBook_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateBookRequest
            {
                Title = "",
                Author = "Test Author",
                Isbn = "9781234567890",
                Category = "Fiction",
                PublishedYear = 2023,
                TotalCopies = 5
            };

            var expectedResponse = new BookResponse
            {
                Success = false,
                Message = "Title, Author, ISBN, and Category are required"
            };

            _mockBookService.Setup(s => s.CreateBookAsync(request, 1))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateBook(request);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<BookResponse>(actionResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Title, Author, ISBN, and Category are required", returnValue.Message);
        }

        [Fact]
        public async Task CreateBook_DuplicateIsbn_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateBookRequest
            {
                Title = "Test Book",
                Author = "Test Author",
                Isbn = "9781234567890",
                Category = "Fiction",
                PublishedYear = 2023,
                TotalCopies = 5
            };

            var expectedResponse = new BookResponse
            {
                Success = false,
                Message = "A book with this ISBN already exists"
            };

            _mockBookService.Setup(s => s.CreateBookAsync(request, 1))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateBook(request);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<BookResponse>(actionResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("A book with this ISBN already exists", returnValue.Message);
        }

        [Fact]
        public async Task CreateBook_InvalidUserToken_ThrowsUnauthorizedException()
        {
            // Arrange
            SetupUserContext("invalid", "Librarian");
            var request = new CreateBookRequest
            {
                Title = "Test Book",
                Author = "Test Author",
                Isbn = "9781234567890",
                Category = "Fiction",
                PublishedYear = 2023,
                TotalCopies = 5
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.CreateBook(request));
        }

        #endregion

        #region UpdateBook Tests

        [Fact]
        public async Task UpdateBook_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var bookId = 1;
            var request = new UpdateBookRequest
            {
                Title = "Updated Book",
                Author = "Updated Author",
                Isbn = "9781234567890",
                Category = "Fiction",
                PublishedYear = 2023,
                TotalCopies = 10
            };

            var expectedResponse = new BookResponse
            {
                Success = true,
                Message = "Book updated successfully",
                Book = new BookDto { Id = bookId, Title = "Updated Book" }
            };

            _mockBookService.Setup(s => s.UpdateBookAsync(bookId, request, 1))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateBook(bookId, request);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<BookResponse>(actionResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal("Book updated successfully", returnValue.Message);
            _mockBookService.Verify(s => s.UpdateBookAsync(bookId, request, 1), Times.Once);
        }

        [Fact]
        public async Task UpdateBook_BookNotFound_ReturnsBadRequest()
        {
            // Arrange
            var bookId = 999;
            var request = new UpdateBookRequest
            {
                Title = "Updated Book",
                Author = "Updated Author",
                Isbn = "9781234567890",
                Category = "Fiction",
                PublishedYear = 2023,
                TotalCopies = 10
            };

            var expectedResponse = new BookResponse
            {
                Success = false,
                Message = "Book not found"
            };

            _mockBookService.Setup(s => s.UpdateBookAsync(bookId, request, 1))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateBook(bookId, request);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<BookResponse>(actionResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Book not found", returnValue.Message);
        }

        [Fact]
        public async Task UpdateBook_InvalidIsbnLength_ReturnsBadRequest()
        {
            // Arrange
            var bookId = 1;
            var request = new UpdateBookRequest
            {
                Title = "Updated Book",
                Author = "Updated Author",
                Isbn = "123", // Too short
                Category = "Fiction",
                PublishedYear = 2023,
                TotalCopies = 10
            };

            var expectedResponse = new BookResponse
            {
                Success = false,
                Message = "ISBN must be between 10 and 20 characters"
            };

            _mockBookService.Setup(s => s.UpdateBookAsync(bookId, request, 1))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateBook(bookId, request);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<BookResponse>(actionResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("ISBN must be between 10 and 20 characters", returnValue.Message);
        }

        #endregion

        #region UpdateBookStatus Tests

        [Fact]
        public async Task UpdateBookStatus_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var bookId = 1;
            var isActive = false;

            var expectedResponse = new BookResponse
            {
                Success = true,
                Message = "Book deactivated successfully",
                Book = new BookDto { Id = bookId, IsActive = false }
            };

            _mockBookService.Setup(s => s.UpdateBookStatusAsync(bookId, isActive, 1))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateBookStatus(bookId, isActive);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<BookResponse>(actionResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal("Book deactivated successfully", returnValue.Message);
            _mockBookService.Verify(s => s.UpdateBookStatusAsync(bookId, isActive, 1), Times.Once);
        }

        [Fact]
        public async Task UpdateBookStatus_BookNotFound_ReturnsBadRequest()
        {
            // Arrange
            var bookId = 999;
            var isActive = false;

            var expectedResponse = new BookResponse
            {
                Success = false,
                Message = "Book not found"
            };

            _mockBookService.Setup(s => s.UpdateBookStatusAsync(bookId, isActive, 1))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateBookStatus(bookId, isActive);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<BookResponse>(actionResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Book not found", returnValue.Message);
        }

        [Fact]
        public async Task UpdateBookStatus_ServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var bookId = 1;
            var isActive = false;

            _mockBookService.Setup(s => s.UpdateBookStatusAsync(bookId, isActive, 1))
                           .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.UpdateBookStatus(bookId, isActive);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<BookResponse>(actionResult.Value);
            Assert.False(returnValue.Success);
            Assert.Contains("Controller error:", returnValue.Message);
        }

        #endregion

        #region GetBooks Tests

        [Fact]
        public async Task GetBooks_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var search = "test";
            var category = "Fiction";
            var author = "Test Author";
            var page = 1;
            var pageSize = 10;

            var expectedResponse = new PagedBooksResponse
            {
                Items = new List<BookDto>
                {
                    new BookDto { Id = 1, Title = "Test Book", Author = "Test Author" }
                },
                Total = 1,
                Page = page,
                PageSize = pageSize
            };

            _mockBookService.Setup(s => s.GetBooksAsync(It.IsAny<BookSearchRequest>(), "Librarian"))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetBooks(search, category, author, page, pageSize);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<PagedBooksResponse>(actionResult.Value);
            Assert.Single(returnValue.Items);
            Assert.Equal(1, returnValue.Total);
            _mockBookService.Verify(s => s.GetBooksAsync(It.IsAny<BookSearchRequest>(), "Librarian"), Times.Once);
        }

        [Fact]
        public async Task GetBooks_NoSearchCriteria_ReturnsAllBooks()
        {
            // Arrange
            var expectedResponse = new PagedBooksResponse
            {
                Items = new List<BookDto>
                {
                    new BookDto { Id = 1, Title = "Book 1" },
                    new BookDto { Id = 2, Title = "Book 2" }
                },
                Total = 2,
                Page = 1,
                PageSize = 10
            };

            _mockBookService.Setup(s => s.GetBooksAsync(It.IsAny<BookSearchRequest>(), "Librarian"))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetBooks();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<PagedBooksResponse>(actionResult.Value);
            Assert.Equal(2, returnValue.Items.Count);
            Assert.Equal(2, returnValue.Total);
        }

        [Fact]
        public async Task GetBooks_MemberRole_FiltersActiveBooks()
        {
            // Arrange
            SetupUserContext("2", "Member");
            var expectedResponse = new PagedBooksResponse
            {
                Items = new List<BookDto>
                {
                    new BookDto { Id = 1, Title = "Active Book", IsActive = true }
                },
                Total = 1,
                Page = 1,
                PageSize = 10
            };

            _mockBookService.Setup(s => s.GetBooksAsync(It.IsAny<BookSearchRequest>(), "Member"))
                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetBooks();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<PagedBooksResponse>(actionResult.Value);
            Assert.Single(returnValue.Items);
            Assert.True(returnValue.Items.First().IsActive);
            _mockBookService.Verify(s => s.GetBooksAsync(It.IsAny<BookSearchRequest>(), "Member"), Times.Once);
        }

        #endregion

        #region GetBook Tests

        [Fact]
        public async Task GetBook_ExistingId_ReturnsOkResult()
        {
            // Arrange
            var bookId = 1;
            var expectedBook = new BookDto
            {
                Id = bookId,
                Title = "Test Book",
                Author = "Test Author",
                Isbn = "9781234567890",
                Category = "Fiction",
                PublishedYear = 2023,
                IsActive = true
            };

            _mockBookService.Setup(s => s.GetBookByIdAsync(bookId))
                           .ReturnsAsync(expectedBook);

            // Act
            var result = await _controller.GetBook(bookId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<BookDto>(actionResult.Value);
            Assert.Equal(bookId, returnValue.Id);
            Assert.Equal("Test Book", returnValue.Title);
            _mockBookService.Verify(s => s.GetBookByIdAsync(bookId), Times.Once);
        }

        [Fact]
        public async Task GetBook_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var bookId = 999;
            _mockBookService.Setup(s => s.GetBookByIdAsync(bookId))
                           .ReturnsAsync((BookDto?)null);

            // Act
            var result = await _controller.GetBook(bookId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
            _mockBookService.Verify(s => s.GetBookByIdAsync(bookId), Times.Once);
        }

        #endregion

        #region DeleteBook Tests

        [Fact]
        public async Task DeleteBook_ExistingId_ReturnsNoContent()
        {
            // Arrange
            var bookId = 1;
            _mockBookService.Setup(s => s.DeleteBookAsync(bookId, 1))
                           .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteBook(bookId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockBookService.Verify(s => s.DeleteBookAsync(bookId, 1), Times.Once);
        }

        [Fact]
        public async Task DeleteBook_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var bookId = 999;
            _mockBookService.Setup(s => s.DeleteBookAsync(bookId, 1))
                           .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteBook(bookId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockBookService.Verify(s => s.DeleteBookAsync(bookId, 1), Times.Once);
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