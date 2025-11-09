// File: DTOs/ApiResponse.cs
namespace api.DTOs
{
    public class ApiResponse<T>
    {
        public string Status { get; set; } = "200";
        public string Message { get; set; } = string.Empty;
        public required T Data { get; set; }
    }

    public class PagedResult<T>
    {
        public string Page { get; set; } = "1";
        public string PageSize { get; set; } = "0";
        public string TotalRecords { get; set; } = "0";
        public string TotalPages { get; set; } = "0";
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    }
}