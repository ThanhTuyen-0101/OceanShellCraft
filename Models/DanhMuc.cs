namespace OceanShellCraft.Models
{
    public class DanhMuc
    {
        public int Id { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty; // VD: Đèn Trang Trí
        public ICollection<SanPham> DanhSachSanPham { get; set; } = new List<SanPham>();
    }
}
