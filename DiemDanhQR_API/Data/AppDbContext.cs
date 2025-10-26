// File: Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using DiemDanhQR_API.Models;

namespace DiemDanhQR_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<PhanQuyen> PhanQuyen { get; set; }
        public DbSet<NguoiDung> NguoiDung { get; set; }
        public DbSet<SinhVien> SinhVien { get; set; }
        public DbSet<GiangVien> GiangVien { get; set; }
        public DbSet<ChucNang> ChucNang { get; set; }
        public DbSet<NhomChucNang> NhomChucNang { get; set; }
        public DbSet<LichSuHoatDong> LichSuHoatDong { get; set; }
        public DbSet<TrangThaiDiemDanh> TrangThaiDiemDanh { get; set; }
        public DbSet<ThamGiaLop> ThamGiaLop { get; set; }
        public DbSet<LopHocPhan> LopHocPhan { get; set; }
        public DbSet<MonHoc> MonHoc { get; set; }
        public DbSet<PhongHoc> PhongHoc { get; set; }
        public DbSet<BuoiHoc> BuoiHoc { get; set; }
        public DbSet<DiemDanh> DiemDanh { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // =========================
            // PHANQUYEN
            // =========================
            modelBuilder.Entity<PhanQuyen>(e =>
            {
                e.ToTable("PhanQuyen");
                e.HasKey(x => x.MaQuyen);
                e.Property(x => x.MaQuyen).ValueGeneratedOnAdd(); // IDENTITY
                e.Property(x => x.CodeQuyen).HasMaxLength(50).IsRequired();
                e.Property(x => x.TenQuyen).HasMaxLength(50).IsRequired();
                e.Property(x => x.MoTa).HasMaxLength(200);
            });

            // =========================
            // CHUCNANG
            // =========================
            modelBuilder.Entity<ChucNang>(e =>
            {
                e.ToTable("ChucNang");
                e.HasKey(x => x.MaChucNang);
                e.Property(x => x.MaChucNang).ValueGeneratedOnAdd(); // IDENTITY
                e.Property(x => x.CodeChucNang).HasMaxLength(50).IsRequired();
                e.Property(x => x.TenChucNang).HasMaxLength(100).IsRequired();
                e.Property(x => x.MoTa).HasMaxLength(200);
                e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();
            });

            // =========================
            // NHOMCHUCNANG (PK kép)
            // =========================
            modelBuilder.Entity<NhomChucNang>(e =>
            {
                e.ToTable("NhomChucNang");
                e.HasKey(x => new { x.MaQuyen, x.MaChucNang });

                e.Property(x => x.MaQuyen).IsRequired();
                e.Property(x => x.MaChucNang).IsRequired();

                e.HasOne<PhanQuyen>()
                 .WithMany()
                 .HasForeignKey(x => x.MaQuyen)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<ChucNang>()
                 .WithMany()
                 .HasForeignKey(x => x.MaChucNang)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // NGUOIDUNG
            // =========================
            modelBuilder.Entity<NguoiDung>(e =>
            {
                e.ToTable("NguoiDung");
                e.HasKey(x => x.MaNguoiDung);
                // PK là NVARCHAR(50), KHÔNG identity:
                e.Property(x => x.MaNguoiDung).HasMaxLength(50);

                // Tài khoản
                e.Property(x => x.TenDangNhap).HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.TenDangNhap).IsUnique();
                e.Property(x => x.MatKhau).HasMaxLength(200).IsRequired();
                e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();

                e.Property(x => x.RefreshTokenHash).HasMaxLength(200);
                e.Property(x => x.RefreshTokenIssuedAt).HasColumnType("datetime");
                e.Property(x => x.RefreshTokenExpiresAt).HasColumnType("datetime");
                e.Property(x => x.RefreshTokenId).HasColumnType("uniqueidentifier");
                e.Property(x => x.RefreshTokenRevokedAt).HasColumnType("datetime");

                // Hồ sơ
                e.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
                e.Property(x => x.GioiTinh).HasColumnType("tinyint");
                e.Property(x => x.AnhDaiDien).HasMaxLength(255);
                e.Property(x => x.Email).HasMaxLength(100);
                e.Property(x => x.SoDienThoai).HasMaxLength(15);
                e.Property(x => x.NgaySinh).HasColumnType("date");
                e.Property(x => x.DanToc).HasMaxLength(20);
                e.Property(x => x.TonGiao).HasMaxLength(20);
                e.Property(x => x.DiaChi).HasMaxLength(255);

                // FK -> PhanQuyen
                e.Property(x => x.MaQuyen).IsRequired();
                e.HasOne<PhanQuyen>()
                 .WithMany()
                 .HasForeignKey(x => x.MaQuyen)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // GIANGVIEN (PK = MaNguoiDung; MaGiangVien UNIQUE)
            // =========================
            modelBuilder.Entity<GiangVien>(e =>
            {
                e.ToTable("GiangVien");
                e.HasKey(x => x.MaNguoiDung); // PK đúng theo SQL
                e.Property(x => x.MaNguoiDung).HasMaxLength(50).IsRequired();

                e.Property(x => x.MaGiangVien).HasMaxLength(20).IsRequired();
                e.HasIndex(x => x.MaGiangVien).IsUnique(); // UNIQUE

                e.Property(x => x.Khoa).HasMaxLength(100);
                e.Property(x => x.HocHam).HasMaxLength(50);
                e.Property(x => x.HocVi).HasMaxLength(50);
                e.Property(x => x.NgayTuyenDung).HasColumnType("date");

                e.HasOne<NguoiDung>()
                 .WithMany()
                 .HasForeignKey(x => x.MaNguoiDung)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // SINHVIEN (PK = MaNguoiDung; MaSinhVien UNIQUE)
            // =========================
            modelBuilder.Entity<SinhVien>(e =>
            {
                e.ToTable("SinhVien");
                e.HasKey(x => x.MaNguoiDung); // PK đúng theo SQL
                e.Property(x => x.MaNguoiDung).HasMaxLength(50).IsRequired();

                e.Property(x => x.MaSinhVien).HasMaxLength(20).IsRequired();
                e.HasIndex(x => x.MaSinhVien).IsUnique(); // UNIQUE

                e.Property(x => x.LopHanhChinh).HasMaxLength(50);
                e.Property(x => x.NamNhapHoc).IsRequired();
                e.Property(x => x.Khoa).HasMaxLength(50);
                e.Property(x => x.Nganh).HasMaxLength(50);

                e.HasOne<NguoiDung>()
                    .WithMany()
                    .HasForeignKey(x => x.MaNguoiDung)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // LICHSUHOATDONG
            // =========================
            modelBuilder.Entity<LichSuHoatDong>(e =>
            {
                e.ToTable("LichSuHoatDong");
                e.HasKey(x => x.MaLichSu);
                e.Property(x => x.MaLichSu).ValueGeneratedOnAdd(); // IDENTITY
                e.Property(x => x.ThoiGian).HasColumnType("datetime")
                                           .HasDefaultValueSql("GETDATE()")
                                           .IsRequired();
                e.Property(x => x.HanhDong).HasMaxLength(200).IsRequired();

                e.Property(x => x.MaNguoiDung).HasMaxLength(50).IsRequired();
                e.HasOne<NguoiDung>()
                 .WithMany()
                 .HasForeignKey(x => x.MaNguoiDung)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // MONHOC
            // =========================
            modelBuilder.Entity<MonHoc>(e =>
            {
                e.ToTable("MonHoc");
                e.HasKey(x => x.MaMonHoc);
                e.Property(x => x.MaMonHoc).HasMaxLength(20).IsRequired();
                e.Property(x => x.TenMonHoc).HasMaxLength(100).IsRequired();
                e.Property(x => x.SoTinChi).HasColumnType("tinyint").IsRequired();
                e.Property(x => x.SoTiet).HasColumnType("tinyint").IsRequired(); // NOT NULL
                e.Property(x => x.HocKy).HasColumnType("tinyint");               // NULL
                e.Property(x => x.MoTa).HasMaxLength(200);
                e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();
            });

            // =========================
            // LOPHOCPHAN
            // =========================
            modelBuilder.Entity<LopHocPhan>(e =>
            {
                e.ToTable("LopHocPhan");
                e.HasKey(x => x.MaLopHocPhan);
                e.Property(x => x.MaLopHocPhan).HasMaxLength(20).IsRequired();
                e.Property(x => x.TenLopHocPhan).HasMaxLength(100).IsRequired();
                e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();

                e.Property(x => x.MaMonHoc).HasMaxLength(20).IsRequired();
                e.Property(x => x.MaGiangVien).HasMaxLength(20).IsRequired();

                e.HasOne<MonHoc>()
                 .WithMany()
                 .HasForeignKey(x => x.MaMonHoc)
                 .OnDelete(DeleteBehavior.Restrict);

                // Tham chiếu GiangVien qua MaGiangVien (unique), không phải PK:
                e.HasOne<GiangVien>()
                 .WithMany()
                 .HasForeignKey(x => x.MaGiangVien)
                 .HasPrincipalKey(g => g.MaGiangVien)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // PHONGHOC
            // =========================
            modelBuilder.Entity<PhongHoc>(e =>
            {
                e.ToTable("PhongHoc");
                e.HasKey(x => x.MaPhong);
                e.Property(x => x.MaPhong).ValueGeneratedOnAdd(); // IDENTITY
                e.Property(x => x.TenPhong).HasMaxLength(100).IsRequired();
                e.Property(x => x.ToaNha).HasMaxLength(100);
                e.Property(x => x.Tang).HasMaxLength(20);
                e.Property(x => x.SucChua).HasColumnType("tinyint").IsRequired();
                e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();
            });

            // =========================
            // BUOIHOC
            // =========================
            modelBuilder.Entity<BuoiHoc>(e =>
            {
                e.ToTable("BuoiHoc");
                e.HasKey(x => x.MaBuoi);
                e.Property(x => x.MaBuoi).ValueGeneratedOnAdd(); // IDENTITY
                e.Property(x => x.NgayHoc).HasColumnType("date").IsRequired();
                e.Property(x => x.TietBatDau).HasColumnType("tinyint").IsRequired();
                e.Property(x => x.SoTiet).HasColumnType("tinyint").IsRequired();
                e.Property(x => x.GhiChu).HasMaxLength(200);

                e.Property(x => x.MaLop).HasMaxLength(20).IsRequired();
                e.Property(x => x.MaPhong).IsRequired();

                e.HasOne<LopHocPhan>()
                 .WithMany()
                 .HasForeignKey(x => x.MaLop)
                 .HasPrincipalKey(l => l.MaLopHocPhan)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<PhongHoc>()
                 .WithMany()
                 .HasForeignKey(x => x.MaPhong)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // THAMGIALOP
            // =========================
            modelBuilder.Entity<ThamGiaLop>(e =>
            {
                e.ToTable("ThamGiaLop");
                // e.HasKey(x => x.MaThamGia);
                // e.Property(x => x.MaThamGia).ValueGeneratedOnAdd(); // IDENTITY
                e.HasKey(x => new { x.MaSinhVien, x.MaLopHocPhan });
                e.Property(x => x.NgayThamGia).HasColumnType("date").IsRequired();
                e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();

                e.Property(x => x.MaSinhVien).HasMaxLength(20).IsRequired();
                e.Property(x => x.MaLopHocPhan).HasMaxLength(20).IsRequired();

                // e.HasIndex(x => new { x.MaSinhVien, x.MaLopHocPhan }).IsUnique(); // UQ_ThamGia

                e.HasOne<SinhVien>()
                 .WithMany()
                 .HasForeignKey(x => x.MaSinhVien)
                 .HasPrincipalKey(s => s.MaSinhVien)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<LopHocPhan>()
                 .WithMany()
                 .HasForeignKey(x => x.MaLopHocPhan)
                 .HasPrincipalKey(l => l.MaLopHocPhan)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // TRANGTHAIDIEMDANH
            // =========================
            modelBuilder.Entity<TrangThaiDiemDanh>(e =>
            {
                e.ToTable("TrangThaiDiemDanh");
                e.HasKey(x => x.MaTrangThai);
                e.Property(x => x.MaTrangThai).ValueGeneratedOnAdd(); // IDENTITY
                e.Property(x => x.TenTrangThai).HasMaxLength(50).IsRequired();
                e.Property(x => x.CodeTrangThai).HasMaxLength(30).IsRequired();
                e.HasIndex(x => x.CodeTrangThai).IsUnique();
            });

            // =========================
            // DIEMDANH
            // =========================
            modelBuilder.Entity<DiemDanh>(e =>
            {
                e.ToTable("DiemDanh");
                e.HasKey(x => x.MaDiemDanh);
                e.Property(x => x.MaDiemDanh).ValueGeneratedOnAdd(); // IDENTITY
                e.Property(x => x.ThoiGianQuet).HasColumnType("datetime").IsRequired(); // KHÔNG default trong SQL
                e.Property(x => x.LyDo).HasMaxLength(200);
                e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();

                e.Property(x => x.MaTrangThai).IsRequired();
                e.Property(x => x.MaBuoi).IsRequired();
                e.Property(x => x.MaSinhVien).HasMaxLength(20).IsRequired();

                e.HasIndex(x => new { x.MaBuoi, x.MaSinhVien }).IsUnique(); // UQ_DiemDanh_Lich_SV

                e.HasOne<TrangThaiDiemDanh>()
                 .WithMany()
                 .HasForeignKey(x => x.MaTrangThai)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<BuoiHoc>()
                 .WithMany()
                 .HasForeignKey(x => x.MaBuoi)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<SinhVien>()
                 .WithMany()
                 .HasForeignKey(x => x.MaSinhVien)
                 .HasPrincipalKey(s => s.MaSinhVien)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
