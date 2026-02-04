using Microsoft.AspNetCore.Mvc;

namespace OceanShellCraft.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly MyNgheDbContext _context;
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> DanhSach(int? danhMucId)
        {
            // 1. Tạo câu truy vấn cơ bản (chưa chạy xuống DB)
            // .Include(s => s.DanhMuc): Giống lệnh JOIN trong SQL để lấy tên danh mục
            var truyVan = _context.SanPhams.Include(s => s.DanhMuc).AsQueryable();

            // 2. Nếu người dùng chọn lọc theo danh mục (VD: bấm vào menu "Đèn ngủ")
            if (danhMucId.HasValue)
            {
                truyVan = truyVan.Where(s => s.DanhMucId == danhMucId);
            }

            // 3. Thực thi truy vấn và lấy dữ liệu về list (.ToListAsync())
            var danhSachSanPham = await truyVan.ToListAsync();

            // 4. Trả dữ liệu về View
            return View(danhSachSanPham);
        }
    }
}
