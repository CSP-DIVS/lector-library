namespace Csp.Api.DTOs
{
    public class CreateMemberRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateMemberRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class PagedMembersResponse
    {
        public IEnumerable<UserDto> Items { get; set; } = Enumerable.Empty<UserDto>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}


