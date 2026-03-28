using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using OceanShellCraft.Models;
using System.Data;
using System.Security.Claims;

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
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> DanhSach(int? maDanhMuc, int page = 1)
        {
            int pageSize = 9;
            var query = _context.SanPhams.Include(s => s.DanhMuc).AsQueryable();

            if (maDanhMuc.HasValue)
            {
                query = query.Where(s => s.DanhMucId == maDanhMuc);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.CurrentCategory = maDanhMuc;
            ViewBag.DanhMucs = await _context.DanhMucs.ToListAsync();

            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return View(data);
        }
        #endregion

        #region 2. Chi tiết Sản phẩm (Công khai)
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var sanPham = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .Include(s => s.DanhGias)
                    .ThenInclude(dg => dg.NguoiDung)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sanPham == null) return NotFound();

            bool canReview = false;
            bool isFavorite = false; // Biến kiểm tra sản phẩm đã được yêu thích chưa

            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out int uId))
                {
                    // Check quyền đánh giá
                    canReview = await _context.DonHangs
                        .Include(dh => dh.ChiTietDonHangs)
                        .AnyAsync(dh => dh.NguoiDungId == uId
                                     && dh.TrangThai == "Đã hoàn thành"
                                     && dh.ChiTietDonHangs.Any(ct => ct.SanPhamId == id));

                    // Check trạng thái yêu thích
                    isFavorite = await _context.SanPhamYeuThiches
                        .AnyAsync(yt => yt.NguoiDungId == uId && yt.SanPhamId == id);
                }
            }

            ViewBag.CanReview = canReview;
            ViewBag.IsFavorite = isFavorite; // Truyền ra View

            return View(sanPham);
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

        #region 5. Đánh giá Sản phẩm
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemDanhGia(int SanPhamId, int SoSao, string NoiDung)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int uId))
                return RedirectToAction("DangNhap", "TaiKhoan");

            var donHangHopLe = await _context.DonHangs
                .Include(dh => dh.ChiTietDonHangs)
                .FirstOrDefaultAsync(dh => dh.NguoiDungId == uId
                                        && dh.TrangThai == "Đã hoàn thành"
                                        && dh.ChiTietDonHangs.Any(ct => ct.SanPhamId == SanPhamId));

            if (donHangHopLe == null)
                return RedirectToAction("ChiTiet", "SanPham", new { id = SanPhamId }, "danh-gia");

            var danhGiaMoi = new OceanShellCraft.Models.DanhGia
            {
                SanPhamId = SanPhamId,
                NguoiDungId = uId,
                DonHangId = donHangHopLe.Id,
                SoSao = SoSao,
                NoiDung = NoiDung,
                NgayDanhGia = DateTime.Now
            };

            _context.DanhGias.Add(danhGiaMoi);
            await _context.SaveChangesAsync();

            return RedirectToAction("ChiTiet", "SanPham", new { id = SanPhamId }, "danh-gia");
        }
        #endregion

        #region 6. Xử lý Sản Phẩm Yêu Thích (AJAX)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int sanPhamId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int uId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            // Kiểm tra xem đã có trong danh sách yêu thích chưa
            var existingFav = await _context.SanPhamYeuThiches
                .FirstOrDefaultAsync(yt => yt.NguoiDungId == uId && yt.SanPhamId == sanPhamId);

            if (existingFav != null)
            {
                // Nếu đã có -> Xóa khỏi danh sách (Bỏ tim)
                _context.SanPhamYeuThiches.Remove(existingFav);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isFavorite = false });
            }
            else
            {
                // Nếu chưa có -> Thêm vào danh sách (Thả tim)
                var newFav = new SanPhamYeuThich
                {
                    NguoiDungId = uId,
                    SanPhamId = sanPhamId
                };
                _context.SanPhamYeuThiches.Add(newFav);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isFavorite = true });
            }
        }
        #endregion
    }
}