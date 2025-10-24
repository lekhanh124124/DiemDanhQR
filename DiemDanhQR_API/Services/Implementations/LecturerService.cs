// File: Services/Implementations/LecturerService.cs
using BCrypt.Net;
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

        public LecturerService(ILecturerRepository repo)
        {
            _repo = repo;
        }

        public async Task<CreateLecturerResponse> CreateAsync(CreateLecturerRequest request)
        {
            var maGV = HelperFunctions.NormalizeCode(request.MaGiangVien);
            if (string.IsNullOrWhiteSpace(maGV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã giảng viên không hợp lệ.");

            var maND = HelperFunctions.NormalizeCode(
                string.IsNullOrWhiteSpace(request.MaNguoiDung) ? request.MaGiangVien : request.MaNguoiDung
            );
            if (string.IsNullOrWhiteSpace(maND))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã người dùng không hợp lệ.");

            if (request.MaQuyen <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã quyền không hợp lệ.");

            // Quyền phải tồn tại
            var role = await _repo.GetRoleAsync(request.MaQuyen);
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");

            // Giảng viên không được trùng
            if (await _repo.ExistsLecturerAsync(maGV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã giảng viên đã tồn tại.");

            // Kiểm tra/tạo user
            var user = await _repo.GetUserByMaAsync(maND)
                       ?? await _repo.GetUserByUsernameAsync(maND); // TenDangNhap = MaND theo quy ước

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
                    DiaChi = request.DiaChi
                };

                await _repo.AddUserAsync(user);
            }
            else
            {
                // Nếu user đã tồn tại, có thể cập nhật nhẹ hồ sơ/nhóm quyền nếu cần
                user.HoTen ??= request.HoTen ?? maND;
                user.MaQuyen ??= request.MaQuyen;
            }

            // Tạo giảng viên
            var gv = new GiangVien
            {
                MaGiangVien = maGV,
                MaNguoiDung = maND,
                Khoa = request.Khoa,
                HocHam = request.HocHam,
                HocVi = request.HocVi,
                NgayTuyenDung = request.NgayTuyenDung
            };

            await _repo.AddLecturerAsync(gv);
            await _repo.SaveChangesAsync();

            var resp = new CreateLecturerResponse(
                maGV,
                maND,
                user.TenDangNhap ?? maND,
                user.HoTen ?? maND,
                user.MaQuyen ?? request.MaQuyen,
                gv.Khoa,
                gv.HocHam,
                gv.HocVi,
                gv.NgayTuyenDung,
                user.TrangThai ?? true
            );

            return resp;
        }
        public async Task<PagedResult<LecturerListItemResponse>> GetListAsync(GetLecturersRequest request)
        {
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 200);

            var (items, total) = await _repo.SearchLecturersAsync(
                keyword: request.Keyword,
                khoa: request.Khoa,
                hocHam: request.HocHam,
                hocVi: request.HocVi,
                ngayTuyenDungFrom: request.NgayTuyenDungFrom,
                ngayTuyenDungTo: request.NgayTuyenDungTo,
                trangThaiUser: request.TrangThaiUser,
                sortBy: request.SortBy,
                desc: request.Desc,
                page: page,
                pageSize: pageSize
            );

            var list = items.Select(t => new LecturerListItemResponse(
                maGiangVien: t.Gv.MaGiangVien!,
                hoTen: t.Nd.HoTen,
                khoa: t.Gv.Khoa,
                hocHam: t.Gv.HocHam,
                hocVi: t.Gv.HocVi,
                ngayTuyenDung: t.Gv.NgayTuyenDung
            )).ToList();

            return new PagedResult<LecturerListItemResponse>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                Items = list
            };
        }
    }
}
