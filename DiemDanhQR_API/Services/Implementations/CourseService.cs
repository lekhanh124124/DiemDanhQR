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
                maSinhVien: HelperFunctions.NormalizeCode(req.MaSinhVien), // NEW
                sortBy: sortBy,
                desc: desc,
                page: page,
                pageSize: pageSize
            );

            var items = rows.Select(x =>
                new CourseListItem(
                    maLopHocPhan: x.Lhp.MaLopHocPhan ?? string.Empty,
                    tenLopHocPhan: x.Lhp.TenLopHocPhan ?? string.Empty,
                    trangThai: x.Lhp.TrangThai ?? true,
                    maMonHoc: x.Mh.MaMonHoc ?? string.Empty,
                    tenMonHoc: x.Mh.TenMonHoc ?? string.Empty,
                    soTinChi: (byte)(x.Mh.SoTinChi ?? 0),
                    soTiet: (byte)(x.Mh.SoTiet ?? 0),
                    hocKy: x.Mh.HocKy,
                    maGiangVien: x.Gv.MaGiangVien ?? string.Empty,
                    tenGiangVien: x.Nd.HoTen ?? string.Empty,
                    ngayThamGia: x.NgayThamGia,          // NEW (null nếu không truyền MaSinhVien)
                    trangThaiThamGia: x.TrangThaiThamGia // NEW (null nếu không truyền MaSinhVien)
                )).ToList();

            return new PagedResult<CourseListItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

        public async Task<PagedResult<SubjectListItem>> GetSubjectsAsync(SubjectListRequest req)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize <= 0 ? 20 : Math.Min(req.PageSize, 200);
            var sortBy = req.SortBy ?? "MaMonHoc";
            var sortDir = (req.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (rows, total) = await _repo.SearchSubjectsAsync(
                keyword: req.Keyword,
                maMonHoc: HelperFunctions.NormalizeCode(req.MaMonHoc),
                tenMonHoc: req.TenMonHoc,
                soTinChi: req.SoTinChi,
                soTiet: req.SoTiet,
                hocKy: req.HocKy,
                trangThai: req.TrangThai,
                sortBy: sortBy,
                desc: desc,
                page: page,
                pageSize: pageSize
            );

            var items = rows.Select(m => new SubjectListItem(
                maMonHoc: m.MaMonHoc ?? string.Empty,
                tenMonHoc: m.TenMonHoc ?? string.Empty,
                soTinChi: (byte)(m.SoTinChi ?? 0),
                soTiet: (byte)(m.SoTiet ?? 0),
                hocKy: m.HocKy,
                moTa: m.MoTa ?? string.Empty,
                trangThai: m.TrangThai ?? true
            )).ToList();

            return new PagedResult<SubjectListItem>
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
