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

    public class UpdateUserProfileRequest
    {
        public byte? GioiTinh { get; set; }
        public string? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? DiaChi { get; set; }
    }
}
