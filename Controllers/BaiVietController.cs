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
        public async Task<IActionResult> ChiTiet(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Báo lỗi 404 nếu không có ID
            }

            // Tìm bài viết trong SQL dựa theo ID
            var baiViet = await _context.BaiViets.FirstOrDefaultAsync(m => m.Id == id);

            if (baiViet == null)
            {
                return NotFound(); // Báo lỗi 404 nếu ID không tồn tại trong Data
            }

            return View(baiViet); // Truyền dữ liệu bài viết sang View
        }
    }
}