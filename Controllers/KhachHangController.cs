using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using OceanShellCraft.Models;

namespace OceanShellCraft.Controllers
{
    [Authorize]
    [Route("khach-hang")]
    public class KhachHangController : Controller
    {
        private readonly MyNgheDbContext _context;

        public KhachHangController(MyNgheDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId)) return userId;
            return null;
        }

        

        [Route("don-hang")]
        public async Task<IActionResult> Orders(string trangThai)
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var query = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.SanPham)
                .Where(d => d.NguoiDungId == userId);

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(d => d.TrangThai == trangThai);
            }

            var dsDonHang = await query.OrderByDescending(d => d.NgayDat).ToListAsync();

            return View(dsDonHang);
        }

        [Route("ho-tro")]
        public async Task<IActionResult> Chat()
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var chatHistory = await _context.TinNhans
                .Where(t => t.NguoiDungId == userId)
                .OrderBy(t => t.ThoiGian)
                .ToListAsync();

            return View(chatHistory);
        }

        [HttpPost("cap-nhat-ho-so")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatHoSo(NguoiDung model)
        {
            var userInDb = await _context.NguoiDungs.FindAsync(model.Id);
            if (userInDb == null) return NotFound();

            userInDb.HoTen = model.HoTen;
            userInDb.NgaySinh = model.NgaySinh;
            userInDb.GioiTinh = model.GioiTinh;
            userInDb.SoDienThoai = model.SoDienThoai;
            userInDb.DiaChi = model.DiaChi;

            try
            {
                _context.NguoiDungs.Update(userInDb);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Đã cập nhật hồ sơ thành công!";
            }
            catch
            {
                TempData["Loi"] = "Có lỗi xảy ra khi cập nhật.";
            }

            return RedirectToAction("TongQuan");
        }

        [Route("ho-so")]
        public IActionResult HoSo()
        {
            return RedirectToAction("TongQuan");
        }

        [HttpPost("huy-don-hang/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDonHang(int id)
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var donHang = await _context.DonHangs
                .FirstOrDefaultAsync(d => d.Id == id && d.NguoiDungId == userId);

            if (donHang != null)
            {
                if (donHang.TrangThai == "Chờ xử lý")
                {
                    _context.DonHangs.Remove(donHang);
                    await _context.SaveChangesAsync();
                    TempData["ThongBao"] = "Đã hủy và xóa đơn hàng thành công.";
                }
                else
                {
                    TempData["Loi"] = "Chỉ có thể hủy đơn hàng ở trạng thái Chờ xử lý.";
                }
            }

            return RedirectToAction("Orders");
        }

        [Route("chi-tiet-don-hang/{id}")]
        public async Task<IActionResult> ChiTietDonHang(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int uId))
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.SanPham)
                .FirstOrDefaultAsync(d => d.Id == id && d.NguoiDungId == uId);

            if (donHang == null)
            {
                TempData["Loi"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem.";
                return RedirectToAction("Orders");
            }

            return View(donHang);
        }

        [HttpGet("san-pham-yeu-thich")]
        public async Task<IActionResult> SanPhamYeuThich()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int uId))
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var danhSachYeuThich = await _context.SanPhamYeuThiches
                .Include(yt => yt.SanPham)
                .Where(yt => yt.NguoiDungId == uId)
                .OrderByDescending(yt => yt.NgayThem)
                .ToListAsync();

            return View(danhSachYeuThich);
        }

        [HttpPost("xoa-yeu-thich")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaYeuThich(int sanPhamId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int uId))
            {
                var item = await _context.SanPhamYeuThiches
                    .FirstOrDefaultAsync(yt => yt.NguoiDungId == uId && yt.SanPhamId == sanPhamId);

                if (item != null)
                {
                    _context.SanPhamYeuThiches.Remove(item);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(SanPhamYeuThich));
        }
        [Route("tong-quan")]
        public async Task<IActionResult> TongQuan()
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
            if (nguoiDung == null) return NotFound();

            var danhSachDonHang = await _context.DonHangs
                .Where(d => d.NguoiDungId == userId)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            ViewBag.DonDaMua = danhSachDonHang.Count(d => d.TrangThai == "Đã hoàn thành");
            ViewBag.DonDangGiao = danhSachDonHang.Count(d => d.TrangThai == "Đang giao hàng" || d.TrangThai == "Chờ xử lý" || d.TrangThai == "Đang đóng gói");

            var tongTien = danhSachDonHang.Where(d => d.TrangThai == "Đã hoàn thành").Sum(d => d.TongTien);
            ViewBag.DiemTichLuy = (int)(tongTien / 100000) * 10;

            ViewBag.DonHangGanDay = danhSachDonHang.Take(3).ToList();

            // --- CẬP NHẬT PHẦN NÀY ---
            var now = DateTime.Now;
            ViewBag.GiamGia = await _context.GiamGias
                .Where(v => v.IsKichHoat
                         && v.HanSuDung >= now
                         && v.NgayBatDau <= now
                         && (v.NguoiDungId == userId || v.NguoiDungId == null)) // Lấy cả mã riêng và mã chung
                .OrderByDescending(v => v.NgayBatDau)
                .ToListAsync();

            return View(nguoiDung);
        }
    }
}