using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Csp.Api.Services;
using Csp.Api.DTOs;
using System.Security.Claims;

namespace Csp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpPost]
        [Authorize(Policy = "RequireLibrarian")]
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

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<ActionResult<BookResponse>> UpdateBook(int id, [FromBody] UpdateBookRequest request)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _bookService.UpdateBookAsync(id, request, actorUserId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<ActionResult<BookResponse>> UpdateBookStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                var actorUserId = GetCurrentUserId();
                var result = await _bookService.UpdateBookStatusAsync(id, isActive, actorUserId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BookResponse 
                { 
                    Success = false, 
                    Message = $"Controller error: {ex.Message}" 
                });
            }
        }

        [HttpGet]
        public async Task<ActionResult<PagedBooksResponse>> GetBooks(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] string? author = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var request = new BookSearchRequest
            {
                Search = search,
                Category = category,
                Author = author,
                Page = page,
                PageSize = pageSize
            };

            var userRole = GetCurrentUserRole();
            var result = await _bookService.GetBooksAsync(request, userRole);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            
            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireLibrarian")]
        public async Task<ActionResult> DeleteBook(int id)
        {
            var actorUserId = GetCurrentUserId();
            var success = await _bookService.DeleteBookAsync(id, actorUserId);
            
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || userId <= 0)
            {
                throw new UnauthorizedAccessException("Invalid user ID in token");
            }
            return userId;
        }

        private string GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return roleClaim ?? "Member";
        }
    }
}
