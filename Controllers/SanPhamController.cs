using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using OceanShellCraft.Models;
using OceanShellCraft.Helpers; // <-- GỌI HÀM ToSlug()
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
        [Route("san-pham")] // <-- URL MỚI: /san-pham
        [Route("SanPham/DanhSach")] // <-- Giữ lại bắt link cũ nếu cần
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
        [Route("san-pham/{slug}")] // <-- URL MỚI: /san-pham/ten-san-pham
        [Route("SanPham/ChiTiet/{id:int}")] // <-- Bắt link cũ phòng trường hợp user lưu bookmark
        public async Task<IActionResult> ChiTiet(string? slug, int? id)
        {
            SanPham? sanPham = null;

            // 1. Nếu có Slug (link mới), tìm kiếm trong Database theo Slug
            if (!string.IsNullOrEmpty(slug))
            {
                sanPham = await _context.SanPhams
                    .Include(s => s.DanhMuc)
                    .Include(s => s.DanhGias)
                        .ThenInclude(dg => dg.NguoiDung)
                    .FirstOrDefaultAsync(m => m.Slug == slug);
            }
            // 2. Nếu không có Slug nhưng có ID (người dùng vào bằng link cũ)
            else if (id.HasValue)
            {
                sanPham = await _context.SanPhams
                    .Include(s => s.DanhMuc)
                    .Include(s => s.DanhGias)
                        .ThenInclude(dg => dg.NguoiDung)
                    .FirstOrDefaultAsync(m => m.Id == id.Value);

                // Nếu tìm thấy bằng ID, điều hướng (Redirect) họ về link có Slug chuẩn SEO
                if (sanPham != null && !string.IsNullOrEmpty(sanPham.Slug))
                {
                    return RedirectToAction("ChiTiet", new { slug = sanPham.Slug });
                }
            }

            // Nếu không tìm thấy sản phẩm, trả về trang lỗi 404
            if (sanPham == null) return NotFound();

            bool canReview = false;
            bool isFavorite = false;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out int uId))
                {
                    // Check quyền đánh giá
                    canReview = await _context.DonHangs
                        .Include(dh => dh.ChiTietDonHangs)
                        .AnyAsync(dh => dh.NguoiDungId == uId
                                     && dh.TrangThai == "Đã hoàn thành"
                                     && dh.ChiTietDonHangs.Any(ct => ct.SanPhamId == sanPham.Id));

                    // Check trạng thái yêu thích
                    isFavorite = await _context.SanPhamYeuThiches
                        .AnyAsync(yt => yt.NguoiDungId == uId && yt.SanPhamId == sanPham.Id);
                }
            }

            ViewBag.CanReview = canReview;
            ViewBag.IsFavorite = isFavorite;

            return View(sanPham);
        }
        #endregion

        #region 3. Đánh giá Sản phẩm
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemDanhGia(int SanPhamId, int SoSao, string NoiDung)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int uId))
                return RedirectToAction("DangNhap", "TaiKhoan");

            // Lấy ra slug của sản phẩm để redirect đúng về đường link mới
            var sp = await _context.SanPhams.FindAsync(SanPhamId);
            string slugParam = (sp != null && !string.IsNullOrEmpty(sp.Slug)) ? sp.Slug : "chi-tiet";

            var donHangHopLe = await _context.DonHangs
                .Include(dh => dh.ChiTietDonHangs)
                .FirstOrDefaultAsync(dh => dh.NguoiDungId == uId
                                        && dh.TrangThai == "Đã hoàn thành"
                                        && dh.ChiTietDonHangs.Any(ct => ct.SanPhamId == SanPhamId));

            if (donHangHopLe == null)
                return RedirectToAction("ChiTiet", "SanPham", new { slug = slugParam }, "danh-gia");

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

            return RedirectToAction("ChiTiet", "SanPham", new { slug = slugParam }, "danh-gia");
        }
        #endregion

        #region 4. Cập nhật Slug tự động (Chạy để xóa ID khỏi link)
        [AllowAnonymous]
        [HttpGet]
        [Route("SanPham/CapNhatSlug")]
        public async Task<IActionResult> CapNhatSlug()
        {
            // Bỏ điều kiện kiểm tra rỗng để LẤY TẤT CẢ sản phẩm ra làm mới lại Slug
            var sanPhams = await _context.SanPhams.ToListAsync();

            int count = 0;
            foreach (var sp in sanPhams)
            {
                // Chỉ lấy Tên Sản Phẩm làm Slug (ĐÃ XÓA "-{sp.Id}")
                sp.Slug = sp.TenSanPham.ToSlug();
                count++;
            }

            await _context.SaveChangesAsync();
            return Content($"Tuyệt vời! Đã làm sạch Slug (xóa đuôi ID) cho {count} sản phẩm.");
        }
        #endregion
        #region 5. Yêu thích sản phẩm

        // Dùng cho nút thả tim ở trang Chi tiết sản phẩm (Gọi bằng AJAX)
        [HttpPost]
        [Route("SanPham/ToggleFavorite")]
        public async Task<IActionResult> ToggleFavorite(int sanPhamId)
        {
            // Kiểm tra đăng nhập
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int uId))
            {
                return Json(new { success = false, message = "Lỗi xác thực người dùng" });
            }

            // Tìm xem sản phẩm đã có trong danh sách yêu thích chưa
            var yeuThich = await _context.SanPhamYeuThiches
                .FirstOrDefaultAsync(yt => yt.NguoiDungId == uId && yt.SanPhamId == sanPhamId);

            bool isFavorite;

            if (yeuThich != null)
            {
                // Nếu đã yêu thích rồi thì xóa đi (Bỏ tim)
                _context.SanPhamYeuThiches.Remove(yeuThich);
                isFavorite = false;
            }
            else
            {
                // Nếu chưa có thì thêm vào (Thả tim)
                _context.SanPhamYeuThiches.Add(new SanPhamYeuThich
                {
                    NguoiDungId = uId,
                    SanPhamId = sanPhamId,
                    NgayThem = DateTime.Now
                });
                isFavorite = true;
            }

            await _context.SaveChangesAsync();

            // Trả về JSON để Javascript cập nhật lại icon trái tim
            return Json(new { success = true, isFavorite = isFavorite });
        }

        // Dùng cho nút [X] ở trang Danh sách Sản phẩm Yêu thích
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaYeuThich(int sanPhamId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int uId))
            {
                var yeuThich = await _context.SanPhamYeuThiches
                    .FirstOrDefaultAsync(yt => yt.NguoiDungId == uId && yt.SanPhamId == sanPhamId);

                if (yeuThich != null)
                {
                    _context.SanPhamYeuThiches.Remove(yeuThich);
                    await _context.SaveChangesAsync();
                }
            }

            // Tải lại trang hiện tại để cập nhật danh sách
            return RedirectToAction("DanhSachYeuThich"); // Sửa lại tên Action này nếu trang YeuThich của bạn có tên khác
        }

        #endregion
    }
}