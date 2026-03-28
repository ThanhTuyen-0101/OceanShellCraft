using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceanShellCraft.Models
{
    public class DanhGia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NguoiDungId { get; set; }

        [Required]
        public int SanPhamId { get; set; }

        public int? DonHangId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5 sao")]
        public int SoSao { get; set; }

        [StringLength(1000, ErrorMessage = "Nội dung đánh giá không quá 1000 ký tự")]
        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        // --- Navigation Properties ---
        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? NguoiDung { get; set; }

        [ForeignKey("DonHangId")]
        public virtual DonHang? DonHang { get; set; }

        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; } // Thêm dòng này để liên kết chặt chẽ
    }
}