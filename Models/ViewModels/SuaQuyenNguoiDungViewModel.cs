namespace OceanShellCraft.Models.ViewModels
{
    public class SuaQuyenNguoiDungViewModel
    {
        public int NguoiDungId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string VaiTroHienTai { get; set; } = string.Empty;
        public List<string> DanhSachVaiTro { get; set; } = new List<string> { "Admin", "KhachHang", "NhanVien" };
    }
}