using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace OceanShellCraft.Models
{
    public class DanhMuc
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        public string TenDanhMuc { get; set; } = string.Empty;

        public string? DuongDan { get; set; } // Thay cho Slug

        public string? MoTa { get; set; }

        public int? DanhMucChaId { get; set; } // Cấp bậc danh mục

        public int ThuTu { get; set; } = 1;

        public bool TrangThai { get; set; } = true; // Hoạt động / Ẩn

        public bool HienTrangChu { get; set; } = false;

        // Cấu hình SEO
        public string? TieuDeSeo { get; set; } // Meta Title
        public string? MoTaSeo { get; set; }   // Meta Description

        [JsonIgnore]
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}