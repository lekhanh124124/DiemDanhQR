// File: DTOs/Responses/StudentResponses.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class CreateStudentResponse
    {
        public string? MaSinhVien { get; set; }
        public string? MaNguoiDung { get; set; }
        public string? TenDangNhap { get; set; }
        public string? HoTen { get; set; }
        public int? MaQuyen { get; set; }
        public bool? TrangThaiUser { get; set; }

        public string? LopHanhChinh { get; set; }
        public int? NamNhapHoc { get; set; }
        public string? Khoa { get; set; }
        public string? Nganh { get; set; }

    }
    public class StudentInfoResponse
    {
        public string? MaNguoiDung { get; set; }
        public string? HoTen { get; set; }
        public byte? GioiTinh { get; set; }
        public string? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? NgaySinh { get; set; }
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? DiaChi { get; set; }
        public bool? TrangThai { get; set; }

        // Thông tin sinh viên
        public string? MaSinhVien { get; set; }
        public string? LopHanhChinh { get; set; }
        public int? NamNhapHoc { get; set; }
        public string? Khoa { get; set; }
        public string? Nganh { get; set; }
    }
    public class StudentListItemResponse
    {
        public string? MaSinhVien { get; set; }
        public string? HoTen { get; set; }
        public string? LopHanhChinh { get; set; }
        public int? NamNhapHoc { get; set; }
        public string? Khoa { get; set; }
        public string? Nganh { get; set; }
    }

    public class UpdateStudentResponse
    {
        public string? MaNguoiDung { get; set; }
        public string? MaSinhVien { get; set; }
        public string? TenDangNhap { get; set; }
        public string? HoTen { get; set; }
        public bool TrangThai { get; set; }
        public int? MaQuyen { get; set; }

        public string? LopHanhChinh { get; set; }
        public int? NamNhapHoc { get; set; }
        public string? Khoa { get; set; }
        public string? Nganh { get; set; }

        public byte? GioiTinh { get; set; }
        public string? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? NgaySinh { get; set; }   // dd-MM-yyyy
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? DiaChi { get; set; }
    }
}
