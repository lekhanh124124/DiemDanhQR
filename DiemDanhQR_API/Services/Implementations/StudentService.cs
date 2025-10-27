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

            var maND = HelperFunctions.NormalizeCode(
                string.IsNullOrWhiteSpace(request.MaNguoiDung) ? request.MaSinhVien : request.MaNguoiDung
            );
            if (string.IsNullOrWhiteSpace(maND))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã người dùng không hợp lệ.");

            if (request.MaQuyen <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã quyền không hợp lệ.");

            var role = await _repo.GetRoleAsync(request.MaQuyen);
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");

            if (await _repo.ExistsStudentAsync(maSV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã sinh viên đã tồn tại.");

            var user = await _repo.GetUserByMaAsync(maND)
                       ?? await _repo.GetUserByUsernameAsync(maND);

            string? avatarUrl = null;
            if (request.AnhDaiDien != null)
            {
                avatarUrl = await AvatarHelper.SaveAvatarAsync(request.AnhDaiDien, _env.WebRootPath, maND);
            }

            if (user == null)
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(maND);
                user = new NguoiDung
                {
                    MaNguoiDung = maND,
                    TenDangNhap = maND,
                    HoTen = string.IsNullOrWhiteSpace(request.HoTen) ? maND : request.HoTen!.Trim(),
                    MatKhau = passwordHash,
                    TrangThai = true,
                    MaQuyen = request.MaQuyen,

                    Email = request.Email,
                    SoDienThoai = request.SoDienThoai,
                    NgaySinh = request.NgaySinh,
                    GioiTinh = request.GioiTinh,
                    DiaChi = request.DiaChi,

                    AnhDaiDien = avatarUrl // có file thì set, không thì để null
                };
                await _repo.AddUserAsync(user);
            }
            else
            {
                user.HoTen ??= request.HoTen ?? maND;
                user.MaQuyen ??= request.MaQuyen;

                if (!string.IsNullOrWhiteSpace(avatarUrl))
                    user.AnhDaiDien = avatarUrl;
            }



            var sv = new SinhVien
            {
                MaSinhVien = maSV,
                MaNguoiDung = maND,
                LopHanhChinh = request.LopHanhChinh,
                NamNhapHoc = request.NamNhapHoc ?? DateTime.Now.Year,
                Khoa = request.Khoa,
                Nganh = request.Nganh,
            };

            await _repo.AddStudentAsync(sv);
            await _repo.SaveChangesAsync();

            return new CreateStudentResponse
            {
                MaSinhVien = sv.MaSinhVien,
                MaNguoiDung = user.MaNguoiDung,
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
                keyword: request.Keyword,
                khoa: request.Khoa,
                nganh: request.Nganh,
                namNhapHoc: request.NamNhapHoc,
                trangThaiUser: request.TrangThaiUser,
                maLopHocPhan: request.MaLopHocPhan,   // NEW
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
            if (string.IsNullOrWhiteSpace(request.MaNguoiDung))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã người dùng không được để trống.");

            var maND = HelperFunctions.NormalizeCode(request.MaNguoiDung);
            if (string.IsNullOrWhiteSpace(maND))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã người dùng không hợp lệ.");

            var user = await _repo.GetUserByMaAsync(maND);
            if (user == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy người dùng.");

            var sv = await _repo.GetStudentByMaNguoiDungAsync(maND);
            if (sv == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy sinh viên.");

            // --- Update User (như hiện tại) ---
            if (!string.IsNullOrWhiteSpace(request.TenDangNhap))
            {
                var newUsername = HelperFunctions.NormalizeCode(request.TenDangNhap);
                if (!string.Equals(newUsername, user!.TenDangNhap, StringComparison.OrdinalIgnoreCase))
                {
                    if (await _repo.ExistsUsernameForAnotherAsync(newUsername, maND))
                        ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập đã tồn tại.");
                    user.TenDangNhap = newUsername;
                }
            }

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
                var avatarUrl = await AvatarHelper.SaveAvatarAsync(request.AnhDaiDien, _env.WebRootPath, user!.MaNguoiDung!);
                if (!string.IsNullOrWhiteSpace(avatarUrl))
                    user.AnhDaiDien = avatarUrl;
            }

            await _repo.UpdateUserAsync(user!);

            // --- Update SinhVien ---
            if (!string.IsNullOrWhiteSpace(request.LopHanhChinh)) sv!.LopHanhChinh = request.LopHanhChinh.Trim();
            if (request.NamNhapHoc.HasValue) sv!.NamNhapHoc = request.NamNhapHoc.Value;
            if (!string.IsNullOrWhiteSpace(request.Khoa)) sv!.Khoa = request.Khoa.Trim();
            if (!string.IsNullOrWhiteSpace(request.Nganh)) sv!.Nganh = request.Nganh.Trim();
            await _repo.UpdateStudentAsync(sv!);

            // --- Log ---
            await _repo.AddActivityAsync(new LichSuHoatDong
            {
                MaNguoiDung = user!.MaNguoiDung,
                HanhDong = $"Cập nhật thông tin sinh viên [{sv!.MaSinhVien}] (ND: {user.MaNguoiDung})",
                ThoiGian = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();

            return new UpdateStudentResponse
            {
                MaNguoiDung = user.MaNguoiDung,
                MaSinhVien = sv.MaSinhVien,
                TenDangNhap = user.TenDangNhap,
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
                NgaySinh = user.NgaySinh.HasValue ? user.NgaySinh.Value.ToString("dd-MM-yyyy") : null,
                DanToc = user.DanToc,
                TonGiao = user.TonGiao,
                DiaChi = user.DiaChi
            };
        }
    }
}
