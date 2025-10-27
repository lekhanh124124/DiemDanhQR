// File: DTOs/Requests/StudentRequests.cs
namespace DiemDanhQR_API.DTOs.Requests
{
    public class CreateStudentRequest
    {
        // Bắt buộc
        public string MaSinhVien { get; set; } = string.Empty;
        public int MaQuyen { get; set; }
        public string? MaNguoiDung { get; set; }

        // Hồ sơ người dùng 
        public string? HoTen { get; set; }
        public byte? GioiTinh { get; set; }
        public IFormFile? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? DiaChi { get; set; }

        // Thông tin sinh viên (tuỳ chọn)
        public string? LopHanhChinh { get; set; }
        public int? NamNhapHoc { get; set; }
        public string? Khoa { get; set; }
        public string? Nganh { get; set; }
    }

    public class GetStudentsRequest
    {
        // Paging 
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Tìm kiếm 
        public string? Keyword { get; set; }

        // Bộ lọc
        public string? Khoa { get; set; }
        public string? Nganh { get; set; }
        public int? NamNhapHoc { get; set; }
        public bool? TrangThaiUser { get; set; }

        public string? MaLopHocPhan { get; set; }

        public string? SortBy { get; set; } = "HoTen";
        public string? SortDir { get; set; } = "ASC";
    }

    public class UpdateStudentRequest
    {
        public string? MaNguoiDung { get; set; }  

        // User
        public string? TenSinhVien { get; set; }        
        public string? TenDangNhap { get; set; }          
        public bool? TrangThai { get; set; }
        public int? MaQuyen { get; set; }

        // Student
        public string? LopHanhChinh { get; set; }
        public int? NamNhapHoc { get; set; }
        public string? Khoa { get; set; }
        public string? Nganh { get; set; }

        // Hồ sơ cá nhân (User)
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
