using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceanShellCraft.Models
{
    public enum TrangThaiSanPham
    {
        [Display(Name = "Đang bán")] DangBan = 0,
        [Display(Name = "Hết hàng")] HetHang = 1,
        [Display(Name = "Ẩn")] An = 2,
        [Display(Name = "Ngừng kinh doanh")] NgungKinhDoanh = 3
    }

    public class SanPham
    {
        public int Id { get; set; }

        [Required]
        public string TenSanPham { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaTien { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? GiaKhuyenMai { get; set; }

        public string? HinhAnh { get; set; }   // Đường dẫn ảnh
        public string? ChatLieu { get; set; }  // VD: Vỏ ốc hương
        public string? KichThuoc { get; set; }

        public bool IsFeatured { get; set; } // Sản phẩm nổi bật
        public bool IsFavorite { get; set; } // Sản phẩm yêu thích

        // --- QUẢN LÝ KHO & TRẠNG THÁI ---
        public int SoLuong { get; set; } // Tồn kho (Chỉ định nghĩa 1 lần duy nhất ở đây)
        public int SoLuongDaBan { get; set; } = 0;
        public TrangThaiSanPham TrangThai { get; set; } = TrangThaiSanPham.DangBan;

        // --- KHÓA NGOẠI DANH MỤC ---
        public int DanhMucId { get; set; }
        public virtual DanhMuc? DanhMuc { get; set; }

        // --- THÔNG TIN NÂNG CAO (Không tạo cột trong DB) ---
        [NotMapped]
        public double DiemDanhGiaTrungBinh => (DanhGias != null && DanhGias.Any())
            ? DanhGias.Average(d => d.SoSao)
            : 0;

        // --- CÁC QUAN HỆ (Navigation Properties) ---
        // Chỉ định nghĩa 1 lần duy nhất cho mỗi danh sách
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
        public virtual ICollection<BienTheSanPham> BienThes { get; set; } = new List<BienTheSanPham>();
    }
}