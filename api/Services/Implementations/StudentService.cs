// File: Services/Implementations/StudentService.cs
using api.DTOs;
using api.ErrorHandling;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using api.Services.Interfaces;

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
            var maSV = (request.MaSinhVien ?? "").Trim();
            if (string.IsNullOrWhiteSpace(maSV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã sinh viên không hợp lệ.");

            if (await _repo.ExistsStudentAsync(maSV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã sinh viên đã tồn tại.");

            // Lấy role mặc định cho sinh viên theo CodeQuyen = "SV"
            var role = await _permRepo.GetRoleByCodeAsync("SV");
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Không tìm thấy quyền sinh viên (SV).");

            // Save avatar (nếu có)
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
            await _repo.SaveChangesAsync(); // có MaNguoiDung

            // Thông tin SV
            var sv = new SinhVien
            {
                MaNguoiDung = user.MaNguoiDung,
                MaSinhVien = maSV,
                NamNhapHoc = request.NamNhapHoc ?? DateTime.UtcNow.Year,
                MaNganh = request.MaNganh
            };
            await _repo.AddStudentAsync(sv);
            await _repo.SaveChangesAsync();

            // Liên kết ngành/khoa (theo AppDbContext: SinhVien.MaNganh -> Nganh.MaKhoa)
            Nganh? ng = null; Khoa? kh = null;
            if (sv.MaNganh.HasValue)
            {
                ng = await _academicRepo.GetNganhByIdAsync(sv.MaNganh.Value);
                if (ng != null) kh = await _academicRepo.GetKhoaByIdAsync(ng.MaKhoa);
            }

            // Trả response: mọi field đi qua inputResponse
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
                pageSize: pageSize
            );

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
                    MaNganh = inputResponse(x.Ng.MaNganh.ToString()),
                    CodeNganh = inputResponse(x.Ng.CodeNganh),
                    TenNganh = inputResponse(x.Ng.TenNganh)
                },
                Khoa = new KhoaDTO
                {
                    MaKhoa = inputResponse(x.Kh.MaKhoa.ToString()),
                    CodeKhoa = inputResponse(x.Kh.CodeKhoa),
                    TenKhoa = inputResponse(x.Kh.TenKhoa)
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
            var role = await _permRepo.GetRoleByIdAsync(user.MaQuyen);
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
    }
}
// File:
