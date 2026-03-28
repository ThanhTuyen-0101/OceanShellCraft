using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OceanShellCraft.Models;
using OceanShellCraft.Models.ViewModels;

public class GioHangController : Controller
{
    private readonly MyNgheDbContext _context;

    public GioHangController(MyNgheDbContext context) => _context = context;

    // Lấy hoặc tạo Mã phiên từ Cookie để định danh giỏ hàng
    private string LayMaPhien()
    {
        string maPhien = Request.Cookies["MaPhienGioHang"];
        if (string.IsNullOrEmpty(maPhien))
        {
            maPhien = Guid.NewGuid().ToString();
            Response.Cookies.Append("MaPhienGioHang", maPhien, new CookieOptions { Expires = DateTime.Now.AddDays(30) });
        }
        return maPhien;
    }

    public async Task<IActionResult> DanhSach()
    {
        string maPhien = LayMaPhien();
        var gioHang = await _context.GioHangs
            .Include(g => g.SanPham)
            .Where(g => g.MaPhien == maPhien)
            .ToListAsync();
        return View(gioHang);
    }

    public async Task<IActionResult> ThemVaoGio(int sanPhamId)
    {
        string maPhien = LayMaPhien();
        var mucGioHang = await _context.GioHangs
            .FirstOrDefaultAsync(g => g.MaPhien == maPhien && g.SanPhamId == sanPhamId);

        if (mucGioHang == null)
        {
            _context.GioHangs.Add(new GioHang { MaPhien = maPhien, SanPhamId = sanPhamId, SoLuong = 1 });
        }
        else
        {
            mucGioHang.SoLuong++;
        }
        await _context.SaveChangesAsync();
        return RedirectToAction("DanhSach");
    }

    public async Task<IActionResult> XoaKhoiGio(int id)
    {
        var muc = await _context.GioHangs.FindAsync(id);
        if (muc != null) _context.GioHangs.Remove(muc);
        await _context.SaveChangesAsync();
        return RedirectToAction("DanhSach");
    }

    // Thêm hàm này vào GioHangController hiện tại của Như
    public async Task<IActionResult> CapNhatSoLuong(int id, int soLuong)
    {
        var muc = await _context.GioHangs.FindAsync(id);
        if (muc != null && soLuong > 0)
        {
            muc.SoLuong = soLuong;
            await _context.SaveChangesAsync();
        }
        // Vì mình dùng AJAX gọi hàm này, nên trả về OK là được
        return Ok();
    }

    [Authorize]
    public IActionResult DatHang()
    {
        string maPhien = Request.Cookies["MaPhienGioHang"] ?? "";
        var items = _context.GioHangs.Include(g => g.SanPham).Where(g => g.MaPhien == maPhien).ToList();

        if (items.Count == 0) return RedirectToAction("DanhSach");

        var vm = new GioHangViewModel { DanhSachItem = items };
        return View(vm);
    }

    // 2. Khi khách nhấn nút "XÁC NHẬN VÀ ĐẶT HÀNG" (Lưu vào CSDL)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> XacNhanDatHang(string SoDienThoai, string DiaChi, List<int> ListSanPhamId, List<int> ListSoLuong)
    {
        // SỬA LỖI Ở ĐÂY: Kiểm tra an toàn Claim trước khi Parse
        var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            // Nếu không lấy được ID (phiên đăng nhập lỗi hoặc bị mất Claim), yêu cầu đăng nhập lại
            // Đảm bảo tên "NguoiDung" là Controller xử lý đăng nhập của bạn (thay đổi nếu cần)
            return RedirectToAction("Login", "NguoiDung");
        }

        var userId = int.Parse(userIdClaim.Value);

        // 1. Tạo đơn hàng mới
        var donHang = new DonHang
        {
            NguoiDungId = userId,
            NgayDat = DateTime.Now,
            SoDienThoai = SoDienThoai,
            DiaChi = DiaChi,
            TrangThai = "Chờ xử lý",
            TongTien = 0 // Sẽ tính lại dựa trên số lượng khách vừa nhập
        };

        _context.DonHangs.Add(donHang);
        await _context.SaveChangesAsync();

        decimal tongTienMoi = 0;

        // 2. Lưu chi tiết đơn hàng dựa trên số lượng mới khách vừa chọn
        for (int i = 0; i < ListSanPhamId.Count; i++)
        {
            var spId = ListSanPhamId[i];
            var soLuongMoi = ListSoLuong[i];
            var sanPham = await _context.SanPhams.FindAsync(spId);

            if (sanPham != null)
            {
                var ct = new ChiTietDonHang
                {
                    DonHangId = donHang.Id,
                    SanPhamId = spId,
                    SoLuong = soLuongMoi,
                    GiaLucMua = sanPham.GiaTien
                };
                _context.ChiTietDonHangs.Add(ct);
                tongTienMoi += soLuongMoi * sanPham.GiaTien;
            }
        }

        // 3. Cập nhật lại tổng tiền cuối cùng của đơn hàng
        donHang.TongTien = tongTienMoi;

        // 4. Xóa giỏ hàng cũ
        string maPhien = Request.Cookies["MaPhienGioHang"] ?? "";
        var cartItems = _context.GioHangs.Where(g => g.MaPhien == maPhien);
        _context.GioHangs.RemoveRange(cartItems);

        await _context.SaveChangesAsync();

        return View("CamOn");
    }
}