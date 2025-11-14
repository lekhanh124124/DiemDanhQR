// File: DTOs/AllModelsDTOs.cs
using System.Text.Json.Serialization;

namespace api.DTOs
{
    public class BuoiHocDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaBuoi { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? NgayHoc { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TietBatDau { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? SoTiet { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? GhiChu { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TrangThai { get; set; }
    }

    public class ChucNangDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaChucNang { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? CodeChucNang { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TenChucNang { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MoTa { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? ParentChucNangId { get; set; }

    }

    public class DiemDanhDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaDiemDanh { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? ThoiGianQuet { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? LyDo { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TrangThai { get; set; }
    }

    public class GiangVienDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaGiangVien { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? HocHam { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? HocVi { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? NgayTuyenDung { get; set; }
    }

    public class HocKyDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaHocKy { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? NamHoc { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Ky { get; set; }
    }

    public class KhoaDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaKhoa { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? CodeKhoa { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TenKhoa { get; set; }
    }

    public class LichSuHoatDongDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaLichSu { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? ThoiGian { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? HanhDong { get; set; }
    }

    public class LopHocPhanDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaLopHocPhan { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TenLopHocPhan { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TrangThai { get; set; }
    }

    public class MonHocDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaMonHoc { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TenMonHoc { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? SoTinChi { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? SoTiet { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MoTa { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TrangThai { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? LoaiMon { get; set; }
    }

    public class NganhDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaNganh { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? CodeNganh { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TenNganh { get; set; }
    }

    public class NguoiDungDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaNguoiDung { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? HoTen { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? GioiTinh { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? AnhDaiDien { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Email { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? SoDienThoai { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? NgaySinh { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? DiaChi { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TenDangNhap { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MatKhau { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TrangThai { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? RefreshTokenHash { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? RefreshTokenIssuedAt { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? RefreshTokenExpiresAt { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? RefreshTokenId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? RefreshTokenRevokedAt { get; set; }
    }

    public class NhomChucNangDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TrangThai { get; set; }
    }

    public class PhanQuyenDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaQuyen { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? CodeQuyen { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TenQuyen { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MoTa { get; set; }
    }

    public class PhongHocDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaPhong { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TenPhong { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? ToaNha { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Tang { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? SucChua { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TrangThai { get; set; }
    }

    public class SinhVienDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaSinhVien { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? NamNhapHoc { get; set; }
    }

    public class ThamGiaLopDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? NgayThamGia { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TrangThai { get; set; }
    }

    public class TrangThaiDiemDanhDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? MaTrangThai { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? TenTrangThai { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? CodeTrangThai { get; set; }
    }
}
