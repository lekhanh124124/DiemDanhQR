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
    
}
