// File: DTOs/Responses/StudentResponses.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class CreateStudentResponse
    {
        public string MaSinhVien { get; }
        public string MaNguoiDung { get; }
        public string TenDangNhap { get; }
        public string HoTen { get; }
        public int MaQuyen { get; }
        public bool TrangThaiUser { get; }

        public string? LopHanhChinh { get; }
        public int? NamNhapHoc { get; }
        public string? Khoa { get; }
        public string? Nganh { get; }

        // Service truyền thẳng tham số qua ctor
        public CreateStudentResponse(
            string maSinhVien,
            string maNguoiDung,
            string tenDangNhap,
            string hoTen,
            int maQuyen,
            bool trangThaiUser,
            string? lopHanhChinh,
            int? namNhapHoc,
            string? khoa,
            string? nganh)
        {
            MaSinhVien = maSinhVien;
            MaNguoiDung = maNguoiDung;
            TenDangNhap = tenDangNhap;
            HoTen = hoTen;
            MaQuyen = maQuyen;
            TrangThaiUser = trangThaiUser;
            LopHanhChinh = lopHanhChinh;
            NamNhapHoc = namNhapHoc;
            Khoa = khoa;
            Nganh = nganh;
        }
    }
    public class StudentInfoResponse
    {
        // Thông tin user chung (gói gọn vào payload sinh viên)
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

        // Thông tin sinh viên
        public string MaSinhVien { get; }
        public string? LopHanhChinh { get; }
        public int? NamNhapHoc { get; }
        public string? Khoa { get; }
        public string? Nganh { get; }

        public StudentInfoResponse(
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
            string maSinhVien,
            string? lopHanhChinh,
            int? namNhapHoc,
            string? khoa,
            string? nganh)
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

            MaSinhVien = maSinhVien;
            LopHanhChinh = lopHanhChinh;
            NamNhapHoc = namNhapHoc;
            Khoa = khoa;
            Nganh = nganh;
        }
    }
}
