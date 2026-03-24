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
                if (_context.NguoiDungs.Any(u => u.Email == vm.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng");
                    return View(vm);
                }

                var user = new NguoiDung
                {
                    HoTen = vm.HoTen,
                    Email = vm.Email,
                    MatKhau = vm.MatKhau,
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
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.HoTen),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.VaiTro),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("TrangChu", "TrangChu");
            }
            ViewBag.Loi = "Sai email hoặc mật khẩu!";
            return View();
        }

        public async Task<IActionResult> DangXuat()
        {
            // Xóa Cookie xác thực (Quan trọng nhất)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Xóa dòng này nếu bạn chưa cấu hình Session trong Program.cs
            // HttpContext.Session.Clear(); 

            return RedirectToAction("TrangChu", "TrangChu");
        }

        [HttpGet]
        public IActionResult TuChoiTruyCap() => View();
    }
}