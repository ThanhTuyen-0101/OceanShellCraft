using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using OceanShellCraft.Models;

namespace OceanShellCraft.Controllers
{
    [Authorize]
    public class KhachHangController : Controller
    {
        private readonly MyNgheDbContext _context;

        public KhachHangController(MyNgheDbContext context)
        {
            _context = context;
        }

        // Helper để lấy ID người dùng hiện tại nhanh hơn
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId)) return userId;
            return null;
        }

        public async Task<IActionResult> TongQuan()
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
            if (nguoiDung == null) return NotFound();

            // 1. Lấy danh sách đơn hàng của User này
            var danhSachDonHang = await _context.DonHangs
                .Where(d => d.NguoiDungId == userId)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            // 2. Thống kê
            ViewBag.DonDaMua = danhSachDonHang.Count(d => d.TrangThai == "Đã hoàn thành");
            ViewBag.DonDangGiao = danhSachDonHang.Count(d => d.TrangThai == "Đang giao hàng" || d.TrangThai == "Chờ xử lý" || d.TrangThai == "Đang đóng gói");

            // Tự động tính điểm tích lũy (Cứ 100k = 10 điểm)
            var tongTien = danhSachDonHang.Where(d => d.TrangThai == "Đã hoàn thành").Sum(d => d.TongTien);
            ViewBag.DiemTichLuy = (int)(tongTien / 100000) * 10;

            // 3. Lấy 3 đơn hàng gần nhất
            ViewBag.DonHangGanDay = danhSachDonHang.Take(3).ToList();

            // 4. Lấy danh sách Voucher còn hạn
            var now = DateTime.Now;
            ViewBag.GiamGia = await _context.GiamGias
                .Where(v => v.NguoiDungId == userId && v.HanSuDung >= now)
                .ToListAsync();

            return View(nguoiDung);
        }

        // Gộp logic Index (Lọc) vào Orders để khớp với View và Layout
        public async Task<IActionResult> Orders(string trangThai)
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("DangNhap", "TaiKhoan");

            // Bắt đầu truy vấn đơn hàng của User này
            var query = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.SanPham)
                .Where(d => d.NguoiDungId == userId);

            // Thực hiện lọc theo trạng thái nếu có
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(d => d.TrangThai == trangThai);
            }

            var dsDonHang = await query.OrderByDescending(d => d.NgayDat).ToListAsync();

            return View(dsDonHang);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatHoSo(NguoiDung model)
        {
            var userInDb = await _context.NguoiDungs.FindAsync(model.Id);
            if (userInDb == null) return NotFound();

            // Cập nhật các trường cho phép
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

        public IActionResult HoSo()
        {
            return RedirectToAction("TongQuan");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDonHang(int id)
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("DangNhap", "TaiKhoan");

            // Tìm đơn hàng: Phải đúng ID và đúng chủ sở hữu, chỉ cho phép xóa khi đang "Chờ xử lý"
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
        public async Task<IActionResult> ChiTietDonHang(int id)
        {
            // Lấy ID người dùng đang đăng nhập (Giả sử bạn đang dùng ClaimTypes.NameIdentifier)
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int uId))
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // Truy vấn đơn hàng kèm theo chi tiết và thông tin sản phẩm
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.SanPham) // Join bảng SanPham để lấy Tên, Hình ảnh...
                .FirstOrDefaultAsync(d => d.Id == id && d.NguoiDungId == uId);

            if (donHang == null)
            {
                TempData["Loi"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem.";
                return RedirectToAction("Orders");
            }

            return View(donHang);
        }
        [HttpGet]
        public async Task<IActionResult> SanPhamYeuThich()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int uId))
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // Lấy danh sách sản phẩm yêu thích của User hiện tại
            var danhSachYeuThich = await _context.SanPhamYeuThiches
                .Include(yt => yt.SanPham)
                .Where(yt => yt.NguoiDungId == uId)
                .OrderByDescending(yt => yt.NgayThem) // Mới thích hiện lên đầu
                .ToListAsync();

            return View(danhSachYeuThich);
        }
        [HttpPost]
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
    }
}