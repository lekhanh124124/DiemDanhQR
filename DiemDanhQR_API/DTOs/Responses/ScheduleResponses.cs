// File: DTOs/Responses/ScheduleResponses.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class ScheduleListItem
    {
        // Các trường yêu cầu trong response
        public int MaBuoi { get; }
        public int MaPhong { get; }
        public string TenPhong { get; }
        public string MaLopHocPhan { get; }           // MaLopHocPhan
        public string TenLop { get; }
        public string TenMonHoc { get; }
        public DateTime NgayHoc { get; }
        public byte TietBatDau { get; }
        public byte SoTiet { get; }
        public string GhiChu { get; }

        // Thông tin mở rộng về lớp/môn/giảng viên (theo yêu cầu phần "các items có các thông tin như...")
        public bool TrangThaiLop { get; }
        public byte SoTinChi { get; }
        public byte? HocKy { get; }
        public string TenGiangVien { get; }

        // Constructor: service chỉ truyền tham số, không cần viết biến trung gian
        public ScheduleListItem(
            int maBuoi,
            int maPhong,
            string tenPhong,
            string maLopHocPhan,
            string tenLop,
            string tenMonHoc,
            DateTime ngayHoc,
            byte tietBatDau,
            byte soTiet,
            string ghiChu,
            bool trangThaiLop,
            byte soTinChi,
            byte? hocKy,
            string tenGiangVien)
        {
            MaBuoi = maBuoi;
            MaPhong = maPhong;
            TenPhong = tenPhong;
            MaLopHocPhan = maLopHocPhan;
            TenLop = tenLop;
            TenMonHoc = tenMonHoc;
            NgayHoc = ngayHoc;
            TietBatDau = tietBatDau;
            SoTiet = soTiet;
            GhiChu = ghiChu;
            TrangThaiLop = trangThaiLop;
            SoTinChi = soTinChi;
            HocKy = hocKy;
            TenGiangVien = tenGiangVien;
        }
    }
}
