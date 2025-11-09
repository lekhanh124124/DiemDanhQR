// File: DTOs/Responses/UserResponses.cs

namespace api.DTOs
{
    public class CreateUserResponse
    {
        public required NguoiDungDTO NguoiDung { get; set; } // MaNguoiDung, HoTen, TenDangNhap, TrangThai, GioiTinh, AnhDaiDien, Email, SoDienThoai, NgaySinh, DiaChi 
        public required PhanQuyenDTO PhanQuyen { get; set; } // MaQuyen, CodeQuyen, TenQuyen
    }

    public class UserActivityItem
    {
        public required NguoiDungDTO NguoiDung { get; set; } // MaNguoiDung, HoTen, TenDangNhap
        public required LichSuHoatDongDTO LichSuHoatDong { get; set; } // MaLichSu, ThoiGian, HanhDong
    }

    public class UpdateUserProfileResponse
    {
        public required NguoiDungDTO NguoiDung { get; set; } // MaNguoiDung, HoTen, TenDangNhap, TrangThai, GioiTinh, AnhDaiDien, Email, SoDienThoai, NgaySinh, DiaChi 
        public required PhanQuyenDTO PhanQuyen { get; set; } // MaQuyen, CodeQuyen, TenQuyen
    }

    public class UserItem
    {
        public required NguoiDungDTO NguoiDung { get; set; } // MaNguoiDung, HoTen, TenDangNhap, TrangThai
        public required PhanQuyenDTO PhanQuyen { get; set; } // MaQuyen, CodeQuyen, TenQuyen
    }
}