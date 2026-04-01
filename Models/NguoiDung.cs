using System.ComponentModel.DataAnnotations;

namespace OceanShellCraft.Models
{
    public class NguoiDung
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string MatKhau { get; set; } = string.Empty;

        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? AnhDaiDien { get; set; }

        public bool IsLocked { get; set; } = false;
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? LastLogin { get; set; }
        public string VaiTro { get; set; } = "KhachHang";

        // Giữ lại các phần quan trọng này
        public int DiemTichLuy { get; set; } = 0;
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
        public virtual ICollection<TinNhan> TinNhans { get; set; } = new List<TinNhan>();
    }
}