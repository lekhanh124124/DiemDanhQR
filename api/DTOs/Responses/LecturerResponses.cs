// File: DTOs/Responses/LecturerResponses.cs

namespace api.DTOs
{
    public class CreateLecturerResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; } // MaGiangVien, HoTen, TenDangNhap, TrangThai, GioiTinh, AnhDaiDien, Email, SoDienThoai, NgaySinh, DiaChi 
        public GiangVienDTO? GiangVien { get; set; } // HocHam, HocVi, NgayTuyenDung
        public KhoaDTO? Khoa { get; set; } // MaKhoa, CodeKhoa ,TenKhoa
        public PhanQuyenDTO? PhanQuyen { get; set; } // MaQuyen, CodeQuyen, TenQuyen
    }

    public class LecturerInfoResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; } // MaGiangVien, HoTen, TenDangNhap, TrangThai, GioiTinh, AnhDaiDien, Email, SoDienThoai, NgaySinh, DiaChi 
        public GiangVienDTO? GiangVien { get; set; } // HocHam, HocVi, NgayTuyenDung
        public KhoaDTO? Khoa { get; set; } // MaKhoa, CodeKhoa ,TenKhoa
        public PhanQuyenDTO? PhanQuyen { get; set; } // MaQuyen, CodeQuyen, TenQuyen

    }
    public class LecturerListItemResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; } // MaGiangVien, HoTen, TenDangNhap, TrangThai
        public GiangVienDTO? GiangVien { get; set; } // HocHam, HocVi, NgayTuyenDung
        public KhoaDTO? Khoa { get; set; } // MaKhoa, CodeKhoa ,TenKhoa

    }

    public class UpdateLecturerResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; } // MaGiangVien, HoTen, TenDangNhap, TrangThai, GioiTinh, AnhDaiDien, Email, SoDienThoai, NgaySinh, DiaChi 
        public GiangVienDTO? GiangVien { get; set; } // HocHam, HocVi, NgayTuyenDung
        public KhoaDTO? Khoa { get; set; } // MaKhoa, CodeKhoa ,TenKhoa
        public PhanQuyenDTO? PhanQuyen { get; set; } // MaQuyen, CodeQuyen, TenQuyen
    }
}
