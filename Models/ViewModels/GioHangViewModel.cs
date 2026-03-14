namespace OceanShellCraft.Models.ViewModels
{
    public class GioHangViewModel
    {
        public List<GioHang> DanhSachItem { get; set; } = new List<GioHang>();
        public decimal TongTien => DanhSachItem.Sum(item => item.SoLuong * (item.SanPham?.GiaTien ?? 0));
    }
}