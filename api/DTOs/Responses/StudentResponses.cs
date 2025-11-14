// File: DTOs/Responses/StudentResponses.cs

using System.Text.Json.Serialization;

namespace api.DTOs
{
    public class CreateStudentResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; } // MaNguoiDung, HoTen, TenDangNhap, TrangThai, GioiTinh, AnhDaiDien, Email, SoDienThoai, NgaySinh, DiaChi 
        public SinhVienDTO? SinhVien { get; set; } // MaSinhVien, NamNhapHoc
        public NganhDTO? Nganh { get; set; } // MaNganh, CodeNganh, TenNganh
        public KhoaDTO? Khoa { get; set; } // MaKhoa, CodeKhoa ,TenKhoa
        public PhanQuyenDTO? PhanQuyen { get; set; } // MaQuyen, CodeQuyen, TenQuyen

    }
    public class StudentInfoResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; } // MaNguoiDung, HoTen, TenDangNhap, TrangThai, GioiTinh, AnhDaiDien, Email, SoDienThoai, NgaySinh, DiaChi 
        public SinhVienDTO? SinhVien { get; set; } // MaSinhVien, NamNhapHoc
        public NganhDTO? Nganh { get; set; } // MaNganh, CodeNganh, TenNganh
        public KhoaDTO? Khoa { get; set; } // MaKhoa, CodeKhoa ,TenKhoa
        public PhanQuyenDTO? PhanQuyen { get; set; } // MaQuyen, CodeQuyen, TenQuyen
    }
    public class StudentListItemResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; } // MaNguoiDung, HoTen, TenDangNhap, TrangThai
        public SinhVienDTO? SinhVien { get; set; }   // MaSinhVien, NamNhapHoc
        public NganhDTO? Nganh { get; set; }         // MaNganh, CodeNganh, TenNganh
        public KhoaDTO? Khoa { get; set; }           // MaKhoa, CodeKhoa ,TenKhoa

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ThamGiaLopDTO? ThamGiaLop { get; set; } // NgayThamGia, TrangThai
    }

    public class UpdateStudentResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; } // MaNguoiDung, HoTen, TenDangNhap, TrangThai, GioiTinh, AnhDaiDien, Email, SoDienThoai, NgaySinh, DiaChi 
        public SinhVienDTO? SinhVien { get; set; } // MaSinhVien, NamNhapHoc
        public NganhDTO? Nganh { get; set; } // MaNganh, CodeNganh, TenNganh
        public KhoaDTO? Khoa { get; set; } // MaKhoa, CodeKhoa ,TenKhoa
        public PhanQuyenDTO? PhanQuyen { get; set; } // MaQuyen, CodeQuyen, TenQuyen
    }

    public class AddStudentToCourseResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; } // MaNguoiDung, HoTen
        public SinhVienDTO? SinhVien { get; set; } // MaSinhVien, NamNhapHoc
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLopHocPhan
        public ThamGiaLopDTO? ThamGiaLop { get; set; } // NgayThamGia, TrangThai
    }
    public class RemoveStudentFromCourseResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; } // MaNguoiDung, HoTen
        public SinhVienDTO? SinhVien { get; set; } // MaSinhVien, NamNhapHoc
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLopHocPhan
        public ThamGiaLopDTO? ThamGiaLop { get; set; } // NgayThamGia, TrangThai
    }
    public class BulkImportStudentsResponse
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<RowError> FailedDetails { get; set; } = new(); // thêm
    }
    public class RowError
    {
        public int Row { get; set; }
        public string Error { get; set; } = string.Empty;
    }

}
