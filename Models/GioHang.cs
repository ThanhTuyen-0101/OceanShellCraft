using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceanShellCraft.Models
{
    public class GioHang
    {
        [Key]
        public int Id { get; set; }

        // MaPhien dùng để nhận diện giỏ hàng của người chưa đăng nhập hoặc qua Cookie
        [Required]
        public string MaPhien { get; set; } = string.Empty;

        public int SanPhamId { get; set; }

        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải ít nhất là 1")]
        public int SoLuong { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}