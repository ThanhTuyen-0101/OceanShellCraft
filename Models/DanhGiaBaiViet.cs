using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceanShellCraft.Models
{
    public class DanhGiaBaiViet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NguoiDungId { get; set; }

        [Required]
        public int BaiVietId { get; set; }

        [Required]
        [Range(1, 5)]
        public int SoSao { get; set; }

        [StringLength(1000)]
        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        public bool DaDuyet { get; set; } = true;

        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? NguoiDung { get; set; }

        [ForeignKey("BaiVietId")]
        public virtual BaiViet? BaiViet { get; set; }
    }
}