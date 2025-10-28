// File: Services/Implementations/CourseService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Helpers;
using DiemDanhQR_API.Models;
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
                new CourseListItem
                {
                    MaLopHocPhan = x.Lhp.MaLopHocPhan,
                    TenLopHocPhan = x.Lhp.TenLopHocPhan,
                    TrangThai = x.Lhp.TrangThai,
                    MaMonHoc = x.Mh.MaMonHoc,
                    TenMonHoc = x.Mh.TenMonHoc,
                    SoTinChi = (byte?)(x.Mh.SoTinChi ?? 0),
                    SoTiet = (byte?)(x.Mh.SoTiet ?? 0),
                    HocKy = x.Mh.HocKy,
                    MaGiangVien = x.Gv.MaGiangVien,
                    TenGiangVien = x.Nd.HoTen,
                    // Thông tin tham gia
                    NgayThamGia = x.NgayThamGia,
                    TrangThaiThamGia = x.TrangThaiThamGia
                }).ToList();

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

            var items = rows.Select(m => new SubjectListItem
            {
                MaMonHoc = m.MaMonHoc,
                TenMonHoc = m.TenMonHoc,
                SoTinChi = (byte?)(m.SoTinChi ?? 0),
                SoTiet = (byte?)(m.SoTiet ?? 0),
                HocKy = m.HocKy,
                MoTa = m.MoTa,
                TrangThai = m.TrangThai
            }).ToList();

            return new PagedResult<SubjectListItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }
        public async Task<CreateSubjectResponse> CreateSubjectAsync(CreateSubjectRequest req, string? currentUserId)
        {
            var code = HelperFunctions.NormalizeCode(req.MaMonHoc);

            // Kiểm tra trùng mã
            var existed = await _repo.SubjectExistsAsync(code);
            if (existed)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Mã môn học đã tồn tại.");

            // Map DTO -> Model (repo chỉ làm việc với Model)
            var entity = new MonHoc
            {
                MaMonHoc = code,
                TenMonHoc = req.TenMonHoc?.Trim(),
                SoTinChi = req.SoTinChi,
                SoTiet = req.SoTiet,
                HocKy = req.HocKy,
                MoTa = string.IsNullOrWhiteSpace(req.MoTa) ? null : req.MoTa!.Trim(),
                TrangThai = req.TrangThai ?? true
            };

            await _repo.AddSubjectAsync(entity);

            // Ghi log
            var log = new LichSuHoatDong
            {
                MaNguoiDung = string.IsNullOrWhiteSpace(currentUserId) ? "system" : currentUserId,
                HanhDong = $"Tạo môn học: {entity.MaMonHoc} - {entity.TenMonHoc}"
            };
            await _repo.WriteActivityLogAsync(log);

            return new CreateSubjectResponse
            {
                MaMonHoc = entity.MaMonHoc!,
                TenMonHoc = entity.TenMonHoc!
            };
        }
        public async Task<CreateCourseResponse> CreateCourseAsync(CreateCourseRequest req, string? currentUserId)
        {
            var maLhp = HelperFunctions.NormalizeCode(req.MaLopHocPhan);
            var maMon = HelperFunctions.NormalizeCode(req.MaMonHoc);
            var maGv = HelperFunctions.NormalizeCode(req.MaGiangVien);

            // 1) Kiểm tra trùng mã lớp học phần
            var existed = await _repo.CourseExistsAsync(maLhp);
            if (existed)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Mã lớp học phần đã tồn tại.");

            // 2) Kiểm tra tồn tại môn học
            var subjectOk = await _repo.SubjectExistsAsync(maMon);
            if (!subjectOk)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Môn học không tồn tại.");

            // 3) Kiểm tra tồn tại giảng viên theo MaGiangVien
            var lecturerOk = await _repo.LecturerExistsByCodeAsync(maGv);
            if (!lecturerOk)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Giảng viên không tồn tại.");

            // 4) Tạo entity
            var entity = new LopHocPhan
            {
                MaLopHocPhan = maLhp,
                TenLopHocPhan = req.TenLopHocPhan?.Trim(),
                TrangThai = req.TrangThai ?? true,
                MaMonHoc = maMon,
                MaGiangVien = maGv
            };

            await _repo.AddCourseAsync(entity);

            // 5) Ghi log
            var log = new LichSuHoatDong
            {
                MaNguoiDung = string.IsNullOrWhiteSpace(currentUserId) ? "system" : currentUserId,
                HanhDong = $"Tạo lớp học phần: {entity.MaLopHocPhan} - {entity.TenLopHocPhan} (Môn: {entity.MaMonHoc}, GV: {entity.MaGiangVien})"
            };
            await _repo.WriteActivityLogAsync(log);

            return new CreateCourseResponse
            {
                MaLopHocPhan = entity.MaLopHocPhan!,
                TenLopHocPhan = entity.TenLopHocPhan!,
                MaMonHoc = entity.MaMonHoc!,
                MaGiangVien = entity.MaGiangVien!
            };
        }
        public async Task<AddStudentToCourseResponse> AddStudentToCourseAsync(AddStudentToCourseRequest req, string? currentUserId)
        {
            var maLhp = HelperFunctions.NormalizeCode(req.MaLopHocPhan);
            var maSv = HelperFunctions.NormalizeCode(req.MaSinhVien);

            // 1) Kiểm tra tồn tại LHP
            if (!await _repo.CourseExistsAsync(maLhp))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Lớp học phần không tồn tại.");

            // 2) Kiểm tra tồn tại Sinh viên
            if (!await _repo.StudentExistsByCodeAsync(maSv))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Sinh viên không tồn tại.");

            // 3) Chống trùng tham gia
            if (await _repo.ParticipationExistsAsync(maLhp, maSv))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Sinh viên đã tham gia lớp học phần này.");

            // 4) Tạo entity
            var entity = new ThamGiaLop
            {
                MaLopHocPhan = maLhp,
                MaSinhVien = maSv,
                NgayThamGia = req.NgayThamGia ?? DateTime.Now,
                TrangThai = req.TrangThai ?? true
            };

            await _repo.AddParticipationAsync(entity);

            // 5) Ghi log
            var log = new LichSuHoatDong
            {
                MaNguoiDung = string.IsNullOrWhiteSpace(currentUserId) ? "system" : currentUserId,
                HanhDong = $"Thêm sinh viên {entity.MaSinhVien} vào lớp {entity.MaLopHocPhan}"
            };
            await _repo.WriteActivityLogAsync(log);

            return new AddStudentToCourseResponse
            {
                MaLopHocPhan = entity.MaLopHocPhan!,
                MaSinhVien = entity.MaSinhVien!,
                NgayThamGia = entity.NgayThamGia?.ToString("dd-MM-yyyy"),
                TrangThai = entity.TrangThai
            };
        }
    }
}
