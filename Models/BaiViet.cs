using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceanShellCraft.Models
{
    public class BaiViet
    {
        [Key]
        public int Id { get; set; }

        // Mặc định là Blog khi tạo mới
        public string? HinhThuc { get; set; } = "Blog";

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string TieuDe { get; set; }

        public string? NoiDungTomTat { get; set; }
        public string? VideoYoutube { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string NoiDung { get; set; }

        public string? AnhNen { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // 1. Tác giả
        public string TacGia { get; set; } = "Admin";

        // 2. Thẻ (Tag)
        public string? TheTags { get; set; }

        // 3. Lên lịch đăng
        public DateTime? NgayDang { get; set; }

        // 4. SEO cơ bản
        public string? UrlSlug { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }

        // 5. Thống kê
        public int LuotXem { get; set; } = 0;
        public int LuotThich { get; set; } = 0;

        // 6. Đánh giá
        public double DiemDanhGiaTrungBinh { get; set; } = 0.0;
        public int TongLuotDanhGia { get; set; } = 0;

        public virtual ICollection<DanhGiaBaiViet>? DanhGiaBaiViets { get; set; }
    }
}