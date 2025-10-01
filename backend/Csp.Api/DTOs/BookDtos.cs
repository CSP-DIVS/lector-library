using System.ComponentModel.DataAnnotations;

namespace Csp.Api.DTOs
{
    /// <summary>
    /// Represents a book in the library.
    /// </summary>
    public class BookDto
    {
        /// <summary>
        /// The unique identifier for the book.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The title of the book.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The author of the book.
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// The International Standard Book Number (ISBN) of the book.
        /// </summary>
        public string Isbn { get; set; } = string.Empty;

        /// <summary>
        /// The category or genre of the book.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// The year the book was published.
        /// </summary>
        public int PublishedYear { get; set; }

        /// <summary>
        /// Indicates whether the book is active and available for circulation.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The total number of copies of this book in the library.
        /// </summary>
        public int TotalCopies { get; set; }

        /// <summary>
        /// The number of copies currently available for loan.
        /// </summary>
        public int AvailableCopies { get; set; }

        /// <summary>
        /// The calculated status of the book (e.g., "Available", "Checked Out").
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when the book was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The timestamp when the book was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents the request to create a new book.
    /// </summary>
    public class CreateBookRequest
    {
        /// <summary>
        /// The title of the book.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The author of the book.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Author cannot be longer than 100 characters.")]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// The International Standard Book Number (ISBN) of the book.
        /// </summary>
        [Required]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters.")]
        public string Isbn { get; set; } = string.Empty;

        /// <summary>
        /// The category or genre of the book.
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "Category cannot be longer than 50 characters.")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// The year the book was published.
        /// </summary>
        [Range(1000, 9999, ErrorMessage = "Please enter a valid year.")]
        public int PublishedYear { get; set; }

        /// <summary>
        /// The total number of copies of this book.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Total copies must be at least 1.")]
        public int TotalCopies { get; set; } = 1;
    }

    /// <summary>
    /// Represents the request to update an existing book.
    /// </summary>
    public class UpdateBookRequest
    {
        /// <summary>
        /// The title of the book.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The author of the book.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Author cannot be longer than 100 characters.")]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// The International Standard Book Number (ISBN) of the book.
        /// </summary>
        [Required]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters.")]
        public string Isbn { get; set; } = string.Empty;

        /// <summary>
        /// The category or genre of the book.
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "Category cannot be longer than 50 characters.")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// The year the book was published.
        /// </summary>
        [Range(1000, 9999, ErrorMessage = "Please enter a valid year.")]
        public int PublishedYear { get; set; }

        /// <summary>
        /// The total number of copies of this book.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Total copies cannot be negative.")]
        public int TotalCopies { get; set; }
    }

    /// <summary>
    /// Represents the request to update a book's active status.
    /// </summary>
    public class UpdateBookStatusRequest
    {
        /// <summary>
        /// The desired status of the book.
        /// `true` for active, `false` for inactive.
        /// </summary>
        [Required]
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Represents the search criteria for books.
    /// </summary>
    public class BookSearchRequest
    {
        /// <summary>
        /// A search term to filter books by title, author, or ISBN.
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// The category to filter books by.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// The author to filter books by.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// The page number for pagination.
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// The number of items per page for pagination.
        /// </summary>
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Represents a paginated list of books.
    /// </summary>
    public class PagedBooksResponse
    {
        /// <summary>
        /// The list of books on the current page.
        /// </summary>
        public List<BookDto> Items { get; set; } = new();

        /// <summary>
        /// The total number of books matching the search criteria.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// The current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The number of items per page.
        /// </summary>
        public int PageSize { get; set; }
    }

    /// <summary>
    /// A generic response for book-related operations.
    /// </summary>
    public class BookResponse
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// A message providing details about the operation's result.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The book data, if applicable.
        /// </summary>
        public BookDto? Book { get; set; }
    }
}