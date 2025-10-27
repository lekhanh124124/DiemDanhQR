// File: DTOs/Requests/UserRequest.cs
namespace DiemDanhQR_API.DTOs.Requests
{
    public class CreateUserRequest
    {
        public required string MaNguoiDung { get; set; }
        public required int MaQuyen { get; set; }
    }

    public class GetUserInfoRequest
    {
        // Truyền 1 trong 2 (ưu tiên MaNguoiDung nếu đều có)
        public string? MaNguoiDung { get; set; }
        public string? TenDangNhap { get; set; }
    }
    public class UserActivityListRequest
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        
        // Sorting
        public string? SortBy { get; set; } = "ThoiGian";
        public string? SortDir { get; set; } = "DESC";

        // Filters
        public string? Keyword { get; set; }     
        public string? MaNguoiDung { get; set; }  
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public bool? AllUsers { get; set; } = false;
    }
}
