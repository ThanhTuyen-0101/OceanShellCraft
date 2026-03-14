using Microsoft.AspNetCore.Mvc;
using OceanShellCraft.Models;
using Microsoft.EntityFrameworkCore;

public class DanhMucController : Controller
{
    private readonly MyNgheDbContext _context;
    public DanhMucController(MyNgheDbContext context) => _context = context;

    // Hiển thị danh sách danh mục hiện có
    public async Task<IActionResult> DanhSach()
    {
        return View(await _context.DanhMucs.ToListAsync());
    }

    // Trang Tạo mới (GET)
    [HttpGet]
    public IActionResult TaoMoi() => View();

    // Xử lý lưu Danh mục (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoMoi([Bind("TenDanhMuc")] DanhMuc danhMuc)
    {
        if (ModelState.IsValid)
        {
            _context.Add(danhMuc);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(DanhSach));
        }
        return View(danhMuc);
    }
}