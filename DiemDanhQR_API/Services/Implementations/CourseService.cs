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

        public async Task<PagedResult<CourseListItem>> GetListAsync(CourseListRequest req)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize <= 0 ? 20 : Math.Min(req.PageSize, 200);
            var sortBy = req.SortBy ?? "MaLopHocPhan";
            var sortDir = (req.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (rows, total) = await _repo.SearchCoursesAsync(
                maLopHocPhan: HelperFunctions.NormalizeCode(req.MaLopHocPhan),
                tenLopHocPhan: req.TenLopHocPhan,
                trangThai: req.TrangThai,
                tenMonHoc: req.TenMonHoc,
                soTinChi: req.SoTinChi,
                soTiet: req.SoTiet,
                tenGiangVien: req.TenGiangVien,
                maMonHoc: HelperFunctions.NormalizeCode(req.MaMonHoc),
                maGiangVien: HelperFunctions.NormalizeCode(req.MaGiangVien),
                maHocKy: req.MaHocKy,
                namHoc: req.NamHoc,
                ky: req.Ky,
                maSinhVien: HelperFunctions.NormalizeCode(req.MaSinhVien),
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

                    MaGiangVien = x.Gv.MaGiangVien,
                    TenGiangVien = x.Nd.HoTen,

                    MaHocKy = x.Hk.MaHocKy,
                    NamHoc = x.Hk.NamHoc,
                    Ky = x.Hk.Ky,

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
                maMonHoc: HelperFunctions.NormalizeCode(req.MaMonHoc),
                tenMonHoc: req.TenMonHoc,
                soTinChi: req.SoTinChi,
                soTiet: req.SoTiet,
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

            var existed = await _repo.SubjectExistsAsync(code);
            if (existed)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Mã môn học đã tồn tại.");

            var entity = new MonHoc
            {
                MaMonHoc = code,
                TenMonHoc = req.TenMonHoc?.Trim(),
                SoTinChi = req.SoTinChi,
                SoTiet = req.SoTiet,
                MoTa = string.IsNullOrWhiteSpace(req.MoTa) ? null : req.MoTa!.Trim(),
                TrangThai = req.TrangThai ?? true
            };

            await _repo.AddSubjectAsync(entity);

            await _repo.LogActivityAsync(currentUserId, $"Tạo môn học: {entity.MaMonHoc} - {entity.TenMonHoc}");

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

            if (!req.MaHocKy.HasValue || req.MaHocKy.Value <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Mã học kỳ không hợp lệ.");

            var existed = await _repo.CourseExistsAsync(maLhp);
            if (existed)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Mã lớp học phần đã tồn tại.");

            var subjectOk = await _repo.SubjectExistsAsync(maMon);
            if (!subjectOk)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Môn học không tồn tại.");

            var lecturerOk = await _repo.LecturerExistsByCodeAsync(maGv);
            if (!lecturerOk)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Giảng viên không tồn tại.");

            var hkOk = await _repo.SemesterExistsByIdAsync(req.MaHocKy.Value);
            if (!hkOk)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Học kỳ không tồn tại.");

            var entity = new LopHocPhan
            {
                MaLopHocPhan = maLhp,
                TenLopHocPhan = req.TenLopHocPhan?.Trim(),
                TrangThai = req.TrangThai ?? true,
                MaMonHoc = maMon,
                MaGiangVien = maGv,
                MaHocKy = req.MaHocKy.Value
            };

            await _repo.AddCourseAsync(entity);

            await _repo.LogActivityAsync(currentUserId, $"Tạo lớp học phần: {entity.MaLopHocPhan} - {entity.TenLopHocPhan} (Môn: {entity.MaMonHoc}, GV: {entity.MaGiangVien}, HK: {entity.MaHocKy})");

            return new CreateCourseResponse
            {
                MaLopHocPhan = entity.MaLopHocPhan!,
                TenLopHocPhan = entity.TenLopHocPhan!,
                MaMonHoc = entity.MaMonHoc!,
                MaGiangVien = entity.MaGiangVien!,
                MaHocKy = entity.MaHocKy
            };
        }

        // public async Task<AddStudentToCourseResponse> AddStudentToCourseAsync(AddStudentToCourseRequest req, string? currentUserId)
        // {
        //     ... moved to StudentService ...
        // }

        public async Task<PagedResult<SemesterListItem>> GetSemestersAsync(SemesterListRequest req)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize <= 0 ? 20 : Math.Min(req.PageSize, 200);
            var sortBy = req.SortBy ?? "MaHocKy";
            var sortDir = (req.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (rows, total) = await _repo.SearchSemestersAsync(
                namHoc: req.NamHoc,
                ky: req.Ky,
                sortBy: sortBy,
                desc: desc,
                page: page,
                pageSize: pageSize
            );

            var items = rows.Select(x => new SemesterListItem
            {
                MaHocKy = x.MaHocKy,
                NamHoc = x.NamHoc,
                Ky = x.Ky
            }).ToList();

            return new PagedResult<SemesterListItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

        public async Task<CreateSemesterResponse> CreateSemesterAsync(CreateSemesterRequest req, string? currentUserId)
        {
            if (!req.NamHoc.HasValue || !req.Ky.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu năm học/kỳ.");

            var namHoc = req.NamHoc.Value;
            var ky = req.Ky.Value;

            if (await _repo.ExistsSemesterAsync(namHoc, ky))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Học kỳ đã tồn tại.");

            var entity = new HocKy
            {
                NamHoc = namHoc,
                Ky = ky
            };

            await _repo.AddSemesterAsync(entity);
            await _repo.LogActivityAsync(currentUserId, $"Tạo học kỳ {entity.NamHoc} - Kỳ {entity.Ky}");

            return new CreateSemesterResponse
            {
                MaHocKy = entity.MaHocKy,
                NamHoc = entity.NamHoc,
                Ky = entity.Ky
            };
        }

        public async Task<UpdateSemesterResponse> UpdateSemesterAsync(UpdateSemesterRequest req, string? currentUserId)
        {
            if (!req.MaHocKy.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu mã học kỳ.");

            var hk = await _repo.GetSemesterByIdAsync(req.MaHocKy.Value);
            if (hk == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy học kỳ.");

            var newNamHoc = req.NamHoc ?? hk.NamHoc;
            var newKy = req.Ky ?? hk.Ky;

            // Nếu thay đổi dẫn đến trùng (NamHoc, Ky)
            if (await _repo.ExistsSemesterAsync(newNamHoc, newKy, excludeId: hk.MaHocKy))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Học kỳ (năm học, kỳ) đã tồn tại.");

            hk.NamHoc = newNamHoc;
            hk.Ky = newKy;

            await _repo.UpdateSemesterAsync(hk);
            await _repo.LogActivityAsync(currentUserId, $"Cập nhật học kỳ {hk.MaHocKy}: {hk.NamHoc} - Kỳ {hk.Ky}");

            return new UpdateSemesterResponse
            {
                MaHocKy = hk.MaHocKy,
                NamHoc = hk.NamHoc,
                Ky = hk.Ky
            };
        }

        public async Task<UpdateSubjectResponse> UpdateSubjectAsync(UpdateSubjectRequest req, string? currentUserId)
        {
            var code = HelperFunctions.NormalizeCode(req.MaMonHoc);
            var subject = await _repo.GetSubjectByCodeAsync(code);
            if (subject == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy môn học.");

            if (!string.IsNullOrWhiteSpace(req.TenMonHoc)) subject.TenMonHoc = req.TenMonHoc!.Trim();
            if (req.SoTinChi.HasValue) subject.SoTinChi = req.SoTinChi;
            if (req.SoTiet.HasValue) subject.SoTiet = req.SoTiet;
            if (req.TrangThai.HasValue) subject.TrangThai = req.TrangThai;
            if (req.MoTa != null) subject.MoTa = string.IsNullOrWhiteSpace(req.MoTa) ? null : req.MoTa!.Trim();

            await _repo.UpdateSubjectAsync(subject);
            await _repo.LogActivityAsync(currentUserId, $"Cập nhật môn học: {subject.MaMonHoc} - {subject.TenMonHoc}");

            return new UpdateSubjectResponse
            {
                MaMonHoc = subject.MaMonHoc,
                TenMonHoc = subject.TenMonHoc,
                SoTinChi = (byte?)(subject.SoTinChi ?? 0),
                SoTiet = (byte?)(subject.SoTiet ?? 0),
                MoTa = subject.MoTa,
                TrangThai = subject.TrangThai
            };
        }

        public async Task<UpdateCourseResponse> UpdateCourseAsync(UpdateCourseRequest req, string? currentUserId)
        {
            var code = HelperFunctions.NormalizeCode(req.MaLopHocPhan);
            var course = await _repo.GetCourseByCodeAsync(code);
            if (course == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy lớp học phần.");

            // Validate foreign keys if provided
            if (!string.IsNullOrWhiteSpace(req.MaMonHoc))
            {
                var maMon = HelperFunctions.NormalizeCode(req.MaMonHoc);
                var ok = await _repo.SubjectExistsAsync(maMon);
                if (!ok) ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Môn học không tồn tại.");
                course.MaMonHoc = maMon;
            }

            if (!string.IsNullOrWhiteSpace(req.MaGiangVien))
            {
                var maGv = HelperFunctions.NormalizeCode(req.MaGiangVien);
                var ok = await _repo.LecturerExistsByCodeAsync(maGv);
                if (!ok) ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Giảng viên không tồn tại.");
                course.MaGiangVien = maGv;
            }

            if (req.MaHocKy.HasValue)
            {
                if (req.MaHocKy.Value <= 0)
                    ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Mã học kỳ không hợp lệ.");
                var hkOk = await _repo.SemesterExistsByIdAsync(req.MaHocKy.Value);
                if (!hkOk) ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Học kỳ không tồn tại.");
                course.MaHocKy = req.MaHocKy.Value;
            }

            if (!string.IsNullOrWhiteSpace(req.TenLopHocPhan)) course.TenLopHocPhan = req.TenLopHocPhan!.Trim();
            if (req.TrangThai.HasValue) course.TrangThai = req.TrangThai;

            await _repo.UpdateCourseAsync(course);
            await _repo.LogActivityAsync(currentUserId, $"Cập nhật lớp học phần: {course.MaLopHocPhan} - {course.TenLopHocPhan}");

            return new UpdateCourseResponse
            {
                MaLopHocPhan = course.MaLopHocPhan,
                TenLopHocPhan = course.TenLopHocPhan,
                TrangThai = course.TrangThai,
                MaMonHoc = course.MaMonHoc,
                MaGiangVien = course.MaGiangVien,
                MaHocKy = course.MaHocKy
            };
        }
    }
}
