using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OceanShellCraft.Models;

namespace OceanShellCraft.Controllers
{
    public class BaiVietController : Controller
    {
        private readonly MyNgheDbContext _context;

        // Tiêm DbContext để kết nối SQL
        public BaiVietController(MyNgheDbContext context)
        {
            _context = context;
        }

        // 1. TRANG DANH SÁCH (Hiển thị giao diện Slider)
        public async Task<IActionResult> DanhSach()
        {
            // Lấy toàn bộ bài viết, sắp xếp mới nhất lên đầu
            var danhSach = await _context.BaiViets
                .OrderByDescending(b => b.NgayTao)
                .ToListAsync();

            return View(danhSach);
        }

        // 2. TRANG CHI TIẾT (Hiển thị khi người dùng bấm "Xem chi tiết")

        [HttpGet]
        public IActionResult GetChiTiet(int id)
        {
            var post = _context.BaiViets.FirstOrDefault(b => b.Id == id);
            if (post == null) return NotFound();

            // Sửa lại tên PartialView cho đúng với file bạn đang để nội dung chi tiết
            // Nếu file đó tên là _NoiDungBaiViet.cshtml thì sửa thành:
            return PartialView("_ChiTietNoiDung", post);
        }
    }
}