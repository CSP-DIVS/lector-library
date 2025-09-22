namespace Csp.Api.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int PublishedYear { get; set; }
        public bool IsActive { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateBookRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int PublishedYear { get; set; }
        public int TotalCopies { get; set; } = 1;
    }

    public class UpdateBookRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int PublishedYear { get; set; }
        public int TotalCopies { get; set; }
    }

    public class BookSearchRequest
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Author { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedBooksResponse
    {
        public List<BookDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class BookResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public BookDto? Book { get; set; }
    }
}
