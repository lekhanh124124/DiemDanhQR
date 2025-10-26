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

        public async Task<CourseListResponse> GetListAsync(CourseListRequest req)
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
                maMonHoc: HelperFunctions.NormalizeCode(req.MaMonHoc),          // NEW
                maGiangVien: HelperFunctions.NormalizeCode(req.MaGiangVien),    // NEW
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
                    x.Mh.MaMonHoc ?? string.Empty,            // NEW
                    x.Mh.TenMonHoc ?? string.Empty,
                    (byte)(x.Mh.SoTinChi ?? 0),
                    (byte)(x.Mh.SoTiet ?? 0),
                    x.Mh.HocKy,
                    x.Gv.MaGiangVien ?? string.Empty,          // NEW
                    x.Nd.HoTen ?? string.Empty
                )).ToList();

            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            return new CourseListResponse(page, pageSize, total, totalPages, items);
        }
    }
}
