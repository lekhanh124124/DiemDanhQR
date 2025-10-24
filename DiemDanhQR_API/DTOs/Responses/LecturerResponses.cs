// File: DTOs/Responses/LecturerResponses.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class CreateLecturerResponse
    {
        public string MaGiangVien { get; }
        public string MaNguoiDung { get; }
        public string TenDangNhap { get; }
        public string HoTen { get; }
        public int MaQuyen { get; }
        public string? Khoa { get; }
        public string? HocHam { get; }
        public string? HocVi { get; }
        public DateTime? NgayTuyenDung { get; }
        public bool TrangThaiUser { get; }

        // Service truyền thẳng tham số, không cần biến trung gian
        public CreateLecturerResponse(
            string maGiangVien,
            string maNguoiDung,
            string tenDangNhap,
            string hoTen,
            int maQuyen,
            string? khoa,
            string? hocHam,
            string? hocVi,
            DateTime? ngayTuyenDung,
            bool trangThaiUser)
        {
            MaGiangVien = maGiangVien;
            MaNguoiDung = maNguoiDung;
            TenDangNhap = tenDangNhap;
            HoTen = hoTen;
            MaQuyen = maQuyen;
            Khoa = khoa;
            HocHam = hocHam;
            HocVi = hocVi;
            NgayTuyenDung = ngayTuyenDung;
            TrangThaiUser = trangThaiUser;
        }
    }
    public class LecturerInfoResponse
    {
        // Thông tin user chung (gói gọn vào payload giảng viên)
        public string MaNguoiDung { get; }
        public string? HoTen { get; set; }
        public byte? GioiTinh { get; set; }
        public string? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? DiaChi { get; set; }
        public bool TrangThai { get; }

        // Thông tin giảng viên
        public string MaGiangVien { get; }
        public string? Khoa { get; }
        public string? HocHam { get; }
        public string? HocVi { get; }
        public DateTime? NgayTuyenDung { get; }

        // Full-parameter constructor
        public LecturerInfoResponse(
            string maNguoiDung,
            string? hoTen,
            byte? gioiTinh,
            string? anhDaiDien,
            string? email,
            string? soDienThoai,
            DateTime? ngaySinh,
            string? danToc,
            string? tonGiao,
            string? diaChi,
            bool trangThai,
            string maGiangVien,
            string? khoa,
            string? hocHam,
            string? hocVi,
            DateTime? ngayTuyenDung)
        {
            MaNguoiDung = maNguoiDung;
            HoTen = hoTen;
            GioiTinh = gioiTinh;
            AnhDaiDien = anhDaiDien;
            Email = email;
            SoDienThoai = soDienThoai;
            NgaySinh = ngaySinh;
            DanToc = danToc;
            TonGiao = tonGiao;
            DiaChi = diaChi;
            TrangThai = trangThai;
            MaGiangVien = maGiangVien;
            Khoa = khoa;
            HocHam = hocHam;
            HocVi = hocVi;
            NgayTuyenDung = ngayTuyenDung;
        }
    }
    public class LecturerListItemResponse
    {
        public string MaGiangVien { get; }
        public string? HoTen { get; }
        public string? Khoa { get; }
        public string? HocHam { get; }
        public string? HocVi { get; }
        public DateTime? NgayTuyenDung { get; }

        public LecturerListItemResponse(
            string maGiangVien,
            string? hoTen,
            string? khoa,
            string? hocHam,
            string? hocVi,
            DateTime? ngayTuyenDung)
        {
            MaGiangVien = maGiangVien;
            HoTen = hoTen;
            Khoa = khoa;
            HocHam = hocHam;
            HocVi = hocVi;
            NgayTuyenDung = ngayTuyenDung;
        }
    }
}
