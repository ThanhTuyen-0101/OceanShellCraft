using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OceanShellCraft.Models;

namespace OceanShellCraft.ViewComponents
{
    // Tên class phải kết thúc bằng cụm từ ViewComponent
    public class MenuDanhMucViewComponent : ViewComponent
    {
        private readonly MyNgheDbContext _context;

        public MenuDanhMucViewComponent(MyNgheDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Dùng AsNoTracking để nhẹ máy, tránh crash trình Debug
            var danhMucs = await _context.DanhMucs.AsNoTracking().ToListAsync();
            return View("MacDinh", danhMucs);
        }
    }
}