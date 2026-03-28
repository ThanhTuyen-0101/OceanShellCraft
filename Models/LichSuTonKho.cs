using System.ComponentModel.DataAnnotations.Schema;

namespace OceanShellCraft.Models
{
    public class LichSuTonKho
    {
        public int Id { get; set; }
        public int SanPhamId { get; set; }
        public DateTime NgayThayDoi { get; set; } = DateTime.Now;
        public int SoLuongThayDoi { get; set; } // Số dương là Nhập, số âm là Xuất
        public string LoaiThayDoi { get; set; } = string.Empty; // "Nhập hàng", "Bán hàng", "Hoàn trả"
        public string? GhiChu { get; set; }

        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }
    }
}
