using Microsoft.EntityFrameworkCore;
using OceanShellCraft.Models;

namespace OceanShellCraft.Models
{
    public class MyNgheDbContext : DbContext
    {
        public MyNgheDbContext(DbContextOptions<MyNgheDbContext> options) : base(options) { }

        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<BaiViet> BaiViets { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }

        // --- THÊM BẢNG ĐÁNH GIÁ BÀI VIẾT VÀO ĐÂY ---
        public DbSet<DanhGiaBaiViet> DanhGiaBaiViets { get; set; }

        public DbSet<GiamGia> GiamGias { get; set; }
        public DbSet<TinNhan> TinNhans { get; set; }
        public DbSet<SanPhamYeuThich> SanPhamYeuThiches { get; set; }
        public DbSet<BienTheSanPham> BienTheSanPhams { get; set; }
        public DbSet<LichSuTonKho> LichSuTonKhos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình decimal cho các bảng
            modelBuilder.Entity<SanPham>().Property(p => p.GiaTien).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SanPham>().Property(p => p.GiaKhuyenMai).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<BienTheSanPham>().Property(b => b.GiaRieng).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<DonHang>().Property(d => d.TongTien).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ChiTietDonHang>().Property(c => c.GiaLucMua).HasColumnType("decimal(18,2)");

            // Cấu hình quan hệ 1-Nhiều (Sản phẩm - Biến thể)
            modelBuilder.Entity<BienTheSanPham>()
                .HasOne(b => b.SanPham)
                .WithMany(s => s.BienThes)
                .HasForeignKey(b => b.SanPhamId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa sản phẩm thì xóa luôn biến thể

            // Ràng buộc khóa ngoại (Chi tiết đơn hàng - Đơn hàng)
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(c => c.DonHang)
                .WithMany(d => d.ChiTietDonHangs)
                .HasForeignKey(c => c.DonHangId);

            // --- CẤU HÌNH QUAN HỆ CHO BẢNG ĐÁNH GIÁ BÀI VIẾT ---
            modelBuilder.Entity<DanhGiaBaiViet>()
        .HasOne(d => d.BaiViet)
        .WithMany(b => b.DanhGiaBaiViets)
        .HasForeignKey(d => d.BaiVietId)
        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DanhGiaBaiViet>()
    .HasOne(d => d.NguoiDung)
    .WithMany()
    .HasForeignKey(d => d.NguoiDungId)
    .OnDelete(DeleteBehavior.Restrict);// Giữ lại Restrict để tránh lỗi multiple cascade paths trong SQL Server
        }
    }
}