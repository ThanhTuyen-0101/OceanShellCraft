using System.Collections.Generic;
using OceanShellCraft.Models;

namespace OceanShellCraft.Models.ViewModels
{
    public class GioHangViewModel
    {
        // Danh sách tất cả sản phẩm có trong giỏ
        public List<GioHang> DanhSachItem { get; set; } = new List<GioHang>();

        // Tổng tiền của đơn hàng (Đã có { get; set; } để Controller có thể gán giá trị sau khi giảm giá)
        public decimal TongTien { get; set; }

        // Bạn có thể thêm trường này nếu muốn quản lý số tiền đã giảm riêng biệt
        public decimal SoTienGiam { get; set; }
    }
}