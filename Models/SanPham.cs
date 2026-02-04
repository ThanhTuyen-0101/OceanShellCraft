namespace OceanShellCraft.Models
{
    public class SanPham
    {
        public int Id { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public string? MoTa { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaTien { get; set; }

        public string? HinhAnh { get; set; }   // Đường dẫn ảnh
        public string? ChatLieu { get; set; }  // VD: Vỏ ốc hương
        public string? KichThuoc { get; set; }

        // Khóa ngoại liên kết với Danh mục
        public int DanhMucId { get; set; }
        public DanhMuc? DanhMuc { get; set; }
    }
}
