using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using OceanShellCraft.Models;
using Microsoft.AspNetCore.Authorization;
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

        #region 1. Danh sách Sản phẩm (Công khai cho mọi người)
        [AllowAnonymous] // Đảm bảo ai cũng vào được
        [HttpGet]
        public async Task<IActionResult> DanhSach(int? maDanhMuc, int page = 1)
        {
            int pageSize = 9; // Thường chia hết cho 3 để đẹp lưới Bootstrap (col-md-4)
            var query = _context.SanPhams.Include(s => s.DanhMuc).AsQueryable();

            if (maDanhMuc.HasValue)
            {
                query = query.Where(s => s.DanhMucId == maDanhMuc);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Gửi dữ liệu qua ViewBag để View hiển thị
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.CurrentCategory = maDanhMuc;
            ViewBag.DanhMucs = await _context.DanhMucs.ToListAsync();

            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return View(data); // Sẽ tìm file Views/SanPham/DanhSach.cshtml
        }
        #endregion

        #region 2. Chi tiết Sản phẩm (Công khai)
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var sanPham = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sanPham == null) return NotFound();

            return View(sanPham); // Sẽ tìm file Views/SanPham/ChiTiet.cshtml
        }
        #endregion

        #region 3. Thêm mới Sản phẩm (Chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> TaoMoi()
        {
            ViewBag.MaDanhMuc = new SelectList(await _context.DanhMucs.ToListAsync(), "Id", "TenDanhMuc");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoMoi(SanPham model, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    model.HinhAnh = await SaveImage(imageFile);
                }
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(DanhSach));
            }
            ViewBag.MaDanhMuc = new SelectList(await _context.DanhMucs.ToListAsync(), "Id", "TenDanhMuc", model.DanhMucId);
            return View(model);
        }
        #endregion

        #region 4. Xử lý Excel & Tiện ích
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> NhapExcel(IFormFile file)
        {
            if (file == null || file.Length == 0) return RedirectToAction(nameof(DanhSach));

            using (var stream = file.OpenReadStream())
            {
                var rows = stream.Query().ToList();
                var dsSanPham = new List<SanPham>();

                foreach (var row in rows.Skip(1))
                {
                    dsSanPham.Add(new SanPham
                    {
                        TenSanPham = row.A?.ToString(),
                        MoTa = row.B?.ToString() ?? "",
                        GiaTien = decimal.TryParse(row.C?.ToString(), out decimal gia) ? gia : 0,
                        HinhAnh = row.D?.ToString() ?? "",
                        ChatLieu = row.E?.ToString() ?? "",
                        KichThuoc = row.F?.ToString() ?? "",
                        DanhMucId = int.TryParse(row.G?.ToString(), out int id) ? id : 1
                    });
                }
                _context.SanPhams.AddRange(dsSanPham);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(DanhSach));
        }

        [AllowAnonymous]
        public IActionResult TaiFileMau()
        {
            var dataMau = new[] {
                new { TenSanPham = "Tên SP", MoTa = "Mô tả", GiaTien = 100000, HinhAnh = "/img/sp.jpg", ChatLieu = "Vỏ ốc", KichThuoc = "10cm", DanhMucId = 1 }
            };
            var stream = new MemoryStream();
            stream.SaveAs(dataMau);
            stream.Position = 0;
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Nhap_San_Pham.xlsx");
        }

        // Hàm phụ lưu ảnh
        private async Task<string> SaveImage(IFormFile file)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return "/img/" + fileName;
        }
        #endregion
    }
}