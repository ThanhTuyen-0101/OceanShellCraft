using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using OceanShellCraft.Models;
using System.Threading.Tasks;

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

        // ĐÃ SỬA: Trang Tổng Quan giờ sẽ đóng vai trò là trang Hồ Sơ chính
        public async Task<IActionResult> TongQuan()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(int.Parse(userId));
            if (nguoiDung == null)
            {
                return NotFound();
            }

            // Truyền dữ liệu người dùng sang giao diện Tổng Quan
            return View(nguoiDung);
        }

        public IActionResult Orders() => View();

        public IActionResult Messages() => View();

        // Trang HoSo giờ không cần thiết nữa, nếu lỡ có link nào gọi tới thì chuyển hướng về TongQuan
        public IActionResult HoSo()
        {
            return RedirectToAction("TongQuan");
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatHoSo(NguoiDung model)
        {
            var userInDb = await _context.NguoiDungs.FindAsync(model.Id);
            if (userInDb == null)
            {
                return NotFound();
            }

            userInDb.HoTen = model.HoTen;
            userInDb.NgaySinh = model.NgaySinh;
            userInDb.GioiTinh = model.GioiTinh;
            userInDb.SoDienThoai = model.SoDienThoai;

            _context.NguoiDungs.Update(userInDb);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Đã cập nhật hồ sơ thành công!";

            // ĐÃ SỬA: Cập nhật xong thì quay về trang Tổng Quan thay vì trang HoSo cũ
            return RedirectToAction("TongQuan");
        }
    }
}