// File: DTOs/Responses/ApiResponse.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class ApiResponse<T>
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public required T Data { get; set; }
    }

    public class PagedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }  
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    }
}