using System.ComponentModel.DataAnnotations;

namespace OceanShellCraft.Models
{
    public class GiamGia
    {
        [Key]
        public int Id { get; set; }
        public string MaVoucher { get; set; } = string.Empty; // VD: OCEAN10
        public string TenVoucher { get; set; } = string.Empty; // VD: Giảm 10%
        public DateTime HanSuDung { get; set; }

        // Cấp riêng cho user nào đó, nếu null là dùng chung
        public int? NguoiDungId { get; set; }
        public virtual NguoiDung? NguoiDung { get; set; }
    }
}
