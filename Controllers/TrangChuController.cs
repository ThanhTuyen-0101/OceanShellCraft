using Microsoft.AspNetCore.Mvc;
using OceanShellCraft.Models;
using System.Linq;

namespace OceanShellCraft.Controllers
{
    public class TrangChuController : Controller
    {
        // Khai báo kết nối DbContext
        private readonly MyNgheDbContext _context;

        public TrangChuController(MyNgheDbContext context)
        {
            _context = context;
        }

        public IActionResult TrangChu()
        {
            var topSanPhams = _context.SanPhams.Take(3).ToList();

            // Trả về mặc định, nó sẽ tự động tìm file Views/TrangChu/TrangChu.cshtml
            return View(topSanPhams);
        }
        public IActionResult GioiThieu()
        {
            ViewBag.TieuDeTrang = "Giới thiệu về OceanShellCraft";
            return View();
        }
    }
}