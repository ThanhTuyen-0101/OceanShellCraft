using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OceanShellCraft.Models;

namespace OceanShellCraft.Controllers
{
    public class AdminController : Controller
    {
        // Khai báo biến kết nối Database
        private readonly MyNgheDbContext _context;

        // Tiêm DbContext vào Controller
        public AdminController(MyNgheDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // 1. TRANG DANH SÁCH BÀI VIẾT (LẤY TỪ SQL)
        public async Task<IActionResult> BaiViet()
        {
            // Lấy tất cả bài viết, sắp xếp bài mới nhất lên đầu
            var danhSach = await _context.BaiViets.OrderByDescending(b => b.NgayTao).ToListAsync();
            return View(danhSach);
        }

        // 2. HIỂN THỊ FORM THÊM/SỬA
        [HttpGet]
        public async Task<IActionResult> ThemSuaBaiViet(int? id)
        {
            if (id.HasValue)
            {
                ViewData["Title"] = "Sửa bài giới thiệu";
                // Tìm bài viết trong DB dựa vào ID
                var baiViet = await _context.BaiViets.FindAsync(id);
                if (baiViet == null) return NotFound();

                return View(baiViet); // Đổ dữ liệu cũ ra form
            }
            else
            {
                ViewData["Title"] = "Thêm mới bài giới thiệu";
                return View(new BaiViet());
            }
        }

        // 3. XỬ LÝ KHI BẤM NÚT "LƯU" (LƯU VÀO SQL)
        [HttpPost]
        public async Task<IActionResult> ThemSuaBaiViet(BaiViet model, IFormFile? fileAnhNen)
        {
            // Xử lý upload ảnh (nếu có chọn)
            if (fileAnhNen != null && fileAnhNen.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileAnhNen.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileAnhNen.CopyToAsync(fileStream);
                }
                model.AnhNen = "/images/" + fileName;
            }

            // Xử lý Lưu Database
            if (model.Id == 0)
            {
                // Nếu Id = 0 nghĩa là Thêm mới
                model.NgayTao = DateTime.Now;
                _context.BaiViets.Add(model);
            }
            else
            {
                // Nếu có Id nghĩa là Sửa (Cập nhật)
                // Lưu ý: Nếu sửa mà không chọn ảnh mới, thì phải giữ lại link ảnh cũ
                if (string.IsNullOrEmpty(model.AnhNen))
                {
                    var baiVietCu = await _context.BaiViets.AsNoTracking().FirstOrDefaultAsync(b => b.Id == model.Id);
                    if (baiVietCu != null) model.AnhNen = baiVietCu.AnhNen;
                }

                _context.BaiViets.Update(model);
            }

            // Lệnh này chính thức lưu dữ liệu vào SQL Server
            await _context.SaveChangesAsync();

            return RedirectToAction("BaiViet");
        }
    }
}