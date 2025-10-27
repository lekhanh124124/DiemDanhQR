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

    public class GetLecturersRequest
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Keyword
        public string? Keyword { get; set; }

        // Filters
        public string? Khoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public DateTime? NgayTuyenDungFrom { get; set; }
        public DateTime? NgayTuyenDungTo { get; set; }
        public bool? TrangThaiUser { get; set; }

        // Sorting
        // Allowed: MaGiangVien | HoTen | Khoa | HocHam | HocVi | NgayTuyenDung
        public string? SortBy { get; set; } = "HoTen";
        public string? SortDir { get; set; } = "ASC"; // ASC | DESC
    }
}
