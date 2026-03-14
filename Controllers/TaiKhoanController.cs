using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OceanShellCraft.Models;
using System.Security.Claims;
using OceanShellCraft.Models.ViewModels;

namespace OceanShellCraft.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly MyNgheDbContext _context;
        public TaiKhoanController(MyNgheDbContext context) => _context = context;

        [HttpGet]
        public IActionResult DangKy() => View();

        [HttpPost]
        public async Task<IActionResult> DangKy(DangKyViewModel vm)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem email đã tồn tại chưa
                if (_context.NguoiDungs.Any(u => u.Email == vm.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng");
                    return View(vm);
                }

                // Chuyển dữ liệu từ View Model sang Model chính
                var user = new NguoiDung
                {
                    HoTen = vm.HoTen,
                    Email = vm.Email,
                    MatKhau = vm.MatKhau, // Trong thực tế nên mã hóa Pass ở đây
                    VaiTro = vm.Email.ToLower() == "dangngoctamnhu2000@gmail.com" ? "Admin" : "KhachHang"
                };

                _context.NguoiDungs.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("DangNhap");
            }
            return View(vm);
        }

        [HttpGet]
        public IActionResult DangNhap() => View();

        [HttpPost]
        public async Task<IActionResult> DangNhap(string email, string matkhau)
        {
            var user = _context.NguoiDungs.FirstOrDefault(u => u.Email == email && u.MatKhau == matkhau);
            if (user != null)
            {
                // Tạo "Thẻ căn cước" cho người dùng
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.HoTen),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.VaiTro), // Quan trọng nhất để phân quyền
                    new Claim("UserId", user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Đăng nhập chính thức vào hệ thống
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "TrangChu");
            }
            ViewBag.Loi = "Sai email hoặc mật khẩu!";
            return View();
        }

        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "TrangChu");
        }
    }
}