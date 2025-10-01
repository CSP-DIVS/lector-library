using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Csp.Api.Services;
using Csp.Api.DTOs;
using System.Security.Claims;

namespace Csp.Api.Controllers
{
    /// <summary>
    /// Manages the book catalog, allowing for creation, retrieval, updating, and deletion of books.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// Creates a new book.
        /// </summary>
        /// <remarks>
        /// This endpoint is restricted to users with the 'Librarian' role.
        /// </remarks>
        /// <param name="request">The details of the book to create.</param>
        /// <returns>The newly created book.</returns>
        /// <response code="201">Returns the newly created book.</response>
        /// <response code="400">If the request is invalid or the book already exists.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have the required permissions.</response>
        [HttpPost]
        [Authorize(Policy = "RequireLibrarian")]
        [ProducesResponseType(typeof(BookResponse), 201)]
        [ProducesResponseType(typeof(BookResponse), 400)]
        public async Task<ActionResult<BookResponse>> CreateBook([FromBody] CreateBookRequest request)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _bookService.CreateBookAsync(request, actorUserId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetBook), new { id = result.Book?.Id }, result);
        }

        /// <summary>
        /// Updates an existing book.
        /// </summary>
        /// <remarks>
        /// This endpoint is restricted to users with the 'Librarian' role.
        /// </remarks>
        /// <param name="id">The ID of the book to update.</param>
        /// <param name="request">The updated details of the book.</param>
        /// <returns>The updated book.</returns>
        /// <response code="200">Returns the updated book.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="404">If the book is not found.</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireLibrarian")]
        [ProducesResponseType(typeof(BookResponse), 200)]
        [ProducesResponseType(typeof(BookResponse), 400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BookResponse>> UpdateBook(int id, [FromBody] UpdateBookRequest request)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _bookService.UpdateBookAsync(id, request, actorUserId);
            
            if (!result.Success)
            {
                 // Check for a "not found" message to return the appropriate status code.
                if (result.Message.Contains("not found"))
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates the status of a book (active or inactive).
        /// </summary>
        /// <remarks>
        /// This endpoint is restricted to users with the 'Librarian' role.
        /// </remarks>
        /// <param name="id">The ID of the book to update.</param>
        /// <param name="request">The request containing the new status.</param>
        /// <returns>The book with the updated status.</returns>
        /// <response code="200">Returns the updated book.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="404">If the book is not found.</response>
        [HttpPut("{id}/status")]
        [Authorize(Policy = "RequireLibrarian")]
        [ProducesResponseType(typeof(BookResponse), 200)]
        [ProducesResponseType(typeof(BookResponse), 400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BookResponse>> UpdateBookStatus(int id, [FromBody] UpdateBookStatusRequest request)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _bookService.UpdateBookStatusAsync(id, request.IsActive, actorUserId);
            
            if (!result.Success)
            {
                if (result.Message.Contains("not found"))
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of books.
        /// </summary>
        /// <remarks>
        /// Members will only see active books. Librarians and Administrators can see all books.
        /// </remarks>
        /// <param name="request">The search and pagination criteria.</param>
        /// <returns>A paginated list of books.</returns>
        /// <response code="200">Returns the list of books.</response>
        [HttpGet]
        [AllowAnonymous] // Or specify your default policy
        [ProducesResponseType(typeof(PagedBooksResponse), 200)]
        public async Task<ActionResult<PagedBooksResponse>> GetBooks([FromQuery] BookSearchRequest request)
        {
            var userRole = GetCurrentUserRole();
            var result = await _bookService.GetBooksAsync(request, userRole);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a single book by its ID.
        /// </summary>
        /// <param name="id">The ID of the book to retrieve.</param>
        /// <returns>The requested book.</returns>
        /// <response code="200">Returns the book.</response>
        /// <response code="404">If the book is not found.</response>
        [HttpGet("{id}")]
        [AllowAnonymous] // Or specify your default policy
        [ProducesResponseType(typeof(BookDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <remarks>
        /// This is a permanent action and is restricted to Librarians.
        /// </remarks>
        /// <param name="id">The ID of the book to delete.</param>
        /// <response code="204">If the book was successfully deleted.</response>
        /// <response code="404">If the book was not found.</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireLibrarian")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DeleteBook(int id)
        {
            var actorUserId = GetCurrentUserId();
            var success = await _bookService.DeleteBookAsync(id, actorUserId);
            if (!success) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Retrieves the user ID from the current security context.
        /// </summary>
        /// <returns>The authenticated user's ID.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user ID is missing or invalid.</exception>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId) && userId > 0)
            {
                return userId;
            }
            // This should not happen if the [Authorize] attribute is working correctly.
            throw new UnauthorizedAccessException("User ID is missing or invalid in the token.");
        }

        /// <summary>
        /// Retrieves the role from the current security context.
        /// </summary>
        /// <returns>The user's role, or a default value if not found.</returns>
        private string GetCurrentUserRole()
        {
            // If user is not authenticated, Role claim will be null. Default to "Member" for public queries.
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";
        }
    }
}
