namespace OceanShellCraft.Models.ViewModels
{
    public class CaiDatViewModel
    {
        public List<DanhMuc> DanhMucs { get; set; } = new List<DanhMuc>();
        public List<BienTheSanPham> BienThes { get; set; } = new List<BienTheSanPham>();
        public List<SanPham> SanPhams { get; set; } = new List<SanPham>(); // Dùng cho Dropdown chọn sản phẩm
    }
}

