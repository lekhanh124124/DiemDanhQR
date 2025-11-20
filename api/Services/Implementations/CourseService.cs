// File: Services/Implementations/CourseService.cs
using api.DTOs;
using api.ErrorHandling;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using api.Services.Interfaces;

namespace api.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _repo;
        public CourseService(ICourseRepository repo) => _repo = repo;

        private static string? NormalizeCode(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private string inputResponse(string input) => input ?? "null";

        public async Task<PagedResult<CourseListItem>> GetListAsync(CourseListRequest req)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize <= 0 ? 20 : Math.Min(req.PageSize, 200);
            var sortBy = req.SortBy ?? "MaLopHocPhan";
            var sortDir = (req.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (rows, total) = await _repo.SearchCoursesAsync(
                maLopHocPhan: NormalizeCode(req.MaLopHocPhan),
                tenLopHocPhan: req.TenLopHocPhan,
                trangThai: req.TrangThai,
                maMonHoc: NormalizeCode(req.MaMonHoc),
                soTinChi: req.SoTinChi,
                maGiangVien: NormalizeCode(req.MaGiangVien),
                maHocKy: req.MaHocKy,
                namHoc: req.NamHoc,
                ky: req.Ky,
                maSinhVien: NormalizeCode(req.MaSinhVien),
                sortBy: sortBy,
                desc: desc,
                page: page,
                pageSize: pageSize
            );

            string fmtDateOnly(DateOnly? d) => d.HasValue ? d.Value.ToString("dd-MM-yyyy") : null!;

            var items = rows.Select(x =>
                new CourseListItem
                {
                    LopHocPhan = new LopHocPhanDTO
                    {
                        MaLopHocPhan = inputResponse(x.Lhp.MaLopHocPhan),
                        TenLopHocPhan = inputResponse(x.Lhp.TenLopHocPhan),
                        TrangThai = inputResponse(x.Lhp.TrangThai.ToString())
                    },
                    MonHoc = new MonHocDTO
                    {
                        MaMonHoc = inputResponse(x.Mh.MaMonHoc),
                        TenMonHoc = inputResponse(x.Mh.TenMonHoc),
                        SoTinChi = inputResponse(x.Mh.SoTinChi.ToString()),
                        SoTiet = inputResponse(x.Mh.SoTiet.ToString())
                    },
                    GiangVien = new GiangVienDTO
                    {
                        MaGiangVien = inputResponse(x.Gv?.MaGiangVien ?? "null")
                    },
                    // >>> MỚI: map HoTen giảng viên từ NguoiDung
                    GiangVienInfo = new NguoiDungDTO
                    {
                        HoTen = inputResponse(x.Nd?.HoTen ?? "null")
                    },
                    HocKy = new HocKyDTO
                    {
                        MaHocKy = inputResponse(x.Hk.MaHocKy.ToString()),
                        NamHoc = inputResponse(x.Hk.NamHoc.ToString()),
                        Ky = inputResponse(x.Hk.Ky.ToString())
                    },
                    ThamGiaLop = (req.MaSinhVien == null)
                        ? null
                        : new ThamGiaLopDTO
                        {
                            NgayThamGia = inputResponse(fmtDateOnly(x.NgayThamGia)),
                            TrangThai = inputResponse(x.TrangThaiThamGia?.ToString() ?? "null")
                        }
                }).ToList();

            return new PagedResult<CourseListItem>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(pageSize.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)pageSize)).ToString()),
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
                maMonHoc: NormalizeCode(req.MaMonHoc),
                tenMonHoc: req.TenMonHoc,
                soTinChi: req.SoTinChi,
                soTiet: req.SoTiet,
                trangThai: req.TrangThai,
                sortBy: sortBy,
                desc: desc,
                page: page,
                pageSize: pageSize,
                loaiMon: req.LoaiMon // NEW
            );

            var items = rows.Select(m => new SubjectListItem
            {
                MonHoc = new MonHocDTO
                {
                    MaMonHoc = inputResponse(m.MaMonHoc),
                    TenMonHoc = inputResponse(m.TenMonHoc),
                    SoTinChi = inputResponse(m.SoTinChi.ToString()),
                    SoTiet = inputResponse(m.SoTiet.ToString()),
                    MoTa = inputResponse(m.MoTa ?? "null"),
                    TrangThai = inputResponse(m.TrangThai.ToString()),
                    LoaiMon = inputResponse(m.LoaiMon.ToString()) // NEW: trả về LoaiMon
                }
            }).ToList();

            return new PagedResult<SubjectListItem>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(pageSize.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)pageSize)).ToString()),
                Items = items
            };
        }


        public async Task<CreateSubjectResponse> CreateSubjectAsync(CreateSubjectRequest req, string? currentUserLogin)
        {
            // Validate cơ bản
            if (string.IsNullOrWhiteSpace(req.TenMonHoc))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên môn học là bắt buộc.");
            if (!req.SoTinChi.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Số tín chỉ là bắt buộc.");
            if (!req.SoTiet.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Số tiết là bắt buộc.");
            if (req.LoaiMon.HasValue)
            {
                if (req.LoaiMon < 1 || req.LoaiMon > 3)
                    ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Loại môn không hợp lệ.");
            }

            var autoCode = await _repo.GenerateNextSubjectCodeAsync(req.TenMonHoc!.Trim());

            // Double-check (phòng trùng hi hữu)
            if (await _repo.SubjectExistsAsync(autoCode))
                ApiExceptionHelper.Throw(ApiErrorCode.Conflict, "Không thể sinh mã môn học (bị trùng). Vui lòng thử lại.");

            var entity = new MonHoc
            {
                MaMonHoc = autoCode,
                TenMonHoc = req.TenMonHoc!.Trim(),
                SoTinChi = req.SoTinChi!.Value,
                SoTiet = req.SoTiet!.Value,
                MoTa = string.IsNullOrWhiteSpace(req.MoTa) ? null : req.MoTa!.Trim(),
                TrangThai = req.TrangThai ?? true,
                LoaiMon = req.LoaiMon ?? 1
            };

            await _repo.AddSubjectAsync(entity);
            await _repo.LogActivityAsync(currentUserLogin, $"Tạo môn học: {entity.MaMonHoc} - {entity.TenMonHoc}");

            return new CreateSubjectResponse
            {
                MonHoc = new MonHocDTO
                {
                    MaMonHoc = inputResponse(entity.MaMonHoc),
                    TenMonHoc = inputResponse(entity.TenMonHoc),
                    SoTinChi = inputResponse(entity.SoTinChi.ToString()),
                    SoTiet = inputResponse(entity.SoTiet.ToString()),
                    MoTa = inputResponse(entity.MoTa ?? "null"),
                    LoaiMon = inputResponse(entity.LoaiMon.ToString()),
                    TrangThai = inputResponse(entity.TrangThai.ToString())
                }
            };
        }

        public async Task<CreateCourseResponse> CreateCourseAsync(CreateCourseRequest req, string? currentUserLogin)
        {
            var maMon = NormalizeCode(req.MaMonHoc)!;
            var maGv = NormalizeCode(req.MaGiangVien)!;

            if (!req.MaHocKy.HasValue || req.MaHocKy.Value <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Mã học kỳ không hợp lệ.");

            // Tồn tại các thực thể liên quan
            var subject = await _repo.GetSubjectByCodeAsync(maMon)
                ?? throw ApiExceptionHelper.New(ApiErrorCode.BadRequest, "Môn học không tồn tại.");

            var lecturerOk = await _repo.LecturerExistsByCodeAsync(maGv);
            if (!lecturerOk) ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Giảng viên không tồn tại.");

            var semester = await _repo.GetSemesterByIdAsync(req.MaHocKy!.Value)
                ?? throw ApiExceptionHelper.New(ApiErrorCode.BadRequest, "Học kỳ không tồn tại.");

            // >>> Sinh mã lớp học phần: PREFIX-<NAMHOC>-K<KY>-STT
            var autoCode = await _repo.GenerateNextCourseCodeAsync(subject.TenMonHoc ?? subject.MaMonHoc!, semester.NamHoc, semester.Ky);

            // Check hi hữu
            if (await _repo.CourseExistsAsync(autoCode))
                ApiExceptionHelper.Throw(ApiErrorCode.Conflict, "Không thể sinh mã lớp học phần (bị trùng). Vui lòng thử lại.");

            var entity = new LopHocPhan
            {
                MaLopHocPhan = autoCode,
                TenLopHocPhan = req.TenLopHocPhan!.Trim(),
                TrangThai = req.TrangThai ?? true,
                MaMonHoc = maMon,
                MaGiangVien = maGv,
                MaHocKy = semester.MaHocKy
            };

            await _repo.AddCourseAsync(entity);
            await _repo.LogActivityAsync(currentUserLogin,
                $"Tạo lớp học phần: {entity.MaLopHocPhan} - {entity.TenLopHocPhan} (Môn: {entity.MaMonHoc}, GV: {entity.MaGiangVien}, HK: {entity.MaHocKy})");

            // Build response từ dữ liệu vừa tạo
            var (rows, _) = await _repo.SearchCoursesAsync(
                maLopHocPhan: entity.MaLopHocPhan,
                tenLopHocPhan: null, trangThai: null,
                maMonHoc: null, soTinChi: null,
                maGiangVien: null, maHocKy: null,
                namHoc: null, ky: null, maSinhVien: null,
                sortBy: null, desc: false, page: 1, pageSize: 1);

            var x = rows.First();

            return new CreateCourseResponse
            {
                LopHocPhan = new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(x.Lhp.MaLopHocPhan),
                    TenLopHocPhan = inputResponse(x.Lhp.TenLopHocPhan),
                    TrangThai = inputResponse(x.Lhp.TrangThai.ToString())
                },
                MonHoc = new MonHocDTO
                {
                    MaMonHoc = inputResponse(x.Mh.MaMonHoc),
                    TenMonHoc = inputResponse(x.Mh.TenMonHoc),
                    SoTinChi = inputResponse(x.Mh.SoTinChi.ToString()),
                    SoTiet = inputResponse(x.Mh.SoTiet.ToString()),
                    LoaiMon = inputResponse(x.Mh.LoaiMon.ToString())
                },
                GiangVien = new GiangVienDTO
                {
                    MaGiangVien = inputResponse(x.Gv?.MaGiangVien ?? "null")
                },
                HocKy = new HocKyDTO
                {
                    MaHocKy = inputResponse(x.Hk.MaHocKy.ToString()),
                    NamHoc = inputResponse(x.Hk.NamHoc.ToString()),
                    Ky = inputResponse(x.Hk.Ky.ToString())
                }
            };
        }

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
                HocKy = new HocKyDTO
                {
                    MaHocKy = inputResponse(x.MaHocKy.ToString()),
                    NamHoc = inputResponse(x.NamHoc.ToString()),
                    Ky = inputResponse(x.Ky.ToString())
                }
            }).ToList();

            return new PagedResult<SemesterListItem>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(pageSize.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)pageSize)).ToString()),
                Items = items
            };
        }

        public async Task<CreateSemesterResponse> CreateSemesterAsync(CreateSemesterRequest req, string? currentUserLogin)
        {
            if (!req.NamHoc.HasValue || !req.Ky.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu năm học/kỳ.");

            var namHoc = req.NamHoc!.Value;
            var ky = req.Ky!.Value;

            if (await _repo.ExistsSemesterAsync(namHoc, ky))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Học kỳ đã tồn tại.");

            var entity = new HocKy
            {
                NamHoc = namHoc,
                Ky = ky
            };

            await _repo.AddSemesterAsync(entity);
            await _repo.LogActivityAsync(currentUserLogin, $"Tạo học kỳ {entity.NamHoc} - Kỳ {entity.Ky}");

            return new CreateSemesterResponse
            {
                HocKy = new HocKyDTO
                {
                    MaHocKy = inputResponse(entity.MaHocKy.ToString()),
                    NamHoc = inputResponse(entity.NamHoc.ToString()),
                    Ky = inputResponse(entity.Ky.ToString())
                }
            };
        }

        public async Task<UpdateSemesterResponse> UpdateSemesterAsync(UpdateSemesterRequest req, string? currentUserLogin)
        {
            if (!req.MaHocKy.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu mã học kỳ.");

            var hk = await _repo.GetSemesterByIdAsync(req.MaHocKy!.Value);
            if (hk == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy học kỳ.");

            var newNamHoc = req.NamHoc ?? hk!.NamHoc;
            var newKy = req.Ky ?? hk!.Ky;

            if (await _repo.ExistsSemesterAsync(newNamHoc, newKy, excludeId: hk!.MaHocKy))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Học kỳ (năm học, kỳ) đã tồn tại.");

            hk.NamHoc = newNamHoc;
            hk.Ky = newKy;

            await _repo.UpdateSemesterAsync(hk);
            await _repo.LogActivityAsync(currentUserLogin, $"Cập nhật học kỳ {hk.MaHocKy}: {hk.NamHoc} - Kỳ {hk.Ky}");

            return new UpdateSemesterResponse
            {
                HocKy = new HocKyDTO
                {
                    MaHocKy = inputResponse(hk.MaHocKy.ToString()),
                    NamHoc = inputResponse(hk.NamHoc.ToString()),
                    Ky = inputResponse(hk.Ky.ToString())
                }
            };
        }

        public async Task<UpdateSubjectResponse> UpdateSubjectAsync(UpdateSubjectRequest req, string? currentUserLogin)
        {
            var code = NormalizeCode(req.MaMonHoc)!;
            var subject = await _repo.GetSubjectByCodeAsync(code);
            if (subject == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy môn học.");

            if (!string.IsNullOrWhiteSpace(req.TenMonHoc)) subject!.TenMonHoc = req.TenMonHoc!.Trim();
            if (req.SoTinChi.HasValue) subject!.SoTinChi = req.SoTinChi.Value;
            if (req.SoTiet.HasValue) subject!.SoTiet = req.SoTiet.Value;
            if (req.TrangThai.HasValue) subject!.TrangThai = req.TrangThai.Value;
            if (req.MoTa != null) subject!.MoTa = string.IsNullOrWhiteSpace(req.MoTa) ? null : req.MoTa!.Trim();

            await _repo.UpdateSubjectAsync(subject!);
            await _repo.LogActivityAsync(currentUserLogin, $"Cập nhật môn học: {subject!.MaMonHoc} - {subject.TenMonHoc}");

            return new UpdateSubjectResponse
            {
                MonHoc = new MonHocDTO
                {
                    MaMonHoc = inputResponse(subject.MaMonHoc),
                    TenMonHoc = inputResponse(subject.TenMonHoc),
                    SoTinChi = inputResponse(subject.SoTinChi.ToString()),
                    SoTiet = inputResponse(subject.SoTiet.ToString()),
                    MoTa = inputResponse(subject.MoTa ?? "null"),
                    TrangThai = inputResponse(subject.TrangThai.ToString()),
                    LoaiMon = inputResponse(subject.LoaiMon.ToString())
                }
            };
        }

        public async Task<UpdateCourseResponse> UpdateCourseAsync(UpdateCourseRequest req, string? currentUserLogin)
        {
            var code = NormalizeCode(req.MaLopHocPhan)!;
            var course = await _repo.GetCourseByCodeAsync(code);
            if (course == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy lớp học phần.");

            if (!string.IsNullOrWhiteSpace(req.MaMonHoc))
            {
                var maMon = NormalizeCode(req.MaMonHoc)!;
                var ok = await _repo.SubjectExistsAsync(maMon);
                if (!ok) ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Môn học không tồn tại.");
                course!.MaMonHoc = maMon;
            }

            if (!string.IsNullOrWhiteSpace(req.MaGiangVien))
            {
                var maGv = NormalizeCode(req.MaGiangVien)!;
                var ok = await _repo.LecturerExistsByCodeAsync(maGv);
                if (!ok) ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Giảng viên không tồn tại.");
                course!.MaGiangVien = maGv;
            }

            if (req.MaHocKy.HasValue)
            {
                if (req.MaHocKy.Value <= 0)
                    ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Mã học kỳ không hợp lệ.");
                var hkOk = await _repo.SemesterExistsByIdAsync(req.MaHocKy.Value);
                if (!hkOk) ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Học kỳ không tồn tại.");
                course!.MaHocKy = req.MaHocKy.Value;
            }

            if (!string.IsNullOrWhiteSpace(req.TenLopHocPhan)) course!.TenLopHocPhan = req.TenLopHocPhan!.Trim();
            if (req.TrangThai.HasValue) course!.TrangThai = req.TrangThai.Value;

            await _repo.UpdateCourseAsync(course!);
            await _repo.LogActivityAsync(currentUserLogin, $"Cập nhật lớp học phần: {course!.MaLopHocPhan} - {course.TenLopHocPhan}");

            // Build response (chỉ các field theo comment)
            var (rows, _) = await _repo.SearchCoursesAsync(maLopHocPhan: course.MaLopHocPhan,
                tenLopHocPhan: null, trangThai: null, maMonHoc: null, soTinChi: null,
                maGiangVien: null, maHocKy: null, namHoc: null, ky: null,
                maSinhVien: null, sortBy: null, desc: false, page: 1, pageSize: 1);

            var x = rows.First();

            return new UpdateCourseResponse
            {
                LopHocPhan = new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(x.Lhp.MaLopHocPhan),
                    TenLopHocPhan = inputResponse(x.Lhp.TenLopHocPhan),
                    TrangThai = inputResponse(x.Lhp.TrangThai.ToString())
                },
                MonHoc = new MonHocDTO
                {
                    MaMonHoc = inputResponse(x.Mh.MaMonHoc),
                    TenMonHoc = inputResponse(x.Mh.TenMonHoc)
                },
                GiangVien = new GiangVienDTO
                {
                    MaGiangVien = inputResponse(x.Gv?.MaGiangVien ?? "null")
                },
                HocKy = new HocKyDTO
                {
                    MaHocKy = inputResponse(x.Hk.MaHocKy.ToString()),
                    NamHoc = inputResponse(x.Hk.NamHoc.ToString()),
                    Ky = inputResponse(x.Hk.Ky.ToString())
                }
            };
        }
    }
}
