using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs; // Chỉ cần anh bạn này là đủ cân cả thế giới
using OceanShellCraft.Models;
using System.Data;

namespace OceanShellCraft.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly MyNgheDbContext _context;

        public SanPhamController(MyNgheDbContext context)
        {
            _context = context;
        }

        // 1. Hiển thị danh sách sản phẩm
        // 1. Hiển thị danh sách sản phẩm (Đã thêm Phân trang và Lọc theo danh mục)
        public async Task<IActionResult> DanhSach(int? maDanhMuc, int page = 1)
        {
            int pageSize = 6; // Số sản phẩm hiển thị trên 1 trang (giống ảnh mẫu là 6)

            // Lấy danh sách danh mục để hiển thị lên Menu Lọc
            ViewBag.DanhMucs = await _context.DanhMucs.ToListAsync();
            ViewBag.CurrentCategory = maDanhMuc; // Lưu lại danh mục đang chọn để tô màu vàng

            var sanPhams = _context.SanPhams.Include(s => s.DanhMuc).AsQueryable();

            if (maDanhMuc.HasValue)
            {
                sanPhams = sanPhams.Where(s => s.DanhMucId == maDanhMuc);
            }

            // Tính toán số trang
            int totalItems = await sanPhams.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            // Lấy dữ liệu của trang hiện tại
            var pagedData = await sanPhams.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return View(pagedData);
        }

        // 2. Trang tạo mới sản phẩm (Giao diện)
        [HttpGet]
        public IActionResult TaoMoi()
        {
            ViewBag.MaDanhMuc = new SelectList(_context.DanhMucs, "Id", "TenDanhMuc");
            return View();
        }

        // 3. Xử lý tạo mới sản phẩm thủ công
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoMoi([Bind("TenSanPham,MoTa,GiaTien,ChatLieu,KichThuoc,DanhMucId")] SanPham sanPham, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string tenFile = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string duongDanLuu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", tenFile);

                    using (var stream = new FileStream(duongDanLuu, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    sanPham.HinhAnh = "/img/" + tenFile;
                }

                _context.Add(sanPham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(DanhSach));
            }

            ViewBag.MaDanhMuc = new SelectList(_context.DanhMucs, "Id", "TenDanhMuc", sanPham.DanhMucId);
            return View(sanPham);
        }

        // 4. Xử lý Nhập Excel bằng MiniExcel
        [HttpPost]
        public async Task<IActionResult> NhapExcel(IFormFile file)
        {
            if (file == null || file.Length == 0) return RedirectToAction("TaoMoi");

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var rows = stream.Query().ToList();
                    var dsSanPham = new List<SanPham>();

                    foreach (var row in rows.Skip(1))
                    {
                        string tenSP = row.A?.ToString();
                        if (string.IsNullOrWhiteSpace(tenSP)) continue;

                        dsSanPham.Add(new SanPham
                        {
                            TenSanPham = tenSP,
                            MoTa = row.B?.ToString() ?? "",
                            // FIX LỖI: Chỉ định rõ 'out decimal' và 'out int' thay vì 'out var'
                            GiaTien = decimal.TryParse(row.C?.ToString(), out decimal gia) ? gia : 0,
                            HinhAnh = row.D?.ToString() ?? "",
                            ChatLieu = row.E?.ToString() ?? "",
                            KichThuoc = row.F?.ToString() ?? "",
                            DanhMucId = int.TryParse(row.G?.ToString(), out int id) ? id : 1
                        });
                    }

                    if (dsSanPham.Any())
                    {
                        _context.SanPhams.AddRange(dsSanPham);
                        await _context.SaveChangesAsync();
                    }
                }
                return RedirectToAction("DanhSach");
            }
            catch (Exception ex)
            {
                return Content("Lỗi MiniExcel: " + ex.Message);
            }
        }

        // 5. Xuất file mẫu bằng MiniExcel (Siêu ngắn, không cần EPPlus)
        public IActionResult TaiFileMau()
        {
            // Tạo 1 dòng dữ liệu mẫu cực kỳ gọn gàng
            var dataMau = new[]
            {
                new {
                    TenSanPham = "Đèn ngủ vỏ ốc xà cừ",
                    MoTa = "Sản phẩm thủ công tinh xảo",
                    GiaTien = 250000,
                    HinhAnh = "/img/mau.jpg",
                    ChatLieu = "Vỏ ốc tự nhiên",
                    KichThuoc = "20cm",
                    DanhMucId = 1
                }
            };

            var stream = new MemoryStream();
            // MiniExcel tự động tạo file Excel từ mảng dữ liệu ở trên
            stream.SaveAs(dataMau);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Nhap_San_Pham.xlsx");
        }
        public async Task<IActionResult> ChiTiet(int id)
        {
            var sanPham = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sanPham == null) return NotFound();

            return View(sanPham);
        }
    }
}