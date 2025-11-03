// File: Services/Implementations/LecturerService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Models;
using DiemDanhQR_API.Repositories.Interfaces;
using DiemDanhQR_API.Helpers;
using DiemDanhQR_API.Services.Interfaces;

namespace DiemDanhQR_API.Services.Implementations
{
    public class LecturerService : ILecturerService
    {
        private readonly ILecturerRepository _repo;
        private readonly IWebHostEnvironment _env;

        public LecturerService(ILecturerRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        public async Task<CreateLecturerResponse> CreateAsync(CreateLecturerRequest request)
        {
            var maGV = HelperFunctions.NormalizeCode(request.MaGiangVien);
            if (string.IsNullOrWhiteSpace(maGV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã giảng viên không hợp lệ.");

            if (!request.MaQuyen.HasValue || request.MaQuyen.Value <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã quyền không được để trống.");

            var role = await _repo.GetRoleAsync(request.MaQuyen.Value);
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");

            // Không cho trùng mã giảng viên
            if (await _repo.ExistsLecturerAsync(maGV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã giảng viên đã tồn tại.");

            // Avatar: dùng maGV làm key lưu file
            string? avatarUrl = null;
            if (request.AnhDaiDien != null)
            {
                avatarUrl = await AvatarHelper.SaveAvatarAsync(request.AnhDaiDien, _env.WebRootPath, maGV);
            }

            // Tài khoản: TenDangNhap = MaGiangVien
            var user = await _repo.GetUserByUsernameAsync(maGV);
            if (user == null)
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(maGV);
                user = new NguoiDung
                {
                    TenDangNhap = maGV,
                    HoTen = string.IsNullOrWhiteSpace(request.HoTen) ? maGV : request.HoTen!.Trim(),
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
                // Cần SaveChanges để lấy MaNguoiDung (IDENTITY) cho FK GiangVien
                await _repo.SaveChangesAsync();
            }
            else
            {
                user.HoTen ??= request.HoTen ?? maGV;
                user.MaQuyen ??= request.MaQuyen;
                if (!string.IsNullOrWhiteSpace(avatarUrl))
                    user.AnhDaiDien = avatarUrl;

                await _repo.UpdateUserAsync(user);
                await _repo.SaveChangesAsync();
            }

            var gv = new GiangVien
            {
                MaGiangVien = maGV,
                MaNguoiDung = user.MaNguoiDung, // int? FK -> NguoiDung
                Khoa = request.Khoa,
                HocHam = request.HocHam,
                HocVi = request.HocVi,
                NgayTuyenDung = request.NgayTuyenDung
            };

            await _repo.AddLecturerAsync(gv);
            await _repo.SaveChangesAsync();

            return new CreateLecturerResponse
            {
                MaGiangVien = gv.MaGiangVien,
                MaNguoiDung = user.MaNguoiDung?.ToString(),
                TenDangNhap = user.TenDangNhap,
                HoTen = user.HoTen,
                MaQuyen = user.MaQuyen,
                Khoa = gv.Khoa,
                HocHam = gv.HocHam,
                HocVi = gv.HocVi,
                NgayTuyenDung = gv.NgayTuyenDung,
                TrangThaiUser = user.TrangThai ?? true
            };
        }

        public async Task<PagedResult<LecturerListItemResponse>> GetListAsync(GetLecturersRequest request)
        {
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min((int)request.PageSize!, 200);

            var sortBy = request.SortBy ?? "HoTen";
            var sortDir = (request.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (items, total) = await _repo.SearchLecturersAsync(
                // keyword: request.Keyword, // removed
                khoa: request.Khoa,
                hocHam: request.HocHam,
                hocVi: request.HocVi,
                ngayTuyenDungFrom: request.NgayTuyenDungFrom,
                ngayTuyenDungTo: request.NgayTuyenDungTo,
                trangThaiUser: request.TrangThaiUser,
                sortBy: sortBy,
                desc: desc,
                page: (int)page!,
                pageSize: pageSize
            );

            var list = items.Select(t => new LecturerListItemResponse
            {
                MaGiangVien = t.Gv.MaGiangVien!,
                HoTen = t.Nd.HoTen,
                Khoa = t.Gv.Khoa,
                HocHam = t.Gv.HocHam,
                HocVi = t.Gv.HocVi,
                NgayTuyenDung = t.Gv.NgayTuyenDung
            }).ToList();

            return new PagedResult<LecturerListItemResponse>
            {
                Page = (int)page!,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = list
            };
        }

        public async Task<UpdateLecturerResponse> UpdateAsync(UpdateLecturerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MaGiangVien))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã giảng viên không được để trống.");

            var maGV = HelperFunctions.NormalizeCode(request.MaGiangVien);
            if (string.IsNullOrWhiteSpace(maGV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã giảng viên không hợp lệ.");

            // Lấy lecturer theo MaGiangVien
            var gv = await _repo.GetLecturerByMaGiangVienAsync(maGV);
            if (gv == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy hồ sơ giảng viên.");

            // Lấy user theo MaNguoiDung (int) từ giảng viên
            if (!gv.MaNguoiDung.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.InternalError, "Hồ sơ giảng viên không hợp lệ (thiếu liên kết người dùng).");

            var user = await _repo.GetUserByIdAsync(gv.MaNguoiDung!.Value);
            if (user == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy người dùng.");

            // KHÔNG cho đổi tên đăng nhập nữa (đã bỏ TenDangNhap khỏi request)

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
            if (!string.IsNullOrWhiteSpace(request.TenGiangVien)) user!.HoTen = request.TenGiangVien.Trim();

            if (request.GioiTinh.HasValue) user!.GioiTinh = request.GioiTinh;
            if (!string.IsNullOrWhiteSpace(request.Email)) user!.Email = request.Email.Trim();
            if (!string.IsNullOrWhiteSpace(request.SoDienThoai)) user!.SoDienThoai = request.SoDienThoai.Trim();
            if (request.NgaySinh.HasValue) user!.NgaySinh = request.NgaySinh.Value;
            if (!string.IsNullOrWhiteSpace(request.DanToc)) user!.DanToc = request.DanToc.Trim();
            if (!string.IsNullOrWhiteSpace(request.TonGiao)) user!.TonGiao = request.TonGiao.Trim();
            if (!string.IsNullOrWhiteSpace(request.DiaChi)) user!.DiaChi = request.DiaChi.Trim();

            // Ảnh đại diện mới (nếu có file) - dùng MaGiangVien để đặt tên file
            if (request.AnhDaiDien != null)
            {
                var avatarUrl = await AvatarHelper.SaveAvatarAsync(request.AnhDaiDien, _env.WebRootPath, maGV);
                if (!string.IsNullOrWhiteSpace(avatarUrl))
                    user.AnhDaiDien = avatarUrl;
            }

            await _repo.UpdateUserAsync(user!);

            // Thông tin giảng viên
            if (!string.IsNullOrWhiteSpace(request.Khoa)) gv!.Khoa = request.Khoa.Trim();
            if (!string.IsNullOrWhiteSpace(request.HocHam)) gv!.HocHam = request.HocHam.Trim();
            if (!string.IsNullOrWhiteSpace(request.HocVi)) gv!.HocVi = request.HocVi.Trim();
            if (request.NgayTuyenDung.HasValue) gv!.NgayTuyenDung = request.NgayTuyenDung.Value;

            await _repo.UpdateLecturerAsync(gv!);

            await _repo.AddActivityAsync(new LichSuHoatDong
            {
                MaNguoiDung = user!.MaNguoiDung,
                HanhDong = $"Cập nhật thông tin giảng viên [{gv!.MaGiangVien}]",
                ThoiGian = HelperFunctions.UtcToVietnam(DateTime.UtcNow)
            });
            await _repo.SaveChangesAsync();

            return new UpdateLecturerResponse
            {
                MaNguoiDung = user.MaNguoiDung?.ToString(),
                MaGiangVien = gv.MaGiangVien,
                TenDangNhap = user.TenDangNhap, // chỉ trả ra, không cho sửa
                HoTen = user.HoTen,
                TrangThai = user.TrangThai ?? true,
                MaQuyen = user.MaQuyen,

                Khoa = gv.Khoa,
                HocHam = gv.HocHam,
                HocVi = gv.HocVi,
                NgayTuyenDung = gv.NgayTuyenDung?.ToString("yyyy-MM-dd"),

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

    }
}
