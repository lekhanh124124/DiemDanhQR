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

    public class GetStudentsRequest
    {
        // Paging (mặc định: trang 1, 20 bản ghi)
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Tìm kiếm mờ theo MãSV / Họ tên / Email / SĐT
        public string? Keyword { get; set; }

        // Bộ lọc
        public string? Khoa { get; set; }
        public string? Nganh { get; set; }
        public int? NamNhapHoc { get; set; }
        public bool? TrangThaiUser { get; set; } // filter theo NguoiDung.TrangThai

        public string? MaLopHocPhan { get; set; }

        public string? SortBy { get; set; } = "HoTen";
        public string? SortDir { get; set; } = "ASC"; // ASC | DESC
    }
}
