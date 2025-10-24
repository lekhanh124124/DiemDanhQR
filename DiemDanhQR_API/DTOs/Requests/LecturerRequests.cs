// File: DTOs/Requests/LecturerRequests.cs
namespace DiemDanhQR_API.DTOs.Requests
{
    public class CreateLecturerRequest
    {
        // Mã giảng viên (thường là PK hiển thị)
        public string MaGiangVien { get; set; } = string.Empty;

        // Nếu không truyền, sẽ mặc định = MaGiangVien
        public string? MaNguoiDung { get; set; }

        // Quyền áp cho user được tạo (bắt buộc)
        public int MaQuyen { get; set; }

        // Thông tin hồ sơ/nghiệp vụ
        public string? HoTen { get; set; }
        public string? Khoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public DateTime? NgayTuyenDung { get; set; }

        // Tuỳ chọn hồ sơ người dùng
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public byte? GioiTinh { get; set; }
        public string? DiaChi { get; set; }
    }
}
