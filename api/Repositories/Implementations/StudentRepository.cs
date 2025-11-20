// File: Repositories/Implementations/StudentRepository.cs
using System.Text.RegularExpressions;
using api.Data;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.Implementations
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _db;
        public StudentRepository(AppDbContext db) => _db = db;

        public Task<bool> ExistsStudentAsync(string maSinhVien)
            => _db.SinhVien.AnyAsync(s => s.MaSinhVien == maSinhVien);

        public async Task AddStudentAsync(SinhVien entity)
            => await _db.SinhVien.AddAsync(entity);

        public Task<NguoiDung?> GetUserByIdAsync(int maNguoiDung)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == maNguoiDung);

        public Task<NguoiDung?> GetUserByUsernameAsync(string tenDangNhap)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap);

        public async Task AddUserAsync(NguoiDung user)
            => await _db.NguoiDung.AddAsync(user);

        public Task UpdateUserAsync(NguoiDung user)
        {
            _db.NguoiDung.Update(user);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        public async Task<(List<(SinhVien Sv, NguoiDung Nd, Nganh? Ng, Khoa? Kh, DateOnly? NgayTG, bool? TrangThaiTG)> Items, int Total)>
    SearchStudentsAsync(int? maKhoa, int? maNganh, int? namNhapHoc, bool? trangThaiUser,
                        string? maLopHocPhan, string? sortBy, bool desc, int page, int pageSize,
                        string? maSinhVien, string? hoTen) // NEW
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q =
                from sv in _db.SinhVien.AsNoTracking()
                join nd in _db.NguoiDung.AsNoTracking() on sv.MaNguoiDung equals nd.MaNguoiDung
                join ng0 in _db.Nganh.AsNoTracking() on sv.MaNganh equals ng0.MaNganh into ngLeft
                from ng in ngLeft.DefaultIfEmpty()
                join kh0 in _db.Khoa.AsNoTracking() on ng!.MaKhoa equals kh0.MaKhoa into khLeft
                from kh in khLeft.DefaultIfEmpty()
                select new { Sv = sv, Nd = nd, Ng = ng, Kh = kh };

            // Filters
            if (maKhoa.HasValue) q = q.Where(x => x.Kh != null && x.Kh.MaKhoa == maKhoa.Value);
            if (maNganh.HasValue) q = q.Where(x => x.Ng != null && x.Ng.MaNganh == maNganh.Value);
            if (namNhapHoc.HasValue) q = q.Where(x => x.Sv.NamNhapHoc == namNhapHoc.Value);
            if (trangThaiUser.HasValue) q = q.Where(x => x.Nd.TrangThai == trangThaiUser.Value);

            if (!string.IsNullOrWhiteSpace(maSinhVien))
            {
                var id = maSinhVien.Trim();
                q = q.Where(x => x.Sv.MaSinhVien == id);
            }

            // NEW: filter HoTen (case-insensitive)
            if (!string.IsNullOrWhiteSpace(hoTen))
            {
                var kw = hoTen.Trim().ToLower();
                q = q.Where(x => (x.Nd.HoTen ?? "").ToLower().Contains(kw));
            }

            // Sort giữ nguyên
            var key = (sortBy ?? "HoTen").Trim().ToLowerInvariant();
            q = key switch
            {
                "masinhvien" => (desc ? q.OrderByDescending(x => x.Sv.MaSinhVien) : q.OrderBy(x => x.Sv.MaSinhVien)),
                "namnhaphoc" => (desc ? q.OrderByDescending(x => x.Sv.NamNhapHoc) : q.OrderBy(x => x.Sv.NamNhapHoc)),
                "makhoa" => (desc ? q.OrderByDescending(x => x.Kh!.MaKhoa) : q.OrderBy(x => x.Kh!.MaKhoa)),
                "manganh" => (desc ? q.OrderByDescending(x => x.Ng!.MaNganh) : q.OrderBy(x => x.Ng!.MaNganh)),
                _ => (desc ? q.OrderByDescending(x => x.Nd.HoTen) : q.OrderBy(x => x.Nd.HoTen)),
            };

            if (string.IsNullOrWhiteSpace(maLopHocPhan))
            {
                // 4A) KHÔNG lọc theo lớp → không join ThamGiaLop → không phát sinh duplicate
                var total = await q.CountAsync();

                var pageRows = await q
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new { x.Sv, x.Nd, x.Ng, x.Kh })
                    .ToListAsync();

                var items = pageRows
                    .Select(x => (x.Sv, x.Nd, x.Ng, x.Kh, (DateOnly?)null, (bool?)null))
                    .ToList();

                return (items, total);
            }
            else
            {
                // 4B) CÓ MaLopHocPhan → INNER JOIN để chỉ lấy SV thuộc lớp đó
                var q2 =
                    from x in q
                    join tg in _db.ThamGiaLop.AsNoTracking()
                         on new { x.Sv.MaSinhVien, MaLopHocPhan = maLopHocPhan! }
                         equals new { tg.MaSinhVien, tg.MaLopHocPhan }
                    select new { x.Sv, x.Nd, x.Ng, x.Kh, Tg = tg };

                var total = await q2.CountAsync();

                var pageRows = await q2
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var items = pageRows
                    .Select(z => (z.Sv, z.Nd, z.Ng, z.Kh,
                                  (DateOnly?)z.Tg.NgayThamGia,
                                  (bool?)z.Tg.TrangThai))
                    .ToList();

                return (items, total);
            }
        }

        public Task<SinhVien?> GetStudentByMaNguoiDungAsync(int maNguoiDung)
            => _db.SinhVien.FirstOrDefaultAsync(s => s.MaNguoiDung == maNguoiDung);

        public Task<SinhVien?> GetStudentByMaSinhVienAsync(string maSinhVien)
            => _db.SinhVien.FirstOrDefaultAsync(s => s.MaSinhVien == maSinhVien);

        public Task UpdateStudentAsync(SinhVien entity)
        {
            _db.SinhVien.Update(entity);
            return Task.CompletedTask;
        }

        public async Task AddActivityAsync(LichSuHoatDong log)
            => await _db.LichSuHoatDong.AddAsync(log);

        public async Task<bool> CourseExistsAsync(string maLopHocPhan)
        {
            var code = (maLopHocPhan ?? "").Trim();
            if (string.IsNullOrEmpty(code)) return false;
            return await _db.LopHocPhan.AsNoTracking().AnyAsync(l => l.MaLopHocPhan == code);
        }

        public async Task<bool> ParticipationExistsAsync(string maLopHocPhan, string maSinhVien)
        {
            var lhp = (maLopHocPhan ?? "").Trim();
            var sv = (maSinhVien ?? "").Trim();
            return await _db.ThamGiaLop.AsNoTracking()
                .AnyAsync(t => t.MaLopHocPhan == lhp && t.MaSinhVien == sv);
        }

        public async Task AddParticipationAsync(ThamGiaLop thamGia)
        {
            _db.ThamGiaLop.Add(thamGia);
            await _db.SaveChangesAsync();
        }

        public Task<ThamGiaLop?> GetParticipationAsync(string maLopHocPhan, string maSinhVien)
        {
            var lhp = (maLopHocPhan ?? "").Trim();
            var sv = (maSinhVien ?? "").Trim();
            return _db.ThamGiaLop.FirstOrDefaultAsync(t => t.MaLopHocPhan == lhp && t.MaSinhVien == sv);
        }

        public async Task UpdateParticipationAsync(ThamGiaLop thamGia)
        {
            _db.ThamGiaLop.Update(thamGia);
            await _db.SaveChangesAsync();
        }
        public async Task<string> GenerateNextMaSinhVienAsync(string codeNganh, int namNhapHoc)
        {
            var yy = (namNhapHoc % 100).ToString("D2");
            const int sttWidth = 4;

            // Độ dài tối đa của MaSinhVien là 20 (mapping EF) :contentReference[oaicite:2]{index=2}
            // -> Tiền tố (prefix) = CodeNganh + YY, hậu tố (suffix) = STT(3)
            // => codeNganh tối đa 20 - 2 - 3 = 15 ký tự.
            var maxCodeLen = 20 - 2 - sttWidth; // 15
            if (codeNganh.Length > maxCodeLen)
                codeNganh = codeNganh.Substring(0, maxCodeLen);

            var prefix = $"{yy}{codeNganh}";

            // Lấy tất cả SV có cùng prefix, parse phần STT số ở cuối rồi lấy max
            // (SQL: WHERE MaSinhVien LIKE prefix + '%')
            var candidates = await _db.SinhVien
                .AsNoTracking()
                .Where(s => EF.Functions.Like(s.MaSinhVien, prefix + "%"))
                .Select(s => s.MaSinhVien)
                .ToListAsync();

            int maxStt = 0;
            var rx = new Regex($"^{Regex.Escape(prefix)}(\\d+)$");
            foreach (var id in candidates)
            {
                var m = rx.Match(id);
                if (m.Success && int.TryParse(m.Groups[1].Value, out var n))
                    if (n > maxStt) maxStt = n;
            }

            // Tạo mã mới
            string next;
            int attempt = 0;
            do
            {
                var stt = (maxStt + 1 + attempt).ToString($"D{sttWidth}");
                next = prefix + stt;
                attempt++;
            }
            while (await _db.SinhVien.AsNoTracking().AnyAsync(s => s.MaSinhVien == next));

            return next;
        }

        // Lấy ngành theo CodeNganh
        public Task<Nganh?> GetNganhByCodeAsync(string code)
        {
            var c = (code ?? "").Trim();
            return _db.Nganh.FirstOrDefaultAsync(n => n.CodeNganh == c);
        }

        // Kiểm tra username tồn tại
        public Task<NguoiDung?> GetUserByLoginAsync(string tenDangNhap)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap);

        // Tìm theo email/sđt (tránh trùng option)
        public Task<bool> ExistsUserByEmailAsync(string? email)
            => string.IsNullOrWhiteSpace(email) ? Task.FromResult(false)
               : _db.NguoiDung.AsNoTracking().AnyAsync(u => u.Email == email!.Trim());

        public Task<bool> ExistsUserByPhoneAsync(string? phone)
            => string.IsNullOrWhiteSpace(phone) ? Task.FromResult(false)
               : _db.NguoiDung.AsNoTracking().AnyAsync(u => u.SoDienThoai == phone!.Trim());
    }
}
