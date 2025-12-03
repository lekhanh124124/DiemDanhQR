// File: Services/Implementations/StudentService.cs
using System.ComponentModel;
using api.DTOs;
using api.ErrorHandling;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using api.Services.Interfaces;
using OfficeOpenXml;
namespace api.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IPermissionRepository _permRepo;
        private readonly IAcademicRepository _academicRepo;
        private readonly IWebHostEnvironment _env;

        public StudentService(
            IStudentRepository repo,
            IUserRepository userRepo,
            IPermissionRepository permRepo,
            IAcademicRepository academicRepo,
            IWebHostEnvironment env)
        {
            _repo = repo;
            _userRepo = userRepo;
            _permRepo = permRepo;
            _academicRepo = academicRepo;
            _env = env;
        }

        // ========= Helpers =========
        private static string? FDateOnly(DateOnly? d) => d.HasValue ? d.Value.ToString("yyyy-MM-dd") : null;
        private static DateOnly? ToDateOnly(DateTime? dt) => dt.HasValue ? DateOnly.FromDateTime(dt.Value) : null;

        // Bắt buộc dùng cho mọi field trong response
        private string inputResponse(string input) => input ?? "null";

        public async Task<CreateStudentResponse> CreateAsync(CreateStudentRequest request)
        {
            // Lấy role mặc định cho SV
            var role = await _permRepo.GetRoleByCodeAsync("SV");
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Không tìm thấy quyền sinh viên (SV).");

            // Lấy ngành để có CodeNganh và liên kết Khoa
            if (!request.MaNganh.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã ngành là bắt buộc.");

            var ng = await _academicRepo.GetNganhByIdAsync(request.MaNganh.Value);
            if (ng == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy ngành.");

            var kh = await _academicRepo.GetKhoaByIdAsync(ng.MaKhoa);

            // Xác định năm nhập học
            var year = request.NamNhapHoc ?? TimeHelper.UtcToVietnam(DateTime.UtcNow).Year;

            // Generate MaSinhVien = CodeNganh + YY + STT
            var maSV = await _repo.GenerateNextMaSinhVienAsync(ng.CodeNganh, year);

            // Lưu avatar (nếu có)
            string? avatarUrl = await AvatarHelper.SaveAvatarAsync(request.AnhDaiDien, _env.WebRootPath, maSV);

            // Tạo tài khoản: TenDangNhap = MaSinhVien, mật khẩu = hash(MaSinhVien)
            var existed = await _repo.GetUserByUsernameAsync(maSV);
            if (existed != null)
                ApiExceptionHelper.Throw(ApiErrorCode.Conflict, "Tên đăng nhập đã tồn tại.");

            var user = new NguoiDung
            {
                TenDangNhap = maSV,
                HoTen = string.IsNullOrWhiteSpace(request.HoTen) ? maSV : request.HoTen!.Trim(),
                MatKhau = BCrypt.Net.BCrypt.HashPassword(maSV),
                TrangThai = true,
                MaQuyen = role.MaQuyen,

                Email = request.Email,
                SoDienThoai = request.SoDienThoai,
                NgaySinh = ToDateOnly(request.NgaySinh),
                GioiTinh = request.GioiTinh,
                DiaChi = request.DiaChi,
                AnhDaiDien = avatarUrl
            };
            await _repo.AddUserAsync(user);
            await _repo.SaveChangesAsync(); // lấy MaNguoiDung

            // Thêm bản ghi SinhVien
            var sv = new SinhVien
            {
                MaNguoiDung = user.MaNguoiDung,
                MaSinhVien = maSV,
                NamNhapHoc = year,
                MaNganh = request.MaNganh
            };
            await _repo.AddStudentAsync(sv);
            await _repo.SaveChangesAsync();

            // Trả response
            return new CreateStudentResponse
            {
                NguoiDung = new NguoiDungDTO
                {
                    MaNguoiDung = inputResponse(user.MaNguoiDung.ToString()),
                    HoTen = inputResponse(user.HoTen),
                    GioiTinh = inputResponse(user.GioiTinh?.ToString()),
                    AnhDaiDien = inputResponse(user.AnhDaiDien),
                    Email = inputResponse(user.Email),
                    SoDienThoai = inputResponse(user.SoDienThoai),
                    NgaySinh = inputResponse(FDateOnly(user.NgaySinh)),
                    DiaChi = inputResponse(user.DiaChi),
                    TenDangNhap = inputResponse(user.TenDangNhap),
                    TrangThai = inputResponse(user.TrangThai.ToString())
                },
                SinhVien = new SinhVienDTO
                {
                    MaSinhVien = inputResponse(sv.MaSinhVien),
                    NamNhapHoc = inputResponse(sv.NamNhapHoc.ToString())
                },
                Nganh = new NganhDTO
                {
                    MaNganh = inputResponse(ng.MaNganh.ToString()),
                    CodeNganh = inputResponse(ng.CodeNganh),
                    TenNganh = inputResponse(ng.TenNganh)
                },
                Khoa = new KhoaDTO
                {
                    MaKhoa = inputResponse(kh?.MaKhoa.ToString()),
                    CodeKhoa = inputResponse(kh?.CodeKhoa),
                    TenKhoa = inputResponse(kh?.TenKhoa)
                },
                PhanQuyen = new PhanQuyenDTO
                {
                    MaQuyen = inputResponse(role.MaQuyen.ToString()),
                    CodeQuyen = inputResponse(role.CodeQuyen),
                    TenQuyen = inputResponse(role.TenQuyen),
                    MoTa = inputResponse(role.MoTa)
                }
            };
        }

        public async Task<PagedResult<StudentListItemResponse>> GetListAsync(GetStudentsRequest request)
        {
            var page = request.Page.GetValueOrDefault(1);
            var pageSize = request.PageSize.GetValueOrDefault(20);
            var sortBy = request.SortBy ?? "HoTen";
            var desc = string.Equals(request.SortDir, "DESC", StringComparison.OrdinalIgnoreCase);

            var (items, total) = await _repo.SearchStudentsAsync(
                maKhoa: request.MaKhoa,
                maNganh: request.MaNganh,
                namNhapHoc: request.NamNhapHoc,
                trangThaiUser: request.TrangThaiUser,
                maLopHocPhan: request.MaLopHocPhan,
                sortBy: sortBy,
                desc: desc,
                page: page,
                pageSize: pageSize,
                maSinhVien: request.MaSinhVien,
                hoTen: request.HoTen // NEW
            );


            string F(DateOnly? d) => d.HasValue ? d.Value.ToString("yyyy-MM-dd") : null!;
            var list = items.Select(x => new StudentListItemResponse
            {
                NguoiDung = new NguoiDungDTO
                {
                    MaNguoiDung = inputResponse(x.Nd.MaNguoiDung.ToString()),
                    HoTen = inputResponse(x.Nd.HoTen),
                    TenDangNhap = inputResponse(x.Nd.TenDangNhap),
                    TrangThai = inputResponse(x.Nd.TrangThai.ToString())
                },
                SinhVien = new SinhVienDTO
                {
                    MaSinhVien = inputResponse(x.Sv.MaSinhVien),
                    NamNhapHoc = inputResponse(x.Sv.NamNhapHoc.ToString())
                },
                Nganh = new NganhDTO
                {
                    MaNganh = inputResponse(x.Ng?.MaNganh.ToString()),
                    CodeNganh = inputResponse(x.Ng?.CodeNganh),
                    TenNganh = inputResponse(x.Ng?.TenNganh)
                },
                Khoa = new KhoaDTO
                {
                    MaKhoa = inputResponse(x.Kh?.MaKhoa.ToString()),
                    CodeKhoa = inputResponse(x.Kh?.CodeKhoa),
                    TenKhoa = inputResponse(x.Kh?.TenKhoa)
                },
                ThamGiaLop = string.IsNullOrWhiteSpace(request.MaLopHocPhan)
                    ? null
                    : new ThamGiaLopDTO
                    {
                        NgayThamGia = inputResponse(F(x.NgayTG)),
                        TrangThai = inputResponse(x.TrangThaiTG?.ToString())
                    }
            }).ToList();

            return new PagedResult<StudentListItemResponse>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(pageSize.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(Math.Ceiling(total / (double)pageSize).ToString()),
                Items = list
            };
        }

        public async Task<UpdateStudentResponse> UpdateAsync(UpdateStudentRequest request)
        {
            var maSV = (request.MaSinhVien ?? "").Trim();
            if (string.IsNullOrWhiteSpace(maSV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã sinh viên không được để trống.");

            var sv = await _repo.GetStudentByMaSinhVienAsync(maSV);
            if (sv == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy sinh viên.");

            var user = await _repo.GetUserByIdAsync(sv.MaNguoiDung);
            if (user == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy người dùng.");

            // Update User
            if (request.TrangThai.HasValue) user.TrangThai = request.TrangThai.Value;
            if (!string.IsNullOrWhiteSpace(request.TenSinhVien)) user.HoTen = request.TenSinhVien!.Trim();

            if (request.GioiTinh.HasValue) user.GioiTinh = request.GioiTinh;
            if (!string.IsNullOrWhiteSpace(request.Email)) user.Email = request.Email!.Trim();
            if (!string.IsNullOrWhiteSpace(request.SoDienThoai)) user.SoDienThoai = request.SoDienThoai!.Trim();
            if (request.NgaySinh.HasValue) user.NgaySinh = ToDateOnly(request.NgaySinh);
            if (!string.IsNullOrWhiteSpace(request.DiaChi)) user.DiaChi = request.DiaChi!.Trim();

            // Avatar (nếu có)
            if (request.AnhDaiDien != null)
            {
                // Xóa ảnh cũ
                if (!string.IsNullOrWhiteSpace(user.AnhDaiDien))
                {
                    var webRoot = _env.WebRootPath ?? "";
                    try
                    {
                        var relative = user.AnhDaiDien!;
                        if (Uri.TryCreate(relative, UriKind.Absolute, out var uriAbs))
                            relative = uriAbs.LocalPath;

                        var physical = Path.Combine(webRoot, relative.TrimStart('/', '\\'));
                        if (File.Exists(physical)) File.Delete(physical);
                    }
                    catch { }
                }

                // Lưu ảnh mới
                var newUrl = await AvatarHelper.SaveAvatarAsync(request.AnhDaiDien, _env.WebRootPath, user.TenDangNhap ?? maSV);
                if (!string.IsNullOrWhiteSpace(newUrl)) user.AnhDaiDien = newUrl;
            }

            await _repo.UpdateUserAsync(user);

            // Update SV
            if (request.NamNhapHoc.HasValue) sv.NamNhapHoc = request.NamNhapHoc.Value;
            if (request.MaNganh.HasValue) sv.MaNganh = request.MaNganh.Value;

            await _repo.UpdateStudentAsync(sv);

            // Log (ghi DB: VN time, không format)
            await _repo.AddActivityAsync(new LichSuHoatDong
            {
                MaNguoiDung = user.MaNguoiDung,
                HanhDong = $"Cập nhật thông tin sinh viên [{sv.MaSinhVien}]",
                ThoiGian = TimeHelper.UtcToVietnam(DateTime.UtcNow)
            });
            await _repo.SaveChangesAsync();

            // Liên kết
            var role = await _permRepo.GetRoleByIdAsync((int)user.MaQuyen!);
            Nganh? ng = null; Khoa? kh = null;
            if (sv.MaNganh.HasValue)
            {
                ng = await _academicRepo.GetNganhByIdAsync(sv.MaNganh.Value);
                if (ng != null) kh = await _academicRepo.GetKhoaByIdAsync(ng.MaKhoa);
            }

            return new UpdateStudentResponse
            {
                NguoiDung = new NguoiDungDTO
                {
                    MaNguoiDung = inputResponse(user.MaNguoiDung.ToString()),
                    HoTen = inputResponse(user.HoTen),
                    GioiTinh = inputResponse(user.GioiTinh?.ToString()),
                    AnhDaiDien = inputResponse(user.AnhDaiDien),
                    Email = inputResponse(user.Email),
                    SoDienThoai = inputResponse(user.SoDienThoai),
                    NgaySinh = inputResponse(FDateOnly(user.NgaySinh)),
                    DiaChi = inputResponse(user.DiaChi),
                    TenDangNhap = inputResponse(user.TenDangNhap),
                    TrangThai = inputResponse(user.TrangThai.ToString())
                },
                SinhVien = new SinhVienDTO
                {
                    MaSinhVien = inputResponse(sv.MaSinhVien),
                    NamNhapHoc = inputResponse(sv.NamNhapHoc.ToString())
                },
                Nganh = new NganhDTO
                {
                    MaNganh = inputResponse(ng.MaNganh.ToString()),
                    CodeNganh = inputResponse(ng.CodeNganh),
                    TenNganh = inputResponse(ng.TenNganh)
                },
                Khoa = new KhoaDTO
                {
                    MaKhoa = inputResponse(kh.MaKhoa.ToString()),
                    CodeKhoa = inputResponse(kh.CodeKhoa),
                    TenKhoa = inputResponse(kh.TenKhoa)
                },
                PhanQuyen = new PhanQuyenDTO
                {
                    MaQuyen = inputResponse(role.MaQuyen.ToString()),
                    CodeQuyen = inputResponse(role.CodeQuyen),
                    TenQuyen = inputResponse(role.TenQuyen),
                    MoTa = inputResponse(role.MoTa)
                }
            };
        }

        public async Task<AddStudentToCourseResponse> AddStudentToCourseAsync(AddStudentToCourseRequest req, string? currentUserTenDangNhap)
        {
            var maLhp = (req.MaLopHocPhan ?? "").Trim();
            var maSv = (req.MaSinhVien ?? "").Trim();

            if (string.IsNullOrWhiteSpace(maLhp) || string.IsNullOrWhiteSpace(maSv))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu mã lớp học phần hoặc mã sinh viên.");

            if (!await _repo.CourseExistsAsync(maLhp))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Lớp học phần không tồn tại.");

            if (!await _repo.ExistsStudentAsync(maSv))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Sinh viên không tồn tại.");

            if (await _repo.ParticipationExistsAsync(maLhp, maSv))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Sinh viên đã tham gia lớp học phần này.");

            var ngay = req.NgayThamGia ?? DateOnly.FromDateTime(TimeHelper.UtcToVietnam(DateTime.UtcNow));
            var entity = new ThamGiaLop
            {
                MaLopHocPhan = maLhp,
                MaSinhVien = maSv,
                NgayThamGia = ngay,
                TrangThai = req.TrangThai ?? true
            };

            await _repo.AddParticipationAsync(entity);

            // Log
            if (!string.IsNullOrWhiteSpace(currentUserTenDangNhap))
            {
                var user = await _repo.GetUserByUsernameAsync(currentUserTenDangNhap!);
                if (user != null)
                {
                    await _repo.AddActivityAsync(new LichSuHoatDong
                    {
                        MaNguoiDung = user.MaNguoiDung,
                        HanhDong = $"Thêm sinh viên {entity.MaSinhVien} vào lớp {entity.MaLopHocPhan}",
                        ThoiGian = TimeHelper.UtcToVietnam(DateTime.UtcNow)
                    });
                    await _repo.SaveChangesAsync();
                }
            }

            return new AddStudentToCourseResponse
            {
                NguoiDung = new NguoiDungDTO
                {
                    // Không bắt buộc xuất thêm, để "null" theo format yêu cầu
                    HoTen = inputResponse(null)
                },
                SinhVien = new SinhVienDTO
                {
                    MaSinhVien = inputResponse(entity.MaSinhVien),
                    NamNhapHoc = inputResponse(null)
                },
                LopHocPhan = new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(entity.MaLopHocPhan),
                    TenLopHocPhan = inputResponse(null),
                    TrangThai = inputResponse(null)
                },
                ThamGiaLop = new ThamGiaLopDTO
                {
                    NgayThamGia = inputResponse(FDateOnly(entity.NgayThamGia)),
                    TrangThai = inputResponse(entity.TrangThai.ToString())
                }
            };
        }

        public async Task<RemoveStudentFromCourseResponse> RemoveStudentFromCourseAsync(RemoveStudentFromCourseRequest req, string? currentUserTenDangNhap)
        {
            var maLhp = (req.MaLopHocPhan ?? "").Trim();
            var maSv = (req.MaSinhVien ?? "").Trim();

            if (string.IsNullOrWhiteSpace(maLhp) || string.IsNullOrWhiteSpace(maSv))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu mã lớp học phần hoặc mã sinh viên.");

            var thamGia = await _repo.GetParticipationAsync(maLhp, maSv);
            if (thamGia == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy bản ghi tham gia lớp học phần.");

            thamGia.TrangThai = false; // soft remove
            await _repo.UpdateParticipationAsync(thamGia);

            if (!string.IsNullOrWhiteSpace(currentUserTenDangNhap))
            {
                var user = await _repo.GetUserByUsernameAsync(currentUserTenDangNhap!);
                if (user != null)
                {
                    await _repo.AddActivityAsync(new LichSuHoatDong
                    {
                        MaNguoiDung = user.MaNguoiDung,
                        HanhDong = $"Gỡ sinh viên {thamGia.MaSinhVien} khỏi lớp {thamGia.MaLopHocPhan}",
                        ThoiGian = TimeHelper.UtcToVietnam(DateTime.UtcNow)
                    });
                    await _repo.SaveChangesAsync();
                }
            }

            return new RemoveStudentFromCourseResponse
            {
                NguoiDung = new NguoiDungDTO
                {
                    HoTen = inputResponse(null)
                },
                SinhVien = new SinhVienDTO
                {
                    MaSinhVien = inputResponse(thamGia.MaSinhVien),
                    NamNhapHoc = inputResponse(null)
                },
                LopHocPhan = new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(thamGia.MaLopHocPhan),
                    TenLopHocPhan = inputResponse(null),
                    TrangThai = inputResponse(null)
                },
                ThamGiaLop = new ThamGiaLopDTO
                {
                    NgayThamGia = inputResponse(FDateOnly(thamGia.NgayThamGia)),
                    TrangThai = inputResponse(thamGia.TrangThai.ToString())
                }
            };
        }
        public async Task<BulkImportStudentsResponse> BulkImportAsync(BulkImportStudentsRequest req)
        {
            if (req.File == null || req.File.Length <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu file Excel.");

            var role = await _permRepo.GetRoleByCodeAsync("SV")
                       ?? throw ApiExceptionHelper.New(ApiErrorCode.ValidationError, "Không tìm thấy quyền SV.");

            // EPPlus 8+: đặt license trước khi khởi tạo ExcelPackage
            ExcelPackage.License.SetNonCommercialPersonal("lekhanh8224");

            using var ms = new MemoryStream();
            await req.File.CopyToAsync(ms);
            ms.Position = 0;

            int success = 0, failed = 0;
            var failedDetails = new List<RowError>();

            // ====== Caches để giảm query ======
            var nganhById = new Dictionary<int, Nganh>();
            var nganhByCode = new Dictionary<string, Nganh>(StringComparer.OrdinalIgnoreCase);

            using var pkg = new ExcelPackage(ms);
            var ws = pkg.Workbook.Worksheets.FirstOrDefault()
                     ?? throw ApiExceptionHelper.New(ApiErrorCode.ValidationError, "Không tìm thấy sheet trong Excel.");

            // ----- Đọc header -----
            var header = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var colMax = ws.Dimension.End.Column;
            for (int c = 1; c <= colMax; c++)
            {
                var name = ws.Cells[1, c].Text?.Trim();
                if (!string.IsNullOrWhiteSpace(name) && !header.ContainsKey(name)) header[name] = c;
            }

            int Col(string name) => header.TryGetValue(name, out var ci) ? ci : -1;

            // Chấp nhận 2 phương án: MaNganh hoặc CodeNganh (chỉ cần 1 trong 2)
            var hasMaNganh = Col("MaNganh") > 0;
            var hasCodeNganh = Col("CodeNganh") > 0;

            // Các cột còn lại
            foreach (var col in new[] { "HoTen" })
                if (Col(col) <= 0)
                    ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, $"Thiếu cột '{col}'.");

            if (!hasMaNganh && !hasCodeNganh)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu cột 'MaNganh' hoặc 'CodeNganh'.");

            var rowMax = ws.Dimension.End.Row;

            for (int r = 2; r <= rowMax; r++)
            {
                try
                {
                    string? hoTen = ExcelStudentHelper.CleanString(Col("HoTen") > 0 ? ws.Cells[r, Col("HoTen")].Text : null);
                    if (string.IsNullOrWhiteSpace(hoTen))
                        throw new Exception("HoTen trống.");

                    byte? gioiTinh = ExcelStudentHelper.ParseGender(Col("GioiTinh") > 0 ? ws.Cells[r, Col("GioiTinh")].Text : null);

                    string? email = ExcelStudentHelper.CleanString(Col("Email") > 0 ? ws.Cells[r, Col("Email")].Text : null);
                    if (!string.IsNullOrEmpty(email) && !ExcelStudentHelper.IsValidEmail(email))
                        throw new Exception("Email không hợp lệ.");

                    string? sdt = ExcelStudentHelper.NormalizePhone(Col("SoDienThoai") > 0 ? ws.Cells[r, Col("SoDienThoai")].Text : null);
                    if ((Col("SoDienThoai") > 0 ? ws.Cells[r, Col("SoDienThoai")].Text?.Length : 0) > 0 && sdt == null)
                        throw new Exception("Số điện thoại không hợp lệ.");

                    DateOnly? ngaySinh = ExcelStudentHelper.ParseDate(Col("NgaySinh") > 0 ? ws.Cells[r, Col("NgaySinh")].Text : null);
                    string? diaChi = ExcelStudentHelper.CleanString(Col("DiaChi") > 0 ? ws.Cells[r, Col("DiaChi")].Text : null);

                    int namNhapHoc;
                    var rawYear = ExcelStudentHelper.CleanString(Col("NamNhapHoc") > 0 ? ws.Cells[r, Col("NamNhapHoc")].Text : null);
                    if (!string.IsNullOrEmpty(rawYear) && int.TryParse(rawYear, out var y) && y > 1900 && y < 3000)
                        namNhapHoc = y;
                    else
                        namNhapHoc = req.DefaultNamNhapHoc ?? TimeHelper.UtcToVietnam(DateTime.UtcNow).Year;

                    // ======= Resolve NGÀNH: ưu tiên MaNganh, fallback CodeNganh =======
                    Nganh nganh;

                    if (hasMaNganh)
                    {
                        var rawMaNganh = ExcelStudentHelper.CleanString(ws.Cells[r, Col("MaNganh")].Text);
                        if (!string.IsNullOrWhiteSpace(rawMaNganh) && int.TryParse(rawMaNganh, out var maNgInt))
                        {
                            if (!nganhById.TryGetValue(maNgInt, out nganh!))
                            {
                                nganh = await _academicRepo.GetNganhByIdAsync(maNgInt)
                                        ?? throw new Exception($"Không tìm thấy ngành với MaNganh={maNgInt}.");
                                nganhById[maNgInt] = nganh;
                            }
                        }
                        else
                        {
                            // nếu MaNganh trống, thử CodeNganh (nếu có)
                            nganh = null!;
                        }
                    }
                    else nganh = null!;

                    if (nganh == null && hasCodeNganh)
                    {
                        var codeNganh = ExcelStudentHelper.CleanString(ws.Cells[r, Col("CodeNganh")].Text);
                        if (string.IsNullOrWhiteSpace(codeNganh))
                            throw new Exception("Thiếu MaNganh/CodeNganh.");

                        if (!nganhByCode.TryGetValue(codeNganh!, out nganh!))
                        {
                            nganh = await _academicRepo.GetNganhByCodeAsync(codeNganh!)
                                    ?? throw new Exception($"Không tìm thấy ngành với CodeNganh={codeNganh}.");
                            nganhByCode[codeNganh!] = nganh;
                            nganhById[nganh.MaNganh] = nganh; // đồng bộ cache
                        }
                    }

                    if (nganh == null)
                        throw new Exception("Thiếu thông tin ngành.");

                    // ======= Mã SV (ưu tiên cột, trống thì auto-gen theo CodeNganh) =======
                    string? maSV = ExcelStudentHelper.CleanString(Col("MaSinhVien") > 0 ? ws.Cells[r, Col("MaSinhVien")].Text : null);
                    if (string.IsNullOrEmpty(maSV))
                    {
                        var codeForId = nganh.CodeNganh ?? string.Empty;
                        if (string.IsNullOrEmpty(codeForId))
                            throw new Exception($"Ngành (MaNganh={nganh.MaNganh}) không có CodeNganh để sinh mã SV.");
                        maSV = await _repo.GenerateNextMaSinhVienAsync(codeForId, namNhapHoc);
                    }
                    else
                    {
                        if (await _repo.ExistsStudentAsync(maSV))
                            throw new Exception($"Mã sinh viên đã tồn tại: {maSV}");
                    }

                    if (!string.IsNullOrEmpty(email) && await _repo.ExistsUserByEmailAsync(email))
                        throw new Exception($"Email đã tồn tại: {email}");
                    // if (!string.IsNullOrEmpty(sdt) && await _repo.ExistsUserByPhoneAsync(sdt))
                    //     throw new Exception($"Số điện thoại đã tồn tại: {sdt}");

                    // ======= Tạo User =======
                    var user = new NguoiDung
                    {
                        TenDangNhap = maSV,
                        HoTen = hoTen,
                        MatKhau = BCrypt.Net.BCrypt.HashPassword(maSV),
                        TrangThai = true,
                        MaQuyen = role.MaQuyen,
                        GioiTinh = gioiTinh,
                        Email = email,
                        SoDienThoai = sdt,
                        NgaySinh = ngaySinh,
                        DiaChi = diaChi
                    };
                    await _repo.AddUserAsync(user);
                    await _repo.SaveChangesAsync();

                    // ======= Tạo SinhVien (ghi MaNganh) =======
                    var sv = new SinhVien
                    {
                        MaNguoiDung = user.MaNguoiDung,
                        MaSinhVien = maSV,
                        NamNhapHoc = namNhapHoc,
                        MaNganh = nganh.MaNganh
                    };
                    await _repo.AddStudentAsync(sv);

                    // Log
                    await _repo.AddActivityAsync(new LichSuHoatDong
                    {
                        MaNguoiDung = user.MaNguoiDung,
                        HanhDong = $"Import sinh viên [{sv.MaSinhVien}]",
                        ThoiGian = TimeHelper.UtcToVietnam(DateTime.UtcNow)
                    });

                    await _repo.SaveChangesAsync();
                    success++;
                }
                catch (Exception ex)
                {
                    failed++;
                    failedDetails.Add(new RowError { Row = r, Error = ex.Message });
                }
            }

            return new BulkImportStudentsResponse
            {
                SuccessCount = success,
                FailedCount = failed,
                FailedDetails = failedDetails
            };
        }
        public async Task<BulkImportStudentsResponse> BulkAddStudentsToCourseAsync(BulkAddStudentsToCourseRequest req, string? currentUserTenDangNhap)
        {
            var maLhp = (req.MaLopHocPhan ?? "").Trim();
            if (string.IsNullOrWhiteSpace(maLhp))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu mã lớp học phần.");

            if (!await _repo.CourseExistsAsync(maLhp))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Lớp học phần không tồn tại.");

            if (req.File == null || req.File.Length <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu file Excel.");

            // EPPlus 8: đặt license trước khi mở
            ExcelPackage.License.SetNonCommercialPersonal("lekhanh8224");

            using var ms = new MemoryStream();
            await req.File.CopyToAsync(ms);
            ms.Position = 0;

            using var pkg = new ExcelPackage(ms);
            var ws = pkg.Workbook.Worksheets.FirstOrDefault()
                     ?? throw ApiExceptionHelper.New(ApiErrorCode.ValidationError, "Không tìm thấy sheet trong Excel.");

            // Map header
            var header = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var colMax = ws.Dimension.End.Column;
            for (int c = 1; c <= colMax; c++)
            {
                var name = ws.Cells[1, c].Text?.Trim();
                if (!string.IsNullOrWhiteSpace(name) && !header.ContainsKey(name)) header[name] = c;
            }
            int Col(string name) => header.TryGetValue(name, out var ci) ? ci : -1;

            // Bắt buộc: MaSinhVien
            if (Col("MaSinhVien") <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu cột 'MaSinhVien'.");

            var rowMax = ws.Dimension.End.Row;

            int success = 0, failed = 0;
            var failedDetails = new List<RowError>();

            // Lấy default
            DateOnly defaultNgayTG = req.DefaultNgayThamGia ?? DateOnly.FromDateTime(TimeHelper.UtcToVietnam(DateTime.UtcNow));
            bool defaultTrangThai = req.DefaultTrangThai ?? true;

            for (int r = 2; r <= rowMax; r++)
            {
                try
                {
                    // MaSinhVien (bắt buộc)
                    var maSv = ExcelStudentHelper.CleanString(ws.Cells[r, Col("MaSinhVien")].Text);
                    if (string.IsNullOrWhiteSpace(maSv))
                        throw new Exception("MaSinhVien trống.");

                    // Kiểm tra SV tồn tại
                    if (!await _repo.ExistsStudentAsync(maSv))
                        throw new Exception($"Sinh viên không tồn tại: {maSv}");

                    // Nếu đã tồn tại tham gia lớp → coi như lỗi (tránh trùng)
                    if (await _repo.ParticipationExistsAsync(maLhp, maSv))
                        throw new Exception($"Sinh viên đã tham gia lớp: {maSv}");

                    // Ngày tham gia (tuỳ chọn)
                    DateOnly? ngayTG = null;
                    if (Col("NgayThamGia") > 0)
                        ngayTG = ExcelStudentHelper.ParseDate(ws.Cells[r, Col("NgayThamGia")].Text);
                    var ngayUse = ngayTG ?? defaultNgayTG;

                    // Trạng thái (tuỳ chọn)
                    bool? st = null;
                    if (Col("TrangThai") > 0)
                        st = ExcelStudentHelper.ParseBool(ws.Cells[r, Col("TrangThai")].Text);
                    var trangThaiUse = st ?? defaultTrangThai;

                    // Tạo bản ghi tham gia
                    var entity = new ThamGiaLop
                    {
                        MaLopHocPhan = maLhp,
                        MaSinhVien = maSv,
                        NgayThamGia = ngayUse,
                        TrangThai = trangThaiUse
                    };

                    await _repo.AddParticipationAsync(entity);

                    // Log người thực hiện (nếu có)
                    if (!string.IsNullOrWhiteSpace(currentUserTenDangNhap))
                    {
                        var user = await _repo.GetUserByUsernameAsync(currentUserTenDangNhap!);
                        if (user != null)
                        {
                            await _repo.AddActivityAsync(new LichSuHoatDong
                            {
                                MaNguoiDung = user.MaNguoiDung,
                                HanhDong = $"Bulk thêm SV {maSv} vào lớp {maLhp}",
                                ThoiGian = TimeHelper.UtcToVietnam(DateTime.UtcNow)
                            });
                            await _repo.SaveChangesAsync();
                        }
                    }

                    success++;
                }
                catch (Exception ex)
                {
                    failed++;
                    failedDetails.Add(new RowError { Row = r, Error = ex.Message });
                }
            }

            return new BulkImportStudentsResponse
            {
                SuccessCount = success,
                FailedCount = failed,
                FailedDetails = failedDetails
            };
        }

    }
}
