namespace Csp.Api.DTOs
{
    /// <summary>
    /// Data transfer object representing a lending transaction with related book and user information.
    /// Used for returning lending details in API responses.
    /// </summary>
    public class LendingDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the lending transaction.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the borrowed book.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the title of the borrowed book.
        /// </summary>
        public string BookTitle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the author of the borrowed book.
        /// </summary>
        public string BookAuthor { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ISBN of the borrowed book.
        /// </summary>
        public string BookIsbn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the user who borrowed the book.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the username of the borrower.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address of the borrower.
        /// </summary>
        public string MemberEmail { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the book was borrowed.
        /// </summary>
        public DateTime BorrowDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the book is due for return.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets the actual return date of the book.
        /// Null if the book has not been returned yet.
        /// </summary>
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Gets or sets the current status of the lending (Active, Returned, Overdue).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the fine amount for overdue returns.
        /// Null if no fine is applicable.
        /// </summary>
        public decimal? FineAmount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the fine has been paid.
        /// </summary>
        public bool FinePaid { get; set; }

        /// <summary>
        /// Gets or sets the number of times this lending has been renewed.
        /// </summary>
        public int RenewalCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of renewals allowed.
        /// </summary>
        public int MaxRenewals { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the book is currently overdue.
        /// </summary>
        public bool IsOverdue { get; set; }

        /// <summary>
        /// Gets or sets the number of days the book is overdue.
        /// Null if the book is not overdue.
        /// </summary>
        public int? OverdueDays { get; set; }
    }

    /// <summary>
    /// Request object for borrowing a book from the library.
    /// </summary>
    public class BorrowBookRequest
    {
        /// <summary>
        /// Gets or sets the identifier of the book to borrow.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user borrowing the book.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the duration of the loan in days.
        /// Defaults to 14 days.
        /// </summary>
        public int LoanDurationDays { get; set; } = 14;
    }

    /// <summary>
    /// Request object for renewing an existing loan.
    /// </summary>
    public class RenewLoanRequest
    {
        /// <summary>
        /// Gets or sets the identifier of the lending transaction to renew.
        /// </summary>
        public int LendingId { get; set; }
    }

    /// <summary>
    /// Request object for returning a borrowed book.
    /// </summary>
    public class ReturnBookRequest
    {
        /// <summary>
        /// Gets or sets the identifier of the lending transaction for the book being returned.
        /// </summary>
        public int LendingId { get; set; }

        /// <summary>
        /// Gets or sets the fine amount to be charged for late return.
        /// Null if no fine is applicable.
        /// </summary>
        public decimal? FineAmount { get; set; }
    }

    /// <summary>
    /// Response object for lending operations.
    /// </summary>
    public class LendingResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a message describing the result of the operation.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the lending details.
        /// Null if the operation failed or does not return lending details.
        /// </summary>
        public LendingDto? Lending { get; set; }
    }

    /// <summary>
    /// Response object containing a paginated list of lending transactions.
    /// </summary>
    public class PagedLendingsResponse
    {
        /// <summary>
        /// Gets or sets the list of lending transactions for the current page.
        /// </summary>
        public List<LendingDto> Items { get; set; } = new();

        /// <summary>
        /// Gets or sets the total number of lending transactions across all pages.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; }
    }

    /// <summary>
    /// Data transfer object representing a book reservation with related information.
    /// Used for returning reservation details in API responses.
    /// </summary>
    public class ReservationDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the reservation.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the reserved book.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the title of the reserved book.
        /// </summary>
        public string BookTitle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the author of the reserved book.
        /// </summary>
        public string BookAuthor { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ISBN of the reserved book.
        /// </summary>
        public string BookIsbn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the user who made the reservation.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the username of the person who made the reservation.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address of the person who made the reservation.
        /// </summary>
        public string MemberEmail { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the reservation was created.
        /// </summary>
        public DateTime ReservedDate { get; set; }

        /// <summary>
        /// Gets or sets the position of this reservation in the queue.
        /// Lower numbers indicate higher priority.
        /// </summary>
        public int QueuePosition { get; set; }

        /// <summary>
        /// Gets or sets the current status of the reservation (Pending, Available, Fulfilled, Cancelled, Expired).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the reserved book became available.
        /// Null if not yet available.
        /// </summary>
        public DateTime? AvailableDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the reservation will expire.
        /// Null if no expiry date is set.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the reservation was fulfilled.
        /// Null if not yet fulfilled.
        /// </summary>
        public DateTime? FulfilledDate { get; set; }

        /// <summary>
        /// Gets or sets an estimated availability message for the reservation.
        /// Null if not applicable.
        /// </summary>
        public string? EstimatedAvailable { get; set; }
    }

    /// <summary>
    /// Request object for creating a new book reservation.
    /// </summary>
    public class CreateReservationRequest
    {
        /// <summary>
        /// Gets or sets the identifier of the book to reserve.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user making the reservation.
        /// </summary>
        public int UserId { get; set; }
    }

    /// <summary>
    /// Request object for fulfilling a book reservation.
    /// </summary>
    public class FulfillReservationRequest
    {
        /// <summary>
        /// Gets or sets the identifier of the reservation to fulfill.
        /// </summary>
        public int ReservationId { get; set; }

        /// <summary>
        /// Gets or sets the duration of the loan in days when the reservation is fulfilled.
        /// Defaults to 14 days.
        /// </summary>
        public int LoanDurationDays { get; set; } = 14;
    }

    /// <summary>
    /// Response object for reservation operations.
    /// </summary>
    public class ReservationResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a message describing the result of the operation.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the reservation details.
        /// Null if the operation failed or does not return reservation details.
        /// </summary>
        public ReservationDto? Reservation { get; set; }
    }

    /// <summary>
    /// Response object containing a paginated list of reservations.
    /// </summary>
    public class PagedReservationsResponse
    {
        /// <summary>
        /// Gets or sets the list of reservations for the current page.
        /// </summary>
        public List<ReservationDto> Items { get; set; } = new();

        /// <summary>
        /// Gets or sets the total number of reservations across all pages.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; }
    }
}
