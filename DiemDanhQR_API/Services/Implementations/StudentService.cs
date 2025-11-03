// File: Services/Implementations/StudentService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Models;
using DiemDanhQR_API.Repositories.Interfaces;
using DiemDanhQR_API.Helpers;
using DiemDanhQR_API.Services.Interfaces;

namespace DiemDanhQR_API.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repo;
        private readonly IWebHostEnvironment _env;

        public StudentService(IStudentRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        public async Task<CreateStudentResponse> CreateAsync(CreateStudentRequest request)
        {
            var maSV = HelperFunctions.NormalizeCode(request.MaSinhVien);
            if (string.IsNullOrWhiteSpace(maSV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã sinh viên không hợp lệ.");

            if (request.MaQuyen <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã quyền không hợp lệ.");

            var role = await _repo.GetRoleAsync(request.MaQuyen);
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");

            if (await _repo.ExistsStudentAsync(maSV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã sinh viên đã tồn tại.");

            // Avatar
            string? avatarUrl = null;
            if (request.AnhDaiDien != null)
            {
                avatarUrl = await AvatarHelper.SaveAvatarAsync(request.AnhDaiDien, _env.WebRootPath, maSV);
            }

            // Tài khoản: TenDangNhap = MaSinhVien
            var user = await _repo.GetUserByUsernameAsync(maSV);
            if (user == null)
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(maSV);
                user = new NguoiDung
                {
                    TenDangNhap = maSV,
                    HoTen = string.IsNullOrWhiteSpace(request.HoTen) ? maSV : request.HoTen!.Trim(),
                    MatKhau = passwordHash,
                    TrangThai = true,
                    MaQuyen = request.MaQuyen,

                    Email = request.Email,
                    SoDienThoai = request.SoDienThoai,
                    NgaySinh = request.NgaySinh,
                    GioiTinh = request.GioiTinh,
                    DiaChi = request.DiaChi,
                    DanToc = request.DanToc,
                    TonGiao = request.TonGiao,
                    AnhDaiDien = avatarUrl
                };
                await _repo.AddUserAsync(user);
                await _repo.SaveChangesAsync(); // lấy MaNguoiDung (IDENTITY)
            }
            else
            {
                user.HoTen ??= request.HoTen ?? maSV;
                user.MaQuyen ??= request.MaQuyen;
                if (!string.IsNullOrWhiteSpace(avatarUrl))
                    user.AnhDaiDien = avatarUrl;

                await _repo.UpdateUserAsync(user);
                await _repo.SaveChangesAsync();
            }

            var sv = new SinhVien
            {
                MaSinhVien = maSV,
                MaNguoiDung = user.MaNguoiDung, // FK int -> NguoiDung
                LopHanhChinh = request.LopHanhChinh,
                NamNhapHoc = request.NamNhapHoc ?? DateTime.Now.Year,
                Khoa = request.Khoa,
                Nganh = request.Nganh
            };

            await _repo.AddStudentAsync(sv);
            await _repo.SaveChangesAsync();

            return new CreateStudentResponse
            {
                MaSinhVien = sv.MaSinhVien,
                MaNguoiDung = user.MaNguoiDung?.ToString(),
                TenDangNhap = user.TenDangNhap,
                HoTen = user.HoTen,
                MaQuyen = user.MaQuyen,
                TrangThaiUser = user.TrangThai,

                LopHanhChinh = sv.LopHanhChinh,
                NamNhapHoc = sv.NamNhapHoc,
                Khoa = sv.Khoa,
                Nganh = sv.Nganh
            };
        }

        public async Task<PagedResult<StudentListItemResponse>> GetListAsync(GetStudentsRequest request)
        {
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 200);

            var sortBy = request.SortBy ?? "HoTen";
            var sortDir = (request.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (items, total) = await _repo.SearchStudentsAsync(
                // keyword: request.Keyword, // removed
                khoa: request.Khoa,
                nganh: request.Nganh,
                namNhapHoc: request.NamNhapHoc,
                trangThaiUser: request.TrangThaiUser,
                maLopHocPhan: request.MaLopHocPhan,
                sortBy: sortBy,
                desc: desc,
                page: page,
                pageSize: pageSize
            );

            var list = items.Select(t => new StudentListItemResponse
            {
                MaSinhVien = t.Sv.MaSinhVien,
                HoTen = t.Nd.HoTen,
                LopHanhChinh = t.Sv.LopHanhChinh,
                NamNhapHoc = t.Sv.NamNhapHoc,
                Khoa = t.Sv.Khoa,
                Nganh = t.Sv.Nganh
            }).ToList();

            return new PagedResult<StudentListItemResponse>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = list
            };
        }

        public async Task<UpdateStudentResponse> UpdateAsync(UpdateStudentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MaSinhVien))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã sinh viên không được để trống.");

            var maSV = HelperFunctions.NormalizeCode(request.MaSinhVien);
            if (string.IsNullOrWhiteSpace(maSV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã sinh viên không hợp lệ.");

            var sv = await _repo.GetStudentByMaSinhVienAsync(maSV);
            if (sv == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy sinh viên.");

            if (!sv.MaNguoiDung.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.InternalError, "Sinh viên thiếu liên kết người dùng.");

            var user = await _repo.GetUserByIdAsync(sv.MaNguoiDung.Value);
            if (user == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy người dùng.");

            // KHÔNG cho đổi TenDangNhap

            // Đổi mã quyền (nếu có) -> phải tồn tại
            if (request.MaQuyen.HasValue)
            {
                if (request.MaQuyen.Value <= 0)
                    ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã quyền không hợp lệ.");
                var role = await _repo.GetRoleAsync(request.MaQuyen.Value);
                if (role == null)
                    ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");
                user!.MaQuyen = request.MaQuyen.Value;
            }

            if (request.TrangThai.HasValue) user!.TrangThai = request.TrangThai.Value;
            if (!string.IsNullOrWhiteSpace(request.TenSinhVien)) user!.HoTen = request.TenSinhVien.Trim();

            if (request.GioiTinh.HasValue) user!.GioiTinh = request.GioiTinh;
            if (!string.IsNullOrWhiteSpace(request.Email)) user!.Email = request.Email.Trim();
            if (!string.IsNullOrWhiteSpace(request.SoDienThoai)) user!.SoDienThoai = request.SoDienThoai.Trim();
            if (request.NgaySinh.HasValue) user!.NgaySinh = request.NgaySinh.Value;
            if (!string.IsNullOrWhiteSpace(request.DanToc)) user!.DanToc = request.DanToc.Trim();
            if (!string.IsNullOrWhiteSpace(request.TonGiao)) user!.TonGiao = request.TonGiao.Trim();
            if (!string.IsNullOrWhiteSpace(request.DiaChi)) user!.DiaChi = request.DiaChi.Trim();

            if (request.AnhDaiDien != null)
            {
                var avatarUrl = await AvatarHelper.SaveAvatarAsync(request.AnhDaiDien, _env.WebRootPath, maSV);
                if (!string.IsNullOrWhiteSpace(avatarUrl))
                    user.AnhDaiDien = avatarUrl;
            }

            await _repo.UpdateUserAsync(user!);

            // Update SinhVien
            if (!string.IsNullOrWhiteSpace(request.LopHanhChinh)) sv!.LopHanhChinh = request.LopHanhChinh.Trim();
            if (request.NamNhapHoc.HasValue) sv!.NamNhapHoc = request.NamNhapHoc.Value;
            if (!string.IsNullOrWhiteSpace(request.Khoa)) sv!.Khoa = request.Khoa.Trim();
            if (!string.IsNullOrWhiteSpace(request.Nganh)) sv!.Nganh = request.Nganh.Trim();
            await _repo.UpdateStudentAsync(sv!);

            // Log
            await _repo.AddActivityAsync(new LichSuHoatDong
            {
                MaNguoiDung = user!.MaNguoiDung,
                HanhDong = $"Cập nhật thông tin sinh viên [{sv!.MaSinhVien}]",
                ThoiGian = HelperFunctions.UtcToVietnam(DateTime.UtcNow)
            });

            await _repo.SaveChangesAsync();

            return new UpdateStudentResponse
            {
                MaNguoiDung = user.MaNguoiDung?.ToString(),
                MaSinhVien = sv.MaSinhVien,
                TenDangNhap = user.TenDangNhap, // chỉ trả ra, không cho sửa
                HoTen = user.HoTen,
                TrangThai = user.TrangThai ?? true,
                MaQuyen = user.MaQuyen,

                LopHanhChinh = sv.LopHanhChinh,
                NamNhapHoc = sv.NamNhapHoc,
                Khoa = sv.Khoa,
                Nganh = sv.Nganh,

                GioiTinh = user.GioiTinh,
                AnhDaiDien = user.AnhDaiDien,
                Email = user.Email,
                SoDienThoai = user.SoDienThoai,
                NgaySinh = user.NgaySinh?.ToString("dd-MM-yyyy"),
                DanToc = user.DanToc,
                TonGiao = user.TonGiao,
                DiaChi = user.DiaChi
            };
        }

        public async Task<AddStudentToCourseResponse> AddStudentToCourseAsync(AddStudentToCourseRequest req, string? currentUserTenDangNhap)
        {
            var maLhp = HelperFunctions.NormalizeCode(req.MaLopHocPhan);
            var maSv = HelperFunctions.NormalizeCode(req.MaSinhVien);

            if (string.IsNullOrWhiteSpace(maLhp) || string.IsNullOrWhiteSpace(maSv))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu mã lớp học phần hoặc mã sinh viên.");

            // Tồn tại lớp học phần?
            if (!await _repo.CourseExistsAsync(maLhp))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Lớp học phần không tồn tại.");

            // Tồn tại sinh viên?
            if (!await _repo.ExistsStudentAsync(maSv))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Sinh viên không tồn tại.");

            // Đã tham gia chưa?
            if (await _repo.ParticipationExistsAsync(maLhp, maSv))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Sinh viên đã tham gia lớp học phần này.");

            var entity = new ThamGiaLop
            {
                MaLopHocPhan = maLhp,
                MaSinhVien = maSv,
                NgayThamGia = req.NgayThamGia ?? DateTime.Now,
                TrangThai = req.TrangThai ?? true
            };

            await _repo.AddParticipationAsync(entity);

            // Log hoạt động theo username trong token
            if (!string.IsNullOrWhiteSpace(currentUserTenDangNhap))
            {
                var user = await _repo.GetUserByUsernameAsync(currentUserTenDangNhap);
                if (user != null)
                {
                    await _repo.AddActivityAsync(new LichSuHoatDong
                    {
                        MaNguoiDung = user.MaNguoiDung,
                        HanhDong = $"Thêm sinh viên {entity.MaSinhVien} vào lớp {entity.MaLopHocPhan}",
                        ThoiGian = HelperFunctions.UtcToVietnam(DateTime.UtcNow)
                    });
                    await _repo.SaveChangesAsync();
                }
            }

            return new AddStudentToCourseResponse
            {
                MaLopHocPhan = entity.MaLopHocPhan,
                MaSinhVien = entity.MaSinhVien,
                NgayThamGia = entity.NgayThamGia?.ToString("dd-MM-yyyy"),
                TrangThai = entity.TrangThai
            };
        }

        public async Task<RemoveStudentFromCourseResponse> RemoveStudentFromCourseAsync(RemoveStudentFromCourseRequest req, string? currentUserTenDangNhap)
        {
            var maLhp = HelperFunctions.NormalizeCode(req.MaLopHocPhan);
            var maSv = HelperFunctions.NormalizeCode(req.MaSinhVien);

            if (string.IsNullOrWhiteSpace(maLhp) || string.IsNullOrWhiteSpace(maSv))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu mã lớp học phần hoặc mã sinh viên.");

            // Tìm bản ghi tham gia
            var thamGia = await _repo.GetParticipationAsync(maLhp, maSv);
            if (thamGia == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy bản ghi tham gia lớp học phần.");

            // Soft delete: đặt trạng thái tham gia = false
            thamGia.TrangThai = false;
            await _repo.UpdateParticipationAsync(thamGia);

            // Log hoạt động theo username trong token
            if (!string.IsNullOrWhiteSpace(currentUserTenDangNhap))
            {
                var user = await _repo.GetUserByUsernameAsync(currentUserTenDangNhap);
                if (user != null)
                {
                    await _repo.AddActivityAsync(new LichSuHoatDong
                    {
                        MaNguoiDung = user.MaNguoiDung,
                        HanhDong = $"Gỡ sinh viên {thamGia.MaSinhVien} khỏi lớp {thamGia.MaLopHocPhan}",
                        ThoiGian = HelperFunctions.UtcToVietnam(DateTime.UtcNow)
                    });
                    await _repo.SaveChangesAsync();
                }
            }

            return new RemoveStudentFromCourseResponse
            {
                MaLopHocPhan = thamGia.MaLopHocPhan,
                MaSinhVien = thamGia.MaSinhVien,
                TrangThai = thamGia.TrangThai
            };
        }
    }
}
