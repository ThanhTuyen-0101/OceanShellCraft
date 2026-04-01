using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OceanShellCraft.Models;
using System.Linq;

namespace OceanShellCraft.Controllers
{
    [Route("trang-chu")]
    public class TrangChuController : Controller
    {
        private readonly MyNgheDbContext _context;

        public TrangChuController(MyNgheDbContext context)
        {
            _context = context;
        }

        [Route("")]
        [Route("~/")]
        public IActionResult TrangChu()
        {
            var sanPhams = _context.SanPhams.OrderByDescending(x => x.Id).Take(40).ToList();
            ViewBag.DanhMucs = _context.DanhMucs.ToList();

            var topReview = _context.DanhGias
                .Include(d => d.NguoiDung)
                .Include(d => d.SanPham)
                .Where(d => d.SoSao == 5)
                .OrderByDescending(d => d.NgayDanhGia)
                .FirstOrDefault();

            if (topReview != null)
            {
                ViewBag.TopReview = new
                {
                    Id = topReview.Id,
                    TenKhachHang = topReview.NguoiDung?.HoTen ?? "Khách hàng thân thiết",
                    NoiDung = topReview.NoiDung,
                    SanPhamId = topReview.SanPhamId,
                    Slug = topReview.SanPham?.Slug,
                    SoSao = topReview.SoSao,
                    Avatar = topReview.NguoiDung?.AnhDaiDien ?? "/images/avatar-default.png"
                };
            }
            

          
            var latestBaiViet = _context.BaiViets
                .OrderByDescending(b => b.Id)
                .FirstOrDefault();

            ViewBag.LatestBaiViet = latestBaiViet;
            return View(sanPhams);
        }

        [Route("gioi-thieu")]
        public IActionResult GioiThieu()
        {
            ViewBag.TieuDeTrang = "Giới thiệu về OceanShellCraft";
            return View();
        }

        [Route("bai-viet")]
        public IActionResult BaiViet()
        {
            return View();
        }
    }
}