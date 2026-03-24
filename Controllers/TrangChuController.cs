using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // BẮT BUỘC CÓ DÒNG NÀY ĐỂ DÙNG .Include()
using OceanShellCraft.Models;
using System.Linq;

namespace OceanShellCraft.Controllers
{
    public class TrangChuController : Controller
    {
        private readonly MyNgheDbContext _context;

        public TrangChuController(MyNgheDbContext context)
        {
            _context = context;
        }

        public IActionResult TrangChu()
        {
            // 1. Lấy danh sách sản phẩm (VD: 40)
            var sanPhams = _context.SanPhams.OrderByDescending(x => x.Id).Take(40).ToList();

            // 2. Truy xuất danh mục và gán vào ViewBag
            ViewBag.DanhMucs = _context.DanhMucs.ToList();

            // --- THÊM MỚI: 3. LẤY ĐÁNH GIÁ 5 SAO MỚI NHẤT ---
            var topReview = _context.DanhGias
                .Include(d => d.NguoiDung) // Liên kết bảng NguoiDung để lấy Tên
                .Where(d => d.SoSao == 5)  // Lọc chỉ lấy 5 sao
                .OrderByDescending(d => d.NgayDanhGia) // Mới nhất lên đầu
                .FirstOrDefault();

            if (topReview != null)
            {
                // Truyền dữ liệu sang View
                ViewBag.TopReview = new
                {
                    Id = topReview.Id,
                    TenKhachHang = topReview.NguoiDung?.HoTen ?? "Khách hàng",
                    NoiDung = topReview.NoiDung,
                    SanPhamId = topReview.SanPhamId
                };
            }

            // --- THÊM MỚI: 4. LẤY BÀI VIẾT (NEWS) MỚI NHẤT ---
            var latestBaiViet = _context.BaiViets
                .OrderByDescending(b => b.Id) // Bạn có thể thay Id bằng NgayDang/NgayTao nếu có
                .FirstOrDefault();

            ViewBag.LatestBaiViet = latestBaiViet;

            // 5. Trả dữ liệu về View
            return View(sanPhams);
        }

        public IActionResult GioiThieu()
        {
            ViewBag.TieuDeTrang = "Giới thiệu về OceanShellCraft";
            return View();
        }
    }
}