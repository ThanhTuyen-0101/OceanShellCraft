namespace OceanShellCraft.Models.ViewModels
{
    public class AdminDashboardVM
    {
        public int TongDonHang { get; set; }
        public int DonHangHomNay { get; set; }
        public int DonHangThangNay { get; set; }

        public decimal TongDoanhThu { get; set; }
        public decimal DoanhThuHomNay { get; set; }
        public decimal DoanhThuThangNay { get; set; }

        public int TongKhachHang { get; set; }
        public int KhachHangMoiThangNay { get; set; }

        public int TongSanPham { get; set; }
        public int SanPhamSapHetHang { get; set; }

        // Danh sách hiển thị
        public List<OceanShellCraft.Models.DonHang> DonHangGanDay { get; set; }
        public List<OceanShellCraft.Models.SanPham> TopSanPhamBanChay { get; set; }
        public List<OceanShellCraft.Models.SanPham> SanPhamTonKhoThap { get; set; }

        // Dữ liệu biểu đồ (Trạng thái đơn hàng)
        public int DonChoXuLy { get; set; }
        public int DonDangGiao { get; set; }
        public int DonHoanThanh { get; set; }
        public List<double> DoanhThuTheoNgay { get; set; } = new List<double>();
        public List<string> NhanBieuDo { get; set; } = new List<string>();
        public double DoanhThuCaoNhat { get; set; } // Để tính phần trăm chiều cao của cột (height %)
    }
}


