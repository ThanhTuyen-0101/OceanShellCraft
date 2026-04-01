using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceanShellCraft.Models
{
    public partial class DonHang
    {
        [Key]
        public int Id { get; set; }
        public int NguoiDungId { get; set; }
        public DateTime NgayDat { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; } // Tổng tiền sau cùng (đã trừ giảm giá)

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienGiam { get; set; } // Số tiền được giảm

        public string? MaGiamGia { get; set; }
        public string TrangThai { get; set; } = "Chờ xử lý";
        public string SoDienThoai { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;

        public virtual NguoiDung? NguoiDung { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    }
}