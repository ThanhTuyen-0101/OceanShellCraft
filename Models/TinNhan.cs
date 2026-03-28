using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OceanShellCraft.Models
{
    public class TinNhan
    {
        [Key]
        public int Id { get; set; }

        // ID của khách hàng đang chat
        [Required]
        public int NguoiDungId { get; set; }

        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? KhachHang { get; set; }

        [Required]
        public string NoiDung { get; set; } = string.Empty;

        // Xác định ai là người gửi. True = Admin gửi, False = Khách hàng gửi
        public bool IsAdmin { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.Now;
    }
}

