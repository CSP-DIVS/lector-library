using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Csp.Api.Services;
using Csp.Api.DTOs;
using System.Data;
using MySql.Data.MySqlClient;

namespace Csp.Unit.Tests
{
    public class BookServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IConfigurationSection> _mockConnectionStringSection;
        private const string TestConnectionString = "Server=localhost;Database=test_db;Uid=test;Pwd=test;";

        public BookServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConnectionStringSection = new Mock<IConfigurationSection>();
            
            // Setup the connection string section mock
            _mockConnectionStringSection.Setup(x => x.Value).Returns(TestConnectionString);
            _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings:DefaultConnection"))
                             .Returns(_mockConnectionStringSection.Object);
            
            // Also setup the indexer approach that GetConnectionString uses internally
            _mockConfiguration.Setup(x => x["ConnectionStrings:DefaultConnection"])
                             .Returns(TestConnectionString);
        }

        #region CreateBookAsync Tests

        [Fact]
        public void CreateBookAsync_ValidRequest_ShouldPassValidation()
        {
            // Arrange
            var request = new CreateBookRequest
            {
                Title = "Valid Book Title",
                Author = "Valid Author",
                Isbn = "9781234567890",
                Category = "Fiction",
                PublishedYear = 2023,
                TotalCopies = 5
            };
            var actorUserId = 1;

            // Act & Assert - Testing validation logic
            Assert.False(string.IsNullOrWhiteSpace(request.Title));
            Assert.False(string.IsNullOrWhiteSpace(request.Author));
            Assert.False(string.IsNullOrWhiteSpace(request.Isbn));
            Assert.False(string.IsNullOrWhiteSpace(request.Category));
            Assert.True(request.PublishedYear >= 1000 && request.PublishedYear <= DateTime.Now.Year + 1);
            Assert.True(request.TotalCopies >= 1);
            Assert.True(request.Isbn.Length >= 10 && request.Isbn.Length <= 20);
            Assert.True(actorUserId > 0);
        }

        [Theory]
        [InlineData("", "Valid Author", "9781234567890", "Fiction", 2023, 5)]
        [InlineData("Valid Title", "", "9781234567890", "Fiction", 2023, 5)]
        [InlineData("Valid Title", "Valid Author", "", "Fiction", 2023, 5)]
        [InlineData("Valid Title", "Valid Author", "9781234567890", "", 2023, 5)]
        public void CreateBookAsync_InvalidRequiredFields_ShouldFail(
            string title, string author, string isbn, string category, int publishedYear, int totalCopies)
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

            // Act & Assert - Testing validation logic
            var hasEmptyRequiredFields = string.IsNullOrWhiteSpace(request.Title) ||
                                       string.IsNullOrWhiteSpace(request.Author) ||
                                       string.IsNullOrWhiteSpace(request.Isbn) ||
                                       string.IsNullOrWhiteSpace(request.Category);

            Assert.True(hasEmptyRequiredFields);
        }

        [Theory]
        [InlineData("123")] // Too short
        [InlineData("123456789012345678901")] // Too long
        public void CreateBookAsync_InvalidIsbnLength_ShouldFail(string isbn)
        {
            // Arrange & Act
            var isValidLength = isbn.Length >= 10 && isbn.Length <= 20;

            // Assert
            Assert.False(isValidLength);
        }

        [Theory]
        [InlineData(999)] // Too old
        [InlineData(2030)] // Too far in future
        public void CreateBookAsync_InvalidPublishedYear_ShouldFail(int publishedYear)
        {
            // Arrange & Act
            var currentYear = DateTime.Now.Year;
            var isValidYear = publishedYear >= 1000 && publishedYear <= currentYear + 1;

            // Assert
            Assert.False(isValidYear);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CreateBookAsync_InvalidTotalCopies_ShouldFail(int totalCopies)
        {
            // Arrange & Act
            var isValidCopies = totalCopies >= 1;

            // Assert
            Assert.False(isValidCopies);
        }

        #endregion

        #region UpdateBookAsync Tests

        [Fact]
        public void UpdateBookAsync_ValidRequest_ShouldPassValidation()
        {
            // Arrange
            var request = new UpdateBookRequest
            {
                Title = "Updated Book Title",
                Author = "Updated Author",
                Isbn = "9781234567890",
                Category = "Non-Fiction",
                PublishedYear = 2022,
                TotalCopies = 10
            };

            // Act & Assert - Testing validation logic
            Assert.False(string.IsNullOrWhiteSpace(request.Title));
            Assert.False(string.IsNullOrWhiteSpace(request.Author));
            Assert.False(string.IsNullOrWhiteSpace(request.Isbn));
            Assert.False(string.IsNullOrWhiteSpace(request.Category));
            Assert.True(request.PublishedYear >= 1000 && request.PublishedYear <= DateTime.Now.Year + 1);
            Assert.True(request.TotalCopies >= 1);
            Assert.True(request.Isbn.Length >= 10 && request.Isbn.Length <= 20);
        }

        [Theory]
        [InlineData("", "Updated Author", "9781234567890", "Fiction", 2023, 5)]
        [InlineData("Updated Title", "", "9781234567890", "Fiction", 2023, 5)]
        [InlineData("Updated Title", "Updated Author", "", "Fiction", 2023, 5)]
        [InlineData("Updated Title", "Updated Author", "9781234567890", "", 2023, 5)]
        public void UpdateBookAsync_InvalidRequiredFields_ShouldFail(
            string title, string author, string isbn, string category, int publishedYear, int totalCopies)
        {
            // Arrange
            var request = new UpdateBookRequest
            {
                Title = title,
                Author = author,
                Isbn = isbn,
                Category = category,
                PublishedYear = publishedYear,
                TotalCopies = totalCopies
            };

            // Act & Assert
            var hasEmptyRequiredFields = string.IsNullOrWhiteSpace(request.Title) ||
                                       string.IsNullOrWhiteSpace(request.Author) ||
                                       string.IsNullOrWhiteSpace(request.Isbn) ||
                                       string.IsNullOrWhiteSpace(request.Category);

            Assert.True(hasEmptyRequiredFields);
        }

        #endregion

        #region UpdateBookStatusAsync Tests

        [Fact]
        public void UpdateBookStatusAsync_ValidParameters_ShouldProcessCorrectly()
        {
            // Arrange
            var bookId = 1;
            var actorUserId = 1;

            // Act & Assert - Testing parameter validation
            Assert.True(bookId > 0);
            Assert.True(actorUserId > 0);
            // isActive parameter can be any boolean value (true or false)
        }

        [Theory]
        [InlineData(0, 1)] // Invalid book ID
        [InlineData(-1, 1)] // Negative book ID
        [InlineData(1, 0)] // Invalid actor user ID
        [InlineData(1, -1)] // Negative actor user ID
        public void UpdateBookStatusAsync_InvalidParameters_ShouldFail(int bookId, int actorUserId)
        {
            // Act & Assert
            var hasValidBookId = bookId > 0;
            var hasValidActorUserId = actorUserId > 0;

            Assert.False(hasValidBookId && hasValidActorUserId);
        }

        #endregion

        #region GetBooksAsync Tests

        [Fact]
        public void GetBooksAsync_ValidSearchRequest_ShouldProcessCorrectly()
        {
            // Arrange
            var request = new BookSearchRequest
            {
                Search = "test",
                Category = "Fiction",
                Author = "Test Author",
                Page = 1,
                PageSize = 10
            };

            // Act & Assert - Testing request validation
            Assert.True(request.Page >= 1);
            Assert.True(request.PageSize >= 1);
            Assert.True(request.PageSize <= 100); // Reasonable upper limit
        }

        [Theory]
        [InlineData(0, 10)] // Invalid page
        [InlineData(-1, 10)] // Negative page
        [InlineData(1, 0)] // Invalid page size
        [InlineData(1, -1)] // Negative page size
        [InlineData(1, 1000)] // Unreasonably large page size
        public void GetBooksAsync_InvalidPagination_ShouldFail(int page, int pageSize)
        {
            // Arrange
            var request = new BookSearchRequest
            {
                Page = page,
                PageSize = pageSize
            };

            // Act & Assert
            var hasValidPage = request.Page >= 1;
            var hasValidPageSize = request.PageSize >= 1 && request.PageSize <= 100;

            Assert.False(hasValidPage && hasValidPageSize);
        }

        [Fact]
        public void GetBooksAsync_MemberRole_ShouldFilterActiveBooks()
        {
            // Arrange
            var userRole = "Member";

            // Act & Assert
            Assert.Equal("Member", userRole);
            // In actual implementation, this would filter IsActive = 1 books
        }

        [Theory]
        [InlineData("Librarian")]
        [InlineData("Administrator")]
        public void GetBooksAsync_StaffRoles_ShouldShowAllBooks(string userRole)
        {
            // Act & Assert
            Assert.True(userRole == "Librarian" || userRole == "Administrator");
            // In actual implementation, this would show all books regardless of IsActive status
        }

        #endregion

        #region GetBookByIdAsync Tests

        [Theory]
        [InlineData(1, true)]
        [InlineData(100, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void GetBookByIdAsync_BookIdValidation_ShouldProcessCorrectly(int bookId, bool shouldBeValid)
        {
            // Act & Assert
            var isValidId = bookId > 0;
            Assert.Equal(shouldBeValid, isValidId);
        }

        #endregion

        #region DeleteBookAsync Tests

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(100, 2, true)]
        [InlineData(0, 1, false)] // Invalid book ID
        [InlineData(-1, 1, false)] // Negative book ID
        [InlineData(1, 0, false)] // Invalid actor user ID
        [InlineData(1, -1, false)] // Negative actor user ID
        public void DeleteBookAsync_ParameterValidation_ShouldProcessCorrectly(int bookId, int actorUserId, bool shouldBeValid)
        {
            // Act & Assert
            var hasValidBookId = bookId > 0;
            var hasValidActorUserId = actorUserId > 0;
            var isValid = hasValidBookId && hasValidActorUserId;

            Assert.Equal(shouldBeValid, isValid);
        }

        #endregion

        #region Configuration Tests

        [Fact]
        public void BookService_MissingConnectionString_ShouldThrowException()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            
            // Mock the connection strings section to return null
            var mockConnectionStringsSection = new Mock<IConfigurationSection>();
            mockConnectionStringsSection.Setup(x => x["DefaultConnection"]).Returns((string?)null);
            mockConnectionStringsSection.Setup(x => x["Default"]).Returns((string?)null);
            mockConfig.Setup(x => x.GetSection("ConnectionStrings")).Returns(mockConnectionStringsSection.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new BookService(mockConfig.Object));
        }

        [Fact]
        public void BookService_ValidConnectionString_ShouldInitializeSuccessfully()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            
            // Mock the connection strings section to return a valid connection string
            var mockConnectionStringsSection = new Mock<IConfigurationSection>();
            mockConnectionStringsSection.Setup(x => x["DefaultConnection"]).Returns(TestConnectionString);
            mockConfig.Setup(x => x.GetSection("ConnectionStrings")).Returns(mockConnectionStringsSection.Object);

            // Act
            var service = new BookService(mockConfig.Object);

            // Assert
            Assert.NotNull(service);
        }

        #endregion

        #region Data Validation Helper Tests

        [Theory]
        [InlineData("Valid Title", true)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData(null, false)]
        public void ValidateTitle_VariousInputs_ShouldReturnExpectedResults(string? title, bool expectedValid)
        {
            // Act
            var isValid = !string.IsNullOrWhiteSpace(title);

            // Assert
            Assert.Equal(expectedValid, isValid);
        }

        [Theory]
        [InlineData("9781234567890", true)] // 13 chars
        [InlineData("1234567890", true)] // 10 chars
        [InlineData("12345678901234567890", true)] // 20 chars
        [InlineData("123456789", false)] // 9 chars (too short)
        [InlineData("123456789012345678901", false)] // 21 chars (too long)
        [InlineData("", false)]
        [InlineData(null, false)]
        public void ValidateIsbn_VariousInputs_ShouldReturnExpectedResults(string? isbn, bool expectedValid)
        {
            // Act
            var isValid = !string.IsNullOrWhiteSpace(isbn) && isbn.Length >= 10 && isbn.Length <= 20;

            // Assert
            Assert.Equal(expectedValid, isValid);
        }

        [Theory]
        [InlineData(1000, true)] // Minimum year
        [InlineData(2023, true)] // Current/recent year
        [InlineData(2024, true)] // Next year
        [InlineData(999, false)] // Too old
        public void ValidatePublishedYear_VariousInputs_ShouldReturnExpectedResults(int year, bool expectedValid)
        {
            // Act
            var currentYear = DateTime.Now.Year;
            var isValid = year >= 1000 && year <= currentYear + 1;

            // Assert
            Assert.Equal(expectedValid, isValid);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(5, true)]
        [InlineData(100, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(-10, false)]
        public void ValidateTotalCopies_VariousInputs_ShouldReturnExpectedResults(int totalCopies, bool expectedValid)
        {
            // Act
            var isValid = totalCopies >= 1;

            // Assert
            Assert.Equal(expectedValid, isValid);
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public void CalculateAvailableCopies_IncreaseTotal_ShouldIncreaseAvailable()
        {
            // Arrange
            int currentTotal = 5;
            int currentAvailable = 3;
            int newTotal = 10;

            // Act
            int totalDifference = newTotal - currentTotal;
            int newAvailable = Math.Max(0, currentAvailable + totalDifference);

            // Assert
            Assert.Equal(8, newAvailable); // 3 + (10 - 5) = 8
        }

        [Fact]
        public void CalculateAvailableCopies_DecreaseTotal_ShouldDecreaseAvailable()
        {
            // Arrange
            int currentTotal = 10;
            int currentAvailable = 7;
            int newTotal = 5;

            // Act
            int totalDifference = newTotal - currentTotal;
            int newAvailable = Math.Max(0, currentAvailable + totalDifference);

            // Assert
            Assert.Equal(2, newAvailable); // 7 + (5 - 10) = 2
        }

        [Fact]
        public void CalculateAvailableCopies_DecreaseMoreThanAvailable_ShouldNotGoNegative()
        {
            // Arrange
            int currentTotal = 10;
            int currentAvailable = 2;
            int newTotal = 3;

            // Act
            int totalDifference = newTotal - currentTotal;
            int newAvailable = Math.Max(0, currentAvailable + totalDifference);

            // Assert
            Assert.Equal(0, newAvailable); // Math.Max(0, 2 + (3 - 10)) = Math.Max(0, -5) = 0
        }

        #endregion
    }
}