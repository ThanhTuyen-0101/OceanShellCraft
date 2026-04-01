using System.ComponentModel.DataAnnotations.Schema;

namespace OceanShellCraft.Models
{
    public class BienTheSanPham
    {
        public int Id { get; set; }
        public int SanPhamId { get; set; }

        // BẠN PHẢI THÊM DÒNG NÀY VÀO ĐỂ GIAO DIỆN KHÔNG LỖI
        public string TenBienThe { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaRieng { get; set; }
        public int SoLuongRieng { get; set; }

        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }

        // Khai báo nối bảng chi tiết thuộc tính
        public virtual ICollection<ChiTietBienThe> ChiTietBienThes { get; set; } = new List<ChiTietBienThe>();
    }
}