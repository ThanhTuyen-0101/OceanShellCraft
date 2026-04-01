using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceanShellCraft.Models
{
    // Định nghĩa loại giảm giá (0: Phần trăm, 1: Số tiền)
    public enum LoaiGiam
    {
        PhanTram = 0,
        SoTien = 1
    }

    public class GiamGia
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã voucher không được để trống")]
        [StringLength(50)]
        public string MaVoucher { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên chương trình không được để trống")]
        [StringLength(255)]
        public string TenVoucher { get; set; } = string.Empty;

        [StringLength(500)]
        public string? MoTa { get; set; }

        // Mặc định là giảm theo %
        public LoaiGiam LoaiGiam { get; set; } = LoaiGiam.PhanTram;

        [Required(ErrorMessage = "Vui lòng nhập giá trị giảm")]
        public double GiaTriGiam { get; set; }

        // Giá trị giảm tối đa (Chỉ áp dụng khi LoaiGiam = PhanTram)
        public double? GiamToiDa { get; set; }

        // Điều kiện áp dụng: Đơn hàng tối thiểu phải đạt bao nhiêu tiền
        public double DonHangToiThieu { get; set; } = 0;

        // Giới hạn số lượng mã (0 = Không giới hạn)
        public int SoLuongGioiHan { get; set; } = 0;

        // Đếm số lượng mã đã được khách sử dụng
        public int SoLuongDaDung { get; set; } = 0;

        // Mỗi khách được dùng tối đa bao nhiêu lần
        public int GioiHanMoiKhach { get; set; } = 1;

        public DateTime NgayBatDau { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn hạn sử dụng")]
        public DateTime HanSuDung { get; set; }
        public bool IsKichHoat { get; set; } = true;
        public int? NguoiDungId { get; set; }

        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? NguoiDung { get; set; }
    }
}