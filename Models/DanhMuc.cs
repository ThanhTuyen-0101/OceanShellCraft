using System.Text.Json.Serialization; // Thêm cái này

namespace OceanShellCraft.Models
{
    public class DanhMuc
    {
        public int Id { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;

        [JsonIgnore] // Thêm dòng này để ngăn vòng lặp vô tận khi xuất dữ liệu
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}