// File: DTOs/Responses/ScheduleResponses.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class ScheduleListItem
    {
        public int MaBuoi { get; }
        public int MaPhong { get; }
        public string TenPhong { get; }
        public string MaLopHocPhan { get; }           // MaLopHocPhan
        public string TenLopHocPhan { get; }
        public string TenMonHoc { get; }
        public DateTime NgayHoc { get; }
        public byte TietBatDau { get; }
        public byte SoTiet { get; }
        public string TenGiangVien { get; }    // << yêu cầu mới
        public string GhiChu { get; }

        public ScheduleListItem(
            int maBuoi,
            int maPhong,
            string tenPhong,
            string maLopHocPhan,
            string tenLopHocPhan,
            string tenMonHoc,
            DateTime ngayHoc,
            byte tietBatDau,
            byte soTiet,
            string tenGiangVien,
            string ghiChu)
        {
            MaBuoi = maBuoi;
            MaPhong = maPhong;
            TenPhong = tenPhong;
            MaLopHocPhan = maLopHocPhan;
            TenLopHocPhan = tenLopHocPhan;
            TenMonHoc = tenMonHoc;
            NgayHoc = ngayHoc;
            TietBatDau = tietBatDau;
            SoTiet = soTiet;
            TenGiangVien = tenGiangVien;
            GhiChu = ghiChu;
        }
    }
}
