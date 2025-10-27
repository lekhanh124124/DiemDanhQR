// File: DTOs/Responses/LecturerResponses.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class CreateLecturerResponse
    {
        public string? MaGiangVien { get; set; }
        public string? MaNguoiDung { get; set; }
        public string? TenDangNhap { get; set; }
        public string? HoTen { get; set; }
        public int? MaQuyen { get; set; }
        public string? Khoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public DateTime? NgayTuyenDung { get; set; }
        public bool TrangThaiUser { get; set; }
    }

    public class LecturerInfoResponse
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
        public bool TrangThai { get; set; }
        public string? MaGiangVien { get; set; }
        public string? Khoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public DateTime? NgayTuyenDung { get; set; }
    }
    public class LecturerListItemResponse
    {
        public string? MaGiangVien { get; set; }
        public string? HoTen { get; set; }
        public string? Khoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public DateTime? NgayTuyenDung { get; set; }
    }

    public class UpdateLecturerResponse
    {
        public string? MaNguoiDung { get; set; }
        public string? MaGiangVien { get; set; }
        public string? TenDangNhap { get; set; }
        public string? HoTen { get; set; }
        public bool TrangThai { get; set; }
        public int? MaQuyen { get; set; }

        public string? Khoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public string? NgayTuyenDung { get; set; }

        public byte? GioiTinh { get; set; }
        public string? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }

        // Trả về chỉ ngày
        public string? NgaySinh { get; set; }

        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? DiaChi { get; set; }
    }
}
