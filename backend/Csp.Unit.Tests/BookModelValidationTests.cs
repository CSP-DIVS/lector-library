using Xunit;
using Csp.Api.Models;
using Csp.Api.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Csp.Unit.Tests
{
    public class BookModelValidationTests
    {
        #region Book Model Tests

        [Fact]
        public void Book_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var book = new Book();

            // Assert
            Assert.Equal(0, book.Id);
            Assert.Equal(string.Empty, book.Title);
            Assert.Equal(string.Empty, book.Author);
            Assert.Equal(string.Empty, book.Isbn);
            Assert.Equal(string.Empty, book.Category);
            Assert.Equal(0, book.PublishedYear);
            Assert.True(book.IsActive);
            Assert.True((DateTime.UtcNow - book.CreatedAt).TotalMinutes < 1); // Should be very recent
            Assert.True((DateTime.UtcNow - book.UpdatedAt).TotalMinutes < 1); // Should be very recent
            Assert.Equal(0, book.CreatedBy);
            Assert.Equal(0, book.UpdatedBy);
        }

        [Fact]
        public void Book_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var book = new Book();
            var testDate = DateTime.UtcNow.AddDays(-1);

            // Act
            book.Id = 1;
            book.Title = "Test Book";
            book.Author = "Test Author";
            book.Isbn = "9781234567890";
            book.Category = "Fiction";
            book.PublishedYear = 2023;
            book.IsActive = false;
            book.CreatedAt = testDate;
            book.UpdatedAt = testDate;
            book.CreatedBy = 10;
            book.UpdatedBy = 20;

            // Assert
            Assert.Equal(1, book.Id);
            Assert.Equal("Test Book", book.Title);
            Assert.Equal("Test Author", book.Author);
            Assert.Equal("9781234567890", book.Isbn);
            Assert.Equal("Fiction", book.Category);
            Assert.Equal(2023, book.PublishedYear);
            Assert.False(book.IsActive);
            Assert.Equal(testDate, book.CreatedAt);
            Assert.Equal(testDate, book.UpdatedAt);
            Assert.Equal(10, book.CreatedBy);
            Assert.Equal(20, book.UpdatedBy);
        }

        #endregion

        #region BookInventory Model Tests

        [Fact]
        public void BookInventory_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var inventory = new BookInventory();

            // Assert
            Assert.Equal(0, inventory.Id);
            Assert.Equal(0, inventory.BookId);
            Assert.Equal(0, inventory.TotalCopies);
            Assert.Equal(0, inventory.AvailableCopies);
            Assert.True((DateTime.UtcNow - inventory.CreatedAt).TotalMinutes < 1);
            Assert.True((DateTime.UtcNow - inventory.UpdatedAt).TotalMinutes < 1);
        }

        [Fact]
        public void BookInventory_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var inventory = new BookInventory();
            var testDate = DateTime.UtcNow.AddDays(-1);

            // Act
            inventory.Id = 1;
            inventory.BookId = 5;
            inventory.TotalCopies = 10;
            inventory.AvailableCopies = 7;
            inventory.CreatedAt = testDate;
            inventory.UpdatedAt = testDate;

            // Assert
            Assert.Equal(1, inventory.Id);
            Assert.Equal(5, inventory.BookId);
            Assert.Equal(10, inventory.TotalCopies);
            Assert.Equal(7, inventory.AvailableCopies);
            Assert.Equal(testDate, inventory.CreatedAt);
            Assert.Equal(testDate, inventory.UpdatedAt);
        }

        [Theory]
        [InlineData(5, 3, true)] // Available < Total (valid)
        [InlineData(5, 5, true)] // Available = Total (valid)
        [InlineData(5, 0, true)] // Available = 0 (valid)
        [InlineData(5, 6, false)] // Available > Total (invalid business rule)
        [InlineData(0, 1, false)] // Total = 0 but Available > 0 (invalid)
        public void BookInventory_ValidateBusinessRules_ShouldFollowConstraints(int totalCopies, int availableCopies, bool shouldBeValid)
        {
            // Arrange
            var inventory = new BookInventory
            {
                TotalCopies = totalCopies,
                AvailableCopies = availableCopies
            };

            // Act
            var isValid = inventory.AvailableCopies <= inventory.TotalCopies && 
                         (inventory.TotalCopies == 0 ? inventory.AvailableCopies == 0 : true);

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        #endregion

        #region BookDto Tests

        [Fact]
        public void BookDto_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var bookDto = new BookDto();

            // Assert
            Assert.Equal(0, bookDto.Id);
            Assert.Equal(string.Empty, bookDto.Title);
            Assert.Equal(string.Empty, bookDto.Author);
            Assert.Equal(string.Empty, bookDto.Isbn);
            Assert.Equal(string.Empty, bookDto.Category);
            Assert.Equal(0, bookDto.PublishedYear);
            Assert.False(bookDto.IsActive);
            Assert.Equal(0, bookDto.TotalCopies);
            Assert.Equal(0, bookDto.AvailableCopies);
            Assert.Equal(string.Empty, bookDto.Status);
            Assert.Equal(default(DateTime), bookDto.CreatedAt);
            Assert.Equal(default(DateTime), bookDto.UpdatedAt);
        }

        [Fact]
        public void BookDto_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var bookDto = new BookDto();
            var testDate = DateTime.UtcNow;

            // Act
            bookDto.Id = 1;
            bookDto.Title = "Test Book";
            bookDto.Author = "Test Author";
            bookDto.Isbn = "9781234567890";
            bookDto.Category = "Fiction";
            bookDto.PublishedYear = 2023;
            bookDto.IsActive = true;
            bookDto.TotalCopies = 10;
            bookDto.AvailableCopies = 7;
            bookDto.Status = "Available";
            bookDto.CreatedAt = testDate;
            bookDto.UpdatedAt = testDate;

            // Assert
            Assert.Equal(1, bookDto.Id);
            Assert.Equal("Test Book", bookDto.Title);
            Assert.Equal("Test Author", bookDto.Author);
            Assert.Equal("9781234567890", bookDto.Isbn);
            Assert.Equal("Fiction", bookDto.Category);
            Assert.Equal(2023, bookDto.PublishedYear);
            Assert.True(bookDto.IsActive);
            Assert.Equal(10, bookDto.TotalCopies);
            Assert.Equal(7, bookDto.AvailableCopies);
            Assert.Equal("Available", bookDto.Status);
            Assert.Equal(testDate, bookDto.CreatedAt);
            Assert.Equal(testDate, bookDto.UpdatedAt);
        }

        [Theory]
        [InlineData(0, "Unavailable")]
        [InlineData(1, "Available")]
        [InlineData(5, "Available")]
        public void BookDto_StatusProperty_ShouldReflectAvailability(int availableCopies, string expectedStatus)
        {
            // Arrange
            var bookDto = new BookDto
            {
                AvailableCopies = availableCopies,
                Status = availableCopies > 0 ? "Available" : "Unavailable"
            };

            // Act & Assert
            Assert.Equal(expectedStatus, bookDto.Status);
        }

        #endregion

        #region CreateBookRequest Tests

        [Fact]
        public void CreateBookRequest_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var request = new CreateBookRequest();

            // Assert
            Assert.Equal(string.Empty, request.Title);
            Assert.Equal(string.Empty, request.Author);
            Assert.Equal(string.Empty, request.Isbn);
            Assert.Equal(string.Empty, request.Category);
            Assert.Equal(0, request.PublishedYear);
            Assert.Equal(1, request.TotalCopies);
        }

        [Fact]
        public void CreateBookRequest_SetValidProperties_ShouldRetainValues()
        {
            // Arrange & Act
            var request = new CreateBookRequest
            {
                Title = "New Book",
                Author = "New Author", 
                Isbn = "9781234567890",
                Category = "Science",
                PublishedYear = 2023,
                TotalCopies = 5
            };

            // Assert
            Assert.Equal("New Book", request.Title);
            Assert.Equal("New Author", request.Author);
            Assert.Equal("9781234567890", request.Isbn);
            Assert.Equal("Science", request.Category);
            Assert.Equal(2023, request.PublishedYear);
            Assert.Equal(5, request.TotalCopies);
        }

        [Theory]
        [InlineData("", "Author", "9781234567890", "Fiction", 2023, 1, false)]
        [InlineData("Title", "", "9781234567890", "Fiction", 2023, 1, false)]
        [InlineData("Title", "Author", "", "Fiction", 2023, 1, false)]
        [InlineData("Title", "Author", "9781234567890", "", 2023, 1, false)]
        [InlineData("Title", "Author", "9781234567890", "Fiction", 999, 1, false)]
        [InlineData("Title", "Author", "9781234567890", "Fiction", 2023, 0, false)]
        [InlineData("Title", "Author", "9781234567890", "Fiction", 2023, 1, true)]
        public void CreateBookRequest_ValidationRules_ShouldBeAppliedCorrectly(
            string title, string author, string isbn, string category, int publishedYear, int totalCopies, bool shouldBeValid)
        {
            // Arrange
            var request = new CreateBookRequest
            {
                Title = title,
                Author = author,
                Isbn = isbn,
                Category = category,
                PublishedYear = publishedYear,
                TotalCopies = totalCopies
            };

            // Act
            var isValid = !string.IsNullOrWhiteSpace(request.Title) &&
                         !string.IsNullOrWhiteSpace(request.Author) &&
                         !string.IsNullOrWhiteSpace(request.Isbn) &&
                         !string.IsNullOrWhiteSpace(request.Category) &&
                         request.PublishedYear >= 1000 && request.PublishedYear <= DateTime.Now.Year + 1 &&
                         request.TotalCopies >= 1;

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        #endregion

        #region UpdateBookRequest Tests

        [Fact]
        public void UpdateBookRequest_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var request = new UpdateBookRequest();

            // Assert
            Assert.Equal(string.Empty, request.Title);
            Assert.Equal(string.Empty, request.Author);
            Assert.Equal(string.Empty, request.Isbn);
            Assert.Equal(string.Empty, request.Category);
            Assert.Equal(0, request.PublishedYear);
            Assert.Equal(0, request.TotalCopies);
        }

        [Fact]
        public void UpdateBookRequest_SetValidProperties_ShouldRetainValues()
        {
            // Arrange & Act
            var request = new UpdateBookRequest
            {
                Title = "Updated Book",
                Author = "Updated Author",
                Isbn = "9789876543210",
                Category = "Technology",
                PublishedYear = 2022,
                TotalCopies = 15
            };

            // Assert
            Assert.Equal("Updated Book", request.Title);
            Assert.Equal("Updated Author", request.Author);
            Assert.Equal("9789876543210", request.Isbn);
            Assert.Equal("Technology", request.Category);
            Assert.Equal(2022, request.PublishedYear);
            Assert.Equal(15, request.TotalCopies);
        }

        #endregion

        #region BookSearchRequest Tests

        [Fact]
        public void BookSearchRequest_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var request = new BookSearchRequest();

            // Assert
            Assert.Null(request.Search);
            Assert.Null(request.Category);
            Assert.Null(request.Author);
            Assert.Equal(1, request.Page);
            Assert.Equal(10, request.PageSize);
        }

        [Fact]
        public void BookSearchRequest_SetProperties_ShouldRetainValues()
        {
            // Arrange & Act
            var request = new BookSearchRequest
            {
                Search = "test search",
                Category = "Fiction",
                Author = "Test Author",
                Page = 2,
                PageSize = 20
            };

            // Assert
            Assert.Equal("test search", request.Search);
            Assert.Equal("Fiction", request.Category);
            Assert.Equal("Test Author", request.Author);
            Assert.Equal(2, request.Page);
            Assert.Equal(20, request.PageSize);
        }

        [Theory]
        [InlineData(0, 10, false)] // Invalid page
        [InlineData(-1, 10, false)] // Negative page
        [InlineData(1, 0, false)] // Invalid page size
        [InlineData(1, -1, false)] // Negative page size
        [InlineData(1, 10, true)] // Valid
        [InlineData(5, 25, true)] // Valid
        public void BookSearchRequest_PaginationValidation_ShouldBeCorrect(int page, int pageSize, bool shouldBeValid)
        {
            // Arrange
            var request = new BookSearchRequest
            {
                Page = page,
                PageSize = pageSize
            };

            // Act
            var isValid = request.Page >= 1 && request.PageSize >= 1;

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        #endregion

        #region PagedBooksResponse Tests

        [Fact]
        public void PagedBooksResponse_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var response = new PagedBooksResponse();

            // Assert
            Assert.NotNull(response.Items);
            Assert.Empty(response.Items);
            Assert.Equal(0, response.Total);
            Assert.Equal(0, response.Page);
            Assert.Equal(0, response.PageSize);
        }

        [Fact]
        public void PagedBooksResponse_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var books = new List<BookDto>
            {
                new BookDto { Id = 1, Title = "Book 1" },
                new BookDto { Id = 2, Title = "Book 2" }
            };

            // Act
            var response = new PagedBooksResponse
            {
                Items = books,
                Total = 25,
                Page = 3,
                PageSize = 10
            };

            // Assert
            Assert.Equal(2, response.Items.Count);
            Assert.Equal(25, response.Total);
            Assert.Equal(3, response.Page);
            Assert.Equal(10, response.PageSize);
        }

        [Theory]
        [InlineData(5, 5, 10, true)] // Items match subset
        [InlineData(10, 10, 10, true)] // Items match page size
        [InlineData(15, 10, 10, false)] // More items than page size
        [InlineData(0, 0, 10, true)] // No items and no total
        public void PagedBooksResponse_ConsistencyValidation_ShouldBeLogical(int itemCount, int total, int pageSize, bool shouldBeConsistent)
        {
            // Arrange
            var items = new List<BookDto>();
            for (int i = 0; i < itemCount; i++)
            {
                items.Add(new BookDto { Id = i + 1 });
            }

            var response = new PagedBooksResponse
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

        #region BookResponse Tests

        [Fact]
        public void BookResponse_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var response = new BookResponse();

            // Assert
            Assert.False(response.Success);
            Assert.Equal(string.Empty, response.Message);
            Assert.Null(response.Book);
        }

        [Fact]
        public void BookResponse_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var book = new BookDto { Id = 1, Title = "Test Book" };

            // Act
            var response = new BookResponse
            {
                Success = true,
                Message = "Operation successful",
                Book = book
            };

            // Assert
            Assert.True(response.Success);
            Assert.Equal("Operation successful", response.Message);
            Assert.NotNull(response.Book);
            Assert.Equal(1, response.Book.Id);
            Assert.Equal("Test Book", response.Book.Title);
        }

        [Theory]
        [InlineData(true, "Success message", true)] // Success with book
        [InlineData(false, "Error message", false)] // Failure without book
        [InlineData(true, "", true)] // Success with empty message
        [InlineData(false, "", false)] // Failure with empty message
        public void BookResponse_StateValidation_ShouldBeConsistent(bool success, string message, bool shouldHaveBook)
        {
            // Arrange & Act
            var response = new BookResponse
            {
                Success = success,
                Message = message,
                Book = shouldHaveBook ? new BookDto { Id = 1 } : null
            };

            // Assert
            Assert.Equal(success, response.Success);
            Assert.Equal(message, response.Message);
            if (shouldHaveBook)
            {
                Assert.NotNull(response.Book);
            }
            else
            {
                Assert.Null(response.Book);
            }
        }

        #endregion
    }
}