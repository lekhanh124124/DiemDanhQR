// File: Services/Implementations/CourseService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Helpers;
using DiemDanhQR_API.Repositories.Interfaces;
using DiemDanhQR_API.Services.Interfaces;

namespace DiemDanhQR_API.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _repo;
        public CourseService(ICourseRepository repo) => _repo = repo;

        // ĐỔI KIỂU TRẢ VỀ → PagedResult<CourseListItem>
        public async Task<PagedResult<CourseListItem>> GetListAsync(CourseListRequest req)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize <= 0 ? 20 : Math.Min(req.PageSize, 200);
            var sortBy = req.SortBy ?? "MaLopHocPhan";
            var sortDir = (req.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (rows, total) = await _repo.SearchCoursesAsync(
                keyword: req.Keyword,
                maLopHocPhan: HelperFunctions.NormalizeCode(req.MaLopHocPhan),
                tenLopHocPhan: req.TenLopHocPhan,
                trangThai: req.TrangThai,
                tenMonHoc: req.TenMonHoc,
                soTinChi: req.SoTinChi,
                soTiet: req.SoTiet,
                hocKy: req.HocKy,
                tenGiangVien: req.TenGiangVien,
                maMonHoc: HelperFunctions.NormalizeCode(req.MaMonHoc),
                maGiangVien: HelperFunctions.NormalizeCode(req.MaGiangVien),
                sortBy: sortBy,
                desc: desc,
                page: page,
                pageSize: pageSize
            );

            var items = rows.Select(x =>
                new CourseListItem(
                    x.Lhp.MaLopHocPhan ?? string.Empty,
                    x.Lhp.TenLopHocPhan ?? string.Empty,
                    x.Lhp.TrangThai ?? true,
                    x.Mh.MaMonHoc ?? string.Empty,
                    x.Mh.TenMonHoc ?? string.Empty,
                    (byte)(x.Mh.SoTinChi ?? 0),
                    (byte)(x.Mh.SoTiet ?? 0),
                    x.Mh.HocKy,
                    x.Gv.MaGiangVien ?? string.Empty,
                    x.Nd.HoTen ?? string.Empty
                )).ToList();

            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            return new PagedResult<CourseListItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task<PagedResult<CourseParticipantItem>> GetParticipantsAsync(CourseParticipantsRequest req)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize <= 0 ? 20 : Math.Min(req.PageSize, 200);
            var sortBy = req.SortBy ?? "MaLopHocPhan";
            var sortDir = (req.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (rows, total) = await _repo.SearchCourseParticipantsAsync(
                keyword: req.Keyword,
                maLopHocPhan: HelperFunctions.NormalizeCode(req.MaLopHocPhan),
                tenLopHocPhan: req.TenLopHocPhan,
                maMonHoc: HelperFunctions.NormalizeCode(req.MaMonHoc),
                tenMonHoc: req.TenMonHoc,
                hocKy: req.HocKy,                                
                maSinhVien: HelperFunctions.NormalizeCode(req.MaSinhVien),
                tenSinhVien: req.TenSinhVien,
                ngayFrom: req.NgayThamGiaFrom,
                ngayTo: req.NgayThamGiaTo,
                trangThaiThamGia: req.TrangThaiThamGia,
                maGiangVien: HelperFunctions.NormalizeCode(req.MaGiangVien),
                tenGiangVien: req.TenGiangVien,
                sortBy: sortBy,
                desc: desc,
                page: page,
                pageSize: pageSize
            );

            var items = rows.Select(x =>
                new CourseParticipantItem(
                    x.Lhp.MaLopHocPhan ?? string.Empty,
                    x.Lhp.TenLopHocPhan ?? string.Empty,
                    x.Mh.MaMonHoc ?? string.Empty,
                    x.Mh.TenMonHoc ?? string.Empty,
                    x.Mh.HocKy,                                  
                    x.Sv.MaSinhVien ?? string.Empty,
                    x.NdSv.HoTen ?? string.Empty,
                    x.Tgl.NgayThamGia ?? DateTime.MinValue,
                    x.Tgl.TrangThai ?? true,
                    x.Gv.MaGiangVien ?? string.Empty,
                    x.NdGv.HoTen ?? string.Empty
                )).ToList();

            return new PagedResult<CourseParticipantItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

    }
}
