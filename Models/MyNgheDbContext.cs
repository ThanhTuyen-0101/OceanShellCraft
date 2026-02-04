using Microsoft.EntityFrameworkCore;

namespace OceanShellCraft.Models
{
    public class MyNgheDbContext: DbContext
    {
        public MyNgheDbContext(DbContextOptions<MyNgheDbContext> options)
            : base(options)
        {
        }

        // Khai báo bảng Sản phẩm
        public DbSet<SanPham> SanPhams { get; set; }

        // Khai báo bảng Danh mục
        public DbSet<DanhMuc> DanhMucs { get; set; }
    }
}
