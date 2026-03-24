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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình kiểu dữ liệu tiền tệ cho SQL Server
            modelBuilder.Entity<SanPham>().Property(p => p.GiaTien).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<DonHang>().Property(d => d.TongTien).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ChiTietDonHang>().Property(c => c.GiaLucMua).HasColumnType("decimal(18,2)");

            // Ràng buộc khóa ngoại
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(c => c.DonHang)
                .WithMany(d => d.ChiTietDonHangs)
                .HasForeignKey(c => c.DonHangId);
        }
    }
}