// File: DTOs/Requests/LecturerRequests.cs
namespace DiemDanhQR_API.DTOs.Requests
{
    public class CreateLecturerRequest
    {
        public string? MaGiangVien { get; set; }
        public int? MaQuyen { get; set; }
        public string? HoTen { get; set; }
        public string? Khoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public DateTime? NgayTuyenDung { get; set; }

        // Tuỳ chọn hồ sơ người dùng
        public byte? GioiTinh { get; set; }
        public IFormFile? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? DiaChi { get; set; }
    }

    public class GetLecturersRequest
    {
        // Paging
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 20;

        // Filters
        public string? Khoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public DateTime? NgayTuyenDungFrom { get; set; }
        public DateTime? NgayTuyenDungTo { get; set; }
        public bool? TrangThaiUser { get; set; }

        // Sorting
        public string? SortBy { get; set; } = "HoTen";
        public string? SortDir { get; set; } = "ASC"; // ASC | DESC
    }

    public class UpdateLecturerRequest
    {
        // Bắt buộc: dùng Mã giảng viên để xác định hồ sơ
        public string? MaGiangVien { get; set; }

        // Thông tin user/giảng viên có thể cập nhật
        public string? TenGiangVien { get; set; }
        public bool? TrangThai { get; set; }
        public int? MaQuyen { get; set; }

        public string? Khoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public DateTime? NgayTuyenDung { get; set; }

        public byte? GioiTinh { get; set; }
        public IFormFile? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? DiaChi { get; set; }
    }
}
