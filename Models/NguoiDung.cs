using System.ComponentModel.DataAnnotations;

namespace OceanShellCraft.Models
{
    public class NguoiDung
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string MatKhau { get; set; } = string.Empty;

        // --- CÁC TRƯỜNG BỔ SUNG MỚI ---
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; } // Có thể dùng Nam/Nữ/Khác hoặc Enum

        public string? AnhDaiDien { get; set; } // Lưu đường dẫn ảnh thực tế thay vì dùng pravatar
        // ------------------------------

        public string VaiTro { get; set; } = "KhachHang";
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    }
}