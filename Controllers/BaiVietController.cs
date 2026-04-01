using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OceanShellCraft.Models;
using System.Security.Claims;

namespace OceanShellCraft.Controllers
{
    public class BaiVietController : Controller
    {
        private readonly MyNgheDbContext _context;

        public BaiVietController(MyNgheDbContext context)
        {
            _context = context;
        }

        [Route("bai-viet")]
        public async Task<IActionResult> DanhSach()
        {
            var danhSach = await _context.BaiViets
                .OrderByDescending(b => b.NgayTao)
                .ToListAsync();
            return View(danhSach);
        }

        [HttpGet]
        [Route("bai-viet/chi-tiet/{id}")]
        public IActionResult GetChiTiet(int id)
        {
            // Include NguoiDung để hiển thị tên người bình luận
            var post = _context.BaiViets
                .Include(b => b.DanhGiaBaiViets.Where(d => d.DaDuyet))
                    .ThenInclude(d => d.NguoiDung)
                .FirstOrDefault(b => b.Id == id);

            if (post == null) return NotFound();

            return PartialView("_ChiTietNoiDung", post);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken] // Bảo mật CSRF
        [Route("bai-viet/them-binh-luan")]
        public async Task<IActionResult> ThemBinhLuan([FromForm] int BaiVietId, [FromForm] int SoSao, [FromForm] string NoiDung)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập lại." });
            }

            if (string.IsNullOrEmpty(NoiDung) || SoSao <= 0)
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung và chọn số sao." });
            }

            var cmt = new DanhGiaBaiViet
            {
                BaiVietId = BaiVietId,
                NguoiDungId = userId,
                SoSao = SoSao,
                NoiDung = NoiDung,
                NgayDanhGia = DateTime.Now,
                DaDuyet = true
            };

            try
            {
                _context.DanhGiaBaiViets.Add(cmt);

                // Cập nhật thống kê số lượng đánh giá cho bài viết (nếu cần)
                var post = await _context.BaiViets.FindAsync(BaiVietId);
                if (post != null)
                {
                    post.TongLuotDanhGia += 1;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}