using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using OceanShellCraft.Models;
using OceanShellCraft.Models.ViewModels;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace OceanShellCraft.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly MyNgheDbContext _context;
        public TaiKhoanController(MyNgheDbContext context) => _context = context;

        [HttpGet]
        [Route("dang-ky")]
        public IActionResult DangKy() => View();

        [HttpPost]
        [Route("dang-ky")]
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
        [Route("dang-nhap")]
        public IActionResult DangNhap() => View();

        [HttpPost]
        [Route("dang-nhap")]
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

        [Route("login-google")]
        public IActionResult LoginGoogle()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [Route("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded) return RedirectToAction("DangNhap");

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var picture = claims?.FirstOrDefault(x => x.Type == "picture")?.Value;

            if (string.IsNullOrEmpty(email)) return RedirectToAction("DangNhap");

            var user = _context.NguoiDungs.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                user = new NguoiDung
                {
                    Email = email,
                    HoTen = name ?? "Người dùng Google",
                    MatKhau = Guid.NewGuid().ToString(),
                    AnhDaiDien = picture,
                    NgayTao = DateTime.Now,
                    VaiTro = "KhachHang"
                };
                _context.NguoiDungs.Add(user);
                await _context.SaveChangesAsync();
            }

            var localClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.HoTen),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.VaiTro),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(localClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("TrangChu", "TrangChu");
        }

        [Route("dang-xuat")]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("TrangChu", "TrangChu");
        }

        [HttpGet]
        [Route("tu-choi-truy-cap")]
        public IActionResult TuChoiTruyCap() => View();

        #region QUÊN MẬT KHẨU
        [HttpGet]
        [Route("quen-mat-khau")]
        public IActionResult QuenMatKhau() => View();

        [HttpPost]
        [Route("quen-mat-khau")]
        public async Task<IActionResult> QuenMatKhau(QuenMatKhauVM vm)
        {
            var user = _context.NguoiDungs.FirstOrDefault(u => u.Email == vm.Email);
            if (user == null)
            {
                ViewBag.Loi = "Email này không tồn tại trong hệ thống!";
                return View();
            }

            string otp = new Random().Next(100000, 999999).ToString();

            TempData["ResetOTP"] = otp;
            TempData["ResetEmail"] = vm.Email;

            try
            {
                var emailNguoiGui = "tuyenthanhthanh3979@gmail.com";
                var matKhauUngDung = "leqflrdlpvuffrih";

                var mail = new MailMessage();
                mail.From = new MailAddress(emailNguoiGui, "OceanShellCraft Support");
                mail.To.Add(vm.Email);
                mail.Subject = "Mã xác nhận khôi phục mật khẩu";
                mail.IsBodyHtml = true;
                mail.Body = $"<h2 style='color:#007bff;'>Mã OTP của bạn là: {otp}</h2><p>Mã này dùng để đặt lại mật khẩu. Vui lòng không cung cấp cho bất kỳ ai.</p>";

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(emailNguoiGui, matKhauUngDung),
                    EnableSsl = true
                };
                await smtp.SendMailAsync(mail);

                return RedirectToAction("XacNhanOTP");
            }
            catch
            {
                ViewBag.Loi = "Không thể gửi mail lúc này. Vui lòng thử lại!";
                return View();
            }
        }

        [HttpGet]
        [Route("xac-nhan-otp")]
        public IActionResult XacNhanOTP() => View();

        [HttpPost]
        [Route("xac-nhan-otp")]
        public IActionResult XacNhanOTP(QuenMatKhauVM vm)
        {
            string storedOtp = TempData["ResetOTP"]?.ToString();
            string email = TempData["ResetEmail"]?.ToString();

            if (vm.OTP == storedOtp && !string.IsNullOrEmpty(storedOtp))
            {
                TempData["CanReset"] = email;
                return RedirectToAction("DatLaiMatKhau");
            }

            ViewBag.Loi = "Mã OTP không chính xác hoặc đã hết hạn!";
            TempData.Keep("ResetOTP");
            TempData.Keep("ResetEmail");
            return View();
        }

        [HttpGet]
        [Route("dat-lai-mat-khau")]
        public IActionResult DatLaiMatKhau()
        {
            if (TempData["CanReset"] == null) return RedirectToAction("QuenMatKhau");
            return View();
        }

        [HttpPost]
        [Route("dat-lai-mat-khau")]
        public async Task<IActionResult> DatLaiMatKhau(QuenMatKhauVM vm)
        {
            string email = TempData["CanReset"]?.ToString();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("QuenMatKhau");

            if (vm.MatKhauMoi != vm.NhapLaiMatKhau)
            {
                ViewBag.Loi = "Mật khẩu nhập lại không khớp!";
                TempData.Keep("CanReset");
                return View();
            }

            var user = _context.NguoiDungs.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.MatKhau = vm.MatKhauMoi;
                await _context.SaveChangesAsync();
                TempData.Clear();
                return RedirectToAction("DangNhap");
            }

            return RedirectToAction("QuenMatKhau");
        }
        #endregion
    }
}