using System.ComponentModel.DataAnnotations.Schema;

namespace OceanShellCraft.Models
{
    public class ChiTietBienThe
    {
        public int Id { get; set; }

        public int BienTheId { get; set; }

        // Tên thuộc tính (VD: "Kích thước", "Chất liệu", "Màu sắc")
        public string TenThuocTinh { get; set; } = string.Empty;

        // Giá trị tương ứng (VD: "Size L", "Bạc 925", "Màu Tím")
        public string GiaTri { get; set; } = string.Empty;

        [ForeignKey("BienTheId")]
        public virtual BienTheSanPham? BienTheSanPham { get; set; }
    }
}
