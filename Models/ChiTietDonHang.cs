using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceanShellCraft.Models
{
    public class ChiTietDonHang
    {
        [Key]
        public int Id { get; set; }
        public int DonHangId { get; set; }
        public int SanPhamId { get; set; }
        public int SoLuong { get; set; }
        public decimal GiaLucMua { get; set; }

        public virtual DonHang? DonHang { get; set; }
        public virtual SanPham? SanPham { get; set; }
    }
}
