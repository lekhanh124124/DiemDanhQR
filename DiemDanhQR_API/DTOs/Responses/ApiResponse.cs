// File: DTOs/Responses/ApiResponse.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class ApiResponse<T>
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;       
        public required T Data { get; set; }
    }
}