// File: DTOs/Responses/CourseResponses.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class CourseListItem
    {
        public string MaLopHocPhan { get; }
        public string TenLopHocPhan { get; }
        public bool TrangThai { get; }

        public string MaMonHoc { get; }      // NEW
        public string TenMonHoc { get; }
        public byte SoTinChi { get; }
        public byte SoTiet { get; }
        public byte? HocKy { get; }

        public string MaGiangVien { get; }   // NEW
        public string TenGiangVien { get; }

        public CourseListItem(
            string maLopHocPhan,
            string tenLopHocPhan,
            bool trangThai,
            string maMonHoc,
            string tenMonHoc,
            byte soTinChi,
            byte soTiet,
            byte? hocKy,
            string maGiangVien,
            string tenGiangVien)
        {
            MaLopHocPhan = maLopHocPhan;
            TenLopHocPhan = tenLopHocPhan;
            TrangThai = trangThai;

            MaMonHoc = maMonHoc;
            TenMonHoc = tenMonHoc;
            SoTinChi = soTinChi;
            SoTiet = soTiet;
            HocKy = hocKy;

            MaGiangVien = maGiangVien;
            TenGiangVien = tenGiangVien;
        }
    }

    public class CourseListResponse
    {
        public int Page { get; }
        public int PageSize { get; }
        public int TotalItems { get; }
        public int TotalPages { get; }
        public IEnumerable<CourseListItem> Items { get; }

        public CourseListResponse(int page, int pageSize, int totalItems, int totalPages, IEnumerable<CourseListItem> items)
        {
            Page = page;
            PageSize = pageSize;
            TotalItems = totalItems;
            TotalPages = totalPages;
            Items = items;
        }
    }
}
