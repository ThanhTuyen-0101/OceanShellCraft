using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OceanShellCraft.Models;
using OceanShellCraft.Models.ViewModels;

namespace OceanShellCraft.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được vào "vùng cấm" này
    public class QuanTriVienController : Controller
    {
        private readonly MyNgheDbContext _context;

        public QuanTriVienController(MyNgheDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH THÀNH VIÊN
        public async Task<IActionResult> DanhSach()
        {
            var users = await _context.NguoiDungs.ToListAsync();
            return View("DanhSach", users);
        }

        // 2. CẬP NHẬT QUYỀN (GET)
        [HttpGet]
        public async Task<IActionResult> SuaQuyen(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

            // Đóng gói dữ liệu vào ViewModel để mang sang View
            var viewModel = new SuaQuyenNguoiDungViewModel
            {
                NguoiDungId = user.Id,
                Email = user.Email,
                VaiTroHienTai = user.VaiTro
            };

            return View(viewModel);
        }

        // 3. CẬP NHẬT QUYỀN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaQuyen(SuaQuyenNguoiDungViewModel vm, string vaiTroMoi)
        {
            // Tìm user gốc trong Database dựa trên ID từ ViewModel
            var user = await _context.NguoiDungs.FindAsync(vm.NguoiDungId);

            if (user != null)
            {
                // Chỉ cập nhật duy nhất trường VaiTro
                user.VaiTro = vaiTroMoi;

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = $"Đã cập nhật quyền của {user.Email} thành {vaiTroMoi}";
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        // 4. XÓA TÀI KHOẢN
        [HttpPost]
        public async Task<IActionResult> XoaNguoiDung(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);

            // Chặn xóa tài khoản admin gốc để tránh khóa hệ thống
            if (user != null && user.Email != "dangngoctamnhu2000@gmail.com")
            {
                _context.NguoiDungs.Remove(user);
                await _context.SaveChangesAsync();
                TempData["ThongBao"] = "Đã xóa người dùng thành công";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}