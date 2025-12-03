// File: Data/AppDbContext.cs
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PhanQuyen> PhanQuyen { get; set; } = null!;
    public DbSet<ChucNang> ChucNang { get; set; } = null!;
    public DbSet<NhomChucNang> NhomChucNang { get; set; } = null!;
    public DbSet<Khoa> Khoa { get; set; } = null!;
    public DbSet<Nganh> Nganh { get; set; } = null!;
    public DbSet<NguoiDung> NguoiDung { get; set; } = null!;
    public DbSet<GiangVien> GiangVien { get; set; } = null!;
    public DbSet<SinhVien> SinhVien { get; set; } = null!;
    public DbSet<LichSuHoatDong> LichSuHoatDong { get; set; } = null!;
    public DbSet<HocKy> HocKy { get; set; } = null!;
    public DbSet<MonHoc> MonHoc { get; set; } = null!;
    public DbSet<LopHocPhan> LopHocPhan { get; set; } = null!;
    public DbSet<PhongHoc> PhongHoc { get; set; } = null!;
    public DbSet<BuoiHoc> BuoiHoc { get; set; } = null!;
    public DbSet<ThamGiaLop> ThamGiaLop { get; set; } = null!;
    public DbSet<TrangThaiDiemDanh> TrangThaiDiemDanh { get; set; } = null!;
    public DbSet<DiemDanh> DiemDanh { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PHANQUYEN
        modelBuilder.Entity<PhanQuyen>(e =>
        {
            e.ToTable("PhanQuyen");
            e.HasKey(x => x.MaQuyen);
            e.Property(x => x.MaQuyen).ValueGeneratedOnAdd();
            e.Property(x => x.CodeQuyen).HasMaxLength(50).IsRequired();
            e.Property(x => x.TenQuyen).HasMaxLength(50).IsRequired();
            e.Property(x => x.MoTa).HasMaxLength(200);
        });

        // CHUCNANG
        modelBuilder.Entity<ChucNang>(e =>
        {
            e.ToTable("ChucNang");
            e.HasKey(x => x.MaChucNang);
            e.Property(x => x.MaChucNang).ValueGeneratedOnAdd();
            e.Property(x => x.CodeChucNang).HasMaxLength(50).IsRequired();
            e.Property(x => x.TenChucNang).HasMaxLength(100).IsRequired();
            e.Property(x => x.MoTa).HasMaxLength(200);
            e.Property(x => x.Url).HasMaxLength(255);
            e.Property(x => x.Stt);

            // Self reference: ParentChucNangId -> ChucNang.MaChucNang
            e.Property(x => x.ParentChucNangId);
            e.HasOne<ChucNang>()                   // parent
             .WithMany()                           // (không khai báo Children trong model)
             .HasForeignKey(x => x.ParentChucNangId)
             .OnDelete(DeleteBehavior.Restrict);   // tránh xóa cha kéo theo con
        });

        // NHOMCHUCNANG
        modelBuilder.Entity<NhomChucNang>(e =>
        {
            e.ToTable("NhomChucNang");
            e.HasKey(x => new { x.MaQuyen, x.MaChucNang });
            e.Property(x => x.MaQuyen).IsRequired();
            e.Property(x => x.MaChucNang).IsRequired();
            e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();

            e.HasOne<PhanQuyen>()
             .WithMany()
             .HasForeignKey(x => x.MaQuyen)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<ChucNang>()
             .WithMany()
             .HasForeignKey(x => x.MaChucNang)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // KHOA
        modelBuilder.Entity<Khoa>(e =>
        {
            e.ToTable("Khoa");
            e.HasKey(x => x.MaKhoa);
            e.Property(x => x.MaKhoa).ValueGeneratedOnAdd();
            e.Property(x => x.CodeKhoa).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.CodeKhoa).IsUnique();
            e.Property(x => x.TenKhoa).HasMaxLength(150).IsRequired();
        });

        // NGANH
        modelBuilder.Entity<Nganh>(e =>
        {
            e.ToTable("Nganh");
            e.HasKey(x => x.MaNganh);
            e.Property(x => x.MaNganh).ValueGeneratedOnAdd();
            e.Property(x => x.CodeNganh).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.CodeNganh).IsUnique();
            e.Property(x => x.TenNganh).HasMaxLength(100).IsRequired();
            e.Property(x => x.MaKhoa).IsRequired();

            e.HasIndex(x => new { x.TenNganh, x.MaKhoa }).IsUnique();

            e.HasOne<Khoa>()
             .WithMany()
             .HasForeignKey(x => x.MaKhoa)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // NGUOIDUNG
        modelBuilder.Entity<NguoiDung>(e =>
        {
            e.ToTable("NguoiDung");
            e.HasKey(x => x.MaNguoiDung);
            e.Property(x => x.MaNguoiDung).ValueGeneratedOnAdd();

            e.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
            e.Property(x => x.GioiTinh);
            e.Property(x => x.AnhDaiDien).HasMaxLength(255);
            e.Property(x => x.Email).HasMaxLength(100);
            e.Property(x => x.SoDienThoai).HasMaxLength(15);
            e.Property(x => x.NgaySinh).HasColumnType("date");
            e.Property(x => x.DiaChi).HasMaxLength(255);

            e.Property(x => x.TenDangNhap).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.TenDangNhap).IsUnique();

            e.Property(x => x.MatKhau).HasMaxLength(200).IsRequired();
            e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();

            e.Property(x => x.RefreshTokenHash).HasMaxLength(200);
            e.Property(x => x.RefreshTokenIssuedAt);
            e.Property(x => x.RefreshTokenExpiresAt);
            e.Property(x => x.RefreshTokenId);
            e.Property(x => x.RefreshTokenRevokedAt);

            e.Property(x => x.MaQuyen);

            e.HasOne<PhanQuyen>()
             .WithMany()
             .HasForeignKey(x => x.MaQuyen)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // GIANGVIEN (add trigger mapping)
        modelBuilder.Entity<GiangVien>(e =>
        {
            e.ToTable("GiangVien", tb => tb.HasTrigger("trg_GiangVien_Exclusive"));
            e.HasKey(x => x.MaNguoiDung);

            e.Property(x => x.MaGiangVien).HasMaxLength(20).IsRequired();

            // Ensure MaGiangVien is an alternate key (principal for FK from LopHocPhan)
            e.HasAlternateKey(x => x.MaGiangVien).HasName("AK_GiangVien_MaGiangVien");

            // e.HasIndex(x => x.MaGiangVien).IsUnique(); // not needed when using alternate key

            e.Property(x => x.MaKhoa);
            e.Property(x => x.HocHam).HasMaxLength(50);
            e.Property(x => x.HocVi).HasMaxLength(50);
            e.Property(x => x.NgayTuyenDung).HasColumnType("date");

            e.HasOne<NguoiDung>()
             .WithMany()
             .HasForeignKey(x => x.MaNguoiDung)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<Khoa>()
             .WithMany()
             .HasForeignKey(x => x.MaKhoa)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // SINHVIEN (add trigger mapping)
        modelBuilder.Entity<SinhVien>(e =>
        {
            e.ToTable("SinhVien", tb => tb.HasTrigger("trg_SinhVien_Exclusive"));
            e.HasKey(x => x.MaNguoiDung);

            e.Property(x => x.MaSinhVien).HasMaxLength(20).IsRequired();

            // Make MaSinhVien an alternate key (you reference it from ThamGiaLop & DiemDanh)
            e.HasAlternateKey(x => x.MaSinhVien).HasName("AK_SinhVien_MaSinhVien");

            // Optional: map LopHanhChinh if present in model
            // e.Property(x => x.LopHanhChinh).HasMaxLength(50);

            e.Property(x => x.NamNhapHoc).IsRequired();
            e.Property(x => x.MaNganh);

            e.HasOne<NguoiDung>()
             .WithMany()
             .HasForeignKey(x => x.MaNguoiDung)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<Nganh>()
             .WithMany()
             .HasForeignKey(x => x.MaNganh)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // LICHSUHOATDONG
        modelBuilder.Entity<LichSuHoatDong>(e =>
        {
            e.ToTable("LichSuHoatDong");
            e.HasKey(x => x.MaLichSu);
            e.Property(x => x.MaLichSu).ValueGeneratedOnAdd();
            e.Property(x => x.ThoiGian).HasDefaultValueSql("GETDATE()");
            e.Property(x => x.HanhDong).HasMaxLength(200).IsRequired();
            e.Property(x => x.MaNguoiDung);

            e.HasOne<NguoiDung>()
             .WithMany()
             .HasForeignKey(x => x.MaNguoiDung)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // HOCKY
        modelBuilder.Entity<HocKy>(e =>
        {
            e.ToTable("HocKy");
            e.HasKey(x => x.MaHocKy);
            e.Property(x => x.MaHocKy).ValueGeneratedOnAdd();
            e.Property(x => x.NamHoc).IsRequired();
            e.Property(x => x.Ky).IsRequired();

            e.HasIndex(x => new { x.NamHoc, x.Ky }).IsUnique();
            e.HasCheckConstraint("CK_HocKy_Ky", "[Ky] IN (1,2,3)");
        });

        // MONHOC
        modelBuilder.Entity<MonHoc>(e =>
        {
            e.ToTable("MonHoc");
            e.HasKey(x => x.MaMonHoc);
            e.Property(x => x.MaMonHoc).HasMaxLength(20).ValueGeneratedNever();
            e.Property(x => x.TenMonHoc).HasMaxLength(100).IsRequired();
            e.Property(x => x.SoTinChi).IsRequired();
            e.Property(x => x.SoTiet).IsRequired();
            e.Property(x => x.MoTa).HasMaxLength(200);
            e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();
            e.Property(x => x.LoaiMon).HasDefaultValue((byte)1).IsRequired();
            e.HasCheckConstraint("CK_MonHoc_Loai", "[LoaiMon] IN (1,2,3)");
        });

        // LOPHOCPHAN
        modelBuilder.Entity<LopHocPhan>(e =>
        {
            e.ToTable("LopHocPhan");
            e.HasKey(x => x.MaLopHocPhan);
            e.Property(x => x.MaLopHocPhan).HasMaxLength(20).ValueGeneratedNever();
            e.Property(x => x.TenLopHocPhan).HasMaxLength(100).IsRequired();
            e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();

            e.Property(x => x.MaMonHoc).HasMaxLength(20).IsRequired();
            e.Property(x => x.MaGiangVien).HasMaxLength(20);
            e.Property(x => x.MaHocKy).IsRequired();

            e.HasOne<MonHoc>()
             .WithMany()
             .HasForeignKey(x => x.MaMonHoc)
             .OnDelete(DeleteBehavior.Restrict);

            // IMPORTANT: FK is to GiangVien.MaGiangVien (alternate key), not PK MaNguoiDung
            e.HasOne<GiangVien>()
             .WithMany()
             .HasForeignKey(x => x.MaGiangVien)
             .HasPrincipalKey(g => g.MaGiangVien)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne<HocKy>()
             .WithMany()
             .HasForeignKey(x => x.MaHocKy)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // PHONGHOC
        modelBuilder.Entity<PhongHoc>(e =>
        {
            e.ToTable("PhongHoc");
            e.HasKey(x => x.MaPhong);
            e.Property(x => x.MaPhong).ValueGeneratedOnAdd();
            e.Property(x => x.TenPhong).HasMaxLength(100).IsRequired();
            e.Property(x => x.ToaNha).HasMaxLength(100);
            e.Property(x => x.Tang);
            e.Property(x => x.SucChua).IsRequired();
            e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();
        });

        // BUOIHOC
        modelBuilder.Entity<BuoiHoc>(e =>
        {
            e.ToTable("BuoiHoc");
            e.HasKey(x => x.MaBuoi);
            e.Property(x => x.MaBuoi).ValueGeneratedOnAdd();
            e.Property(x => x.NgayHoc).HasColumnType("date").IsRequired();
            e.Property(x => x.TietBatDau).IsRequired();
            e.Property(x => x.SoTiet).IsRequired();
            e.Property(x => x.GhiChu).HasMaxLength(200);
            e.Property(x => x.MaLopHocPhan).HasMaxLength(20).IsRequired();
            e.Property(x => x.MaPhong);
            e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();

            e.HasOne<LopHocPhan>()
             .WithMany()
             .HasForeignKey(x => x.MaLopHocPhan)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<PhongHoc>()
             .WithMany()
             .HasForeignKey(x => x.MaPhong)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // THAMGIALOP
        modelBuilder.Entity<ThamGiaLop>(e =>
        {
            e.ToTable("ThamGiaLop");
            e.HasKey(x => new { x.MaSinhVien, x.MaLopHocPhan });
            e.Property(x => x.NgayThamGia).HasColumnType("date").IsRequired();
            e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();
            e.Property(x => x.MaSinhVien).HasMaxLength(20).IsRequired();
            e.Property(x => x.MaLopHocPhan).HasMaxLength(20).IsRequired();

            e.HasOne<SinhVien>()
             .WithMany()
             .HasForeignKey(x => x.MaSinhVien)
             .HasPrincipalKey(s => s.MaSinhVien)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<LopHocPhan>()
             .WithMany()
             .HasForeignKey(x => x.MaLopHocPhan)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // TRANGTHAIDIEMDANH
        modelBuilder.Entity<TrangThaiDiemDanh>(e =>
        {
            e.ToTable("TrangThaiDiemDanh");
            e.HasKey(x => x.MaTrangThai);
            e.Property(x => x.MaTrangThai).ValueGeneratedOnAdd();
            e.Property(x => x.TenTrangThai).HasMaxLength(50).IsRequired();
            e.Property(x => x.CodeTrangThai).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.CodeTrangThai).IsUnique();
        });

        // DIEMDANH
        modelBuilder.Entity<DiemDanh>(e =>
        {
            e.ToTable("DiemDanh");
            e.HasKey(x => x.MaDiemDanh);
            e.Property(x => x.MaDiemDanh).ValueGeneratedOnAdd();
            e.Property(x => x.ThoiGianQuet).IsRequired();
            e.Property(x => x.MaTrangThai).IsRequired();
            e.Property(x => x.LyDo).HasMaxLength(200);
            e.Property(x => x.TrangThai).HasColumnType("bit").HasDefaultValue(true).IsRequired();
            e.Property(x => x.MaBuoi).IsRequired();
            e.Property(x => x.MaSinhVien).HasMaxLength(20).IsRequired();

            e.HasIndex(x => new { x.MaBuoi, x.MaSinhVien }).IsUnique();

            e.HasOne<TrangThaiDiemDanh>()
             .WithMany()
             .HasForeignKey(x => x.MaTrangThai)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<BuoiHoc>()
             .WithMany()
             .HasForeignKey(x => x.MaBuoi)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<SinhVien>()
             .WithMany()
             .HasForeignKey(x => x.MaSinhVien)
             .HasPrincipalKey(s => s.MaSinhVien)
             .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}