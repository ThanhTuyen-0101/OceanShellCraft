using System.ComponentModel.DataAnnotations.Schema;

namespace OceanShellCraft.Models
{
    public class BienTheSanPham
    {
        public int Id { get; set; }
        public int SanPhamId { get; set; }
        public string TenBienThe { get; set; } = string.Empty; // VD: "Size L", "Màu Xanh"

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaRieng { get; set; }
        public int SoLuongRieng { get; set; }

        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }
    }
}
