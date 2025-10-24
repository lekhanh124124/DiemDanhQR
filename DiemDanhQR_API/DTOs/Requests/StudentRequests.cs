// File: DTOs/Requests/StudentRequests.cs
namespace DiemDanhQR_API.DTOs.Requests
{
    public class CreateStudentRequest
    {
        // Bắt buộc
        public string MaSinhVien { get; set; } = string.Empty;
        public int MaQuyen { get; set; }

        // Nếu không truyền -> mặc định = MaSinhVien
        public string? MaNguoiDung { get; set; }

        // Hồ sơ người dùng (tuỳ chọn)
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public byte? GioiTinh { get; set; }
        public string? DiaChi { get; set; }

        // Thông tin sinh viên (tuỳ chọn)
        public string? LopHanhChinh { get; set; }
        public int? NamNhapHoc { get; set; }
        public string? Khoa { get; set; }
        public string? Nganh { get; set; }
    }
}
