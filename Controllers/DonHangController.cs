using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using OceanShellCraft.Models;
using Microsoft.EntityFrameworkCore;

public class DonHangController : Controller
{
    private readonly MyNgheDbContext _context;
    public DonHangController(MyNgheDbContext context) => _context = context;

    [HttpGet]
    public IActionResult ThanhToan() => View();

    [HttpPost]
    public async Task<IActionResult> ThanhToan(DonHang donHang)
    {
        string maPhien = Request.Cookies["MaPhienGioHang"];
        var gioHang = await _context.GioHangs
            .Include(g => g.SanPham)
            .Where(g => g.MaPhien == maPhien).ToListAsync();

        if (gioHang.Count == 0) return RedirectToAction("Index", "TrangChu");

        donHang.NgayDat = DateTime.Now;
        donHang.TongTien = gioHang.Sum(g => g.SoLuong * (g.SanPham?.GiaTien ?? 0));
        donHang.TrangThai = "Chờ xử lý";

        _context.DonHangs.Add(donHang);
        await _context.SaveChangesAsync();

        foreach (var item in gioHang)
        {
            _context.ChiTietDonHangs.Add(new ChiTietDonHang
            {
                DonHangId = donHang.Id,
                SanPhamId = item.SanPhamId,
                SoLuong = item.SoLuong,
                GiaLucMua = item.SanPham?.GiaTien ?? 0
            });
        }

        // Xóa giỏ hàng sau khi đặt hàng thành công
        _context.GioHangs.RemoveRange(gioHang);
        await _context.SaveChangesAsync();

        return View("HoanThanh", donHang.Id);
    }
}