using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Thêm thư viện này để dùng SelectList
using Microsoft.EntityFrameworkCore;
using OceanShellCraft.Models;
using System.IO;

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

        // ==========================================
        // QUẢN LÝ BÀI VIẾT
        // ==========================================

        // 1. TRANG DANH SÁCH BÀI VIẾT (LẤY TỪ SQL)
        public async Task<IActionResult> BaiViet()
        {
            // Lấy tất cả bài viết, sắp xếp bài mới nhất lên đầu
            var danhSach = await _context.BaiViets.OrderByDescending(b => b.NgayTao).ToListAsync();
            return View(danhSach);
        }

        // 2. HIỂN THỊ FORM THÊM/SỬA BÀI VIẾT
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

        // 4. XÓA BÀI VIẾT
        public async Task<IActionResult> XoaBaiViet(int id)
        {
            // Tìm bài viết cần xóa trong Database
            var baiViet = await _context.BaiViets.FindAsync(id);

            if (baiViet != null)
            {
                // 1. Xóa file ảnh vật lý trong thư mục wwwroot (Nếu có ảnh)
                if (!string.IsNullOrEmpty(baiViet.AnhNen))
                {
                    // Lấy đường dẫn chuẩn xác tới file ảnh
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", baiViet.AnhNen.TrimStart('/'));

                    // Nếu file tồn tại thì xóa nó đi
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // 2. Xóa dữ liệu trong SQL
                _context.BaiViets.Remove(baiViet);
                await _context.SaveChangesAsync();
            }

            // Xóa xong thì load lại trang danh sách
            return RedirectToAction("BaiViet");
        }

        // ==========================================
        // QUẢN LÝ SẢN PHẨM
        // ==========================================

        // 5. TRANG DANH SÁCH SẢN PHẨM
        public async Task<IActionResult> SanPham()
        {
            // Lấy danh sách sản phẩm, nếu có DanhMuc thì Include vào
            var danhSach = await _context.SanPhams.Include(s => s.DanhMuc).ToListAsync();
            return View(danhSach);
        }

        // 5.1 HIỂN THỊ FORM THÊM/SỬA SẢN PHẨM
        [HttpGet]
        public async Task<IActionResult> ThemSuaSanPham(int? id)
        {
            // Lấy danh sách Danh mục để đổ vào DropdownList (Thẻ Select)
            var danhMucs = await _context.DanhMucs.ToListAsync();
            ViewBag.DanhMucList = new SelectList(danhMucs, "Id", "TenDanhMuc");

            if (id.HasValue)
            {
                ViewData["Title"] = "Sửa sản phẩm";
                var sanPham = await _context.SanPhams.FindAsync(id);
                if (sanPham == null) return NotFound();

                return View(sanPham);
            }
            else
            {
                ViewData["Title"] = "Thêm mới sản phẩm";
                return View(new SanPham());
            }
        }

        // 5.2 XỬ LÝ LƯU THÊM/SỬA SẢN PHẨM
        [HttpPost]
        public async Task<IActionResult> ThemSuaSanPham(SanPham model, IFormFile? fileHinhAnh)
        {
            // Xử lý upload ảnh sản phẩm
            if (fileHinhAnh != null && fileHinhAnh.Length > 0)
            {
                // Lưu vào thư mục riêng cho sản phẩm (wwwroot/images/sanpham)
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "sanpham");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileHinhAnh.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileHinhAnh.CopyToAsync(fileStream);
                }
                model.HinhAnh = "/images/sanpham/" + fileName;
            }

            if (model.Id == 0)
            {
                // Thêm mới
                _context.SanPhams.Add(model);
            }
            else
            {
                // Sửa (Cập nhật)
                if (string.IsNullOrEmpty(model.HinhAnh))
                {
                    var spCu = await _context.SanPhams.AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.Id);
                    if (spCu != null) model.HinhAnh = spCu.HinhAnh;
                }
                _context.SanPhams.Update(model);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("SanPham");
        }

        // 5.3 XÓA SẢN PHẨM
        public async Task<IActionResult> XoaSanPham(int id)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham != null)
            {
                // Xóa ảnh vật lý
                if (!string.IsNullOrEmpty(sanPham.HinhAnh))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", sanPham.HinhAnh.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.SanPhams.Remove(sanPham);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("SanPham");
        }

        // ---> API XỬ LÝ BẤM NGÔI SAO NỔI BẬT <---
        [HttpPost]
        [Route("Admin/ToggleNoiBat/{id}")]
        public async Task<IActionResult> ToggleNoiBat(int id)
        {
            // 1. Tìm sản phẩm trong DB
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            // 2. Đảo ngược trạng thái (đang bật thành tắt, đang tắt thành bật)
            sanPham.IsFeatured = !sanPham.IsFeatured;

            // 3. Lưu vào Database
            _context.Update(sanPham);
            await _context.SaveChangesAsync();

            // 4. Trả về kết quả cho Javascript (để JS biết đường đổi màu sao)
            return Ok(new { success = true, isFeatured = sanPham.IsFeatured });
        }

        // ==========================================
        // CÁC TRANG KHÁC
        // ==========================================

        // 6. TRANG TIN NHẮN (GIAO DIỆN CHAT)
        public IActionResult Messages()
        {
            return View();
        }

        // 7. TRANG QUẢN LÝ HÓA ĐƠN (BILLS)
        public IActionResult HoaDon()
        {
            return View();
        }

        // 8. TRANG SETTINGS (CÀI ĐẶT TÀI KHOẢN)
        public IActionResult Settings()
        {
            return View();
        }
    }
}