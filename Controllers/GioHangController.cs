using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OceanShellCraft.Models;
using OceanShellCraft.Models.ViewModels;

namespace OceanShellCraft.Controllers
{
    [Route("gio-hang")]
    public class GioHangController : Controller
    {
        private readonly MyNgheDbContext _context;

        public GioHangController(MyNgheDbContext context) => _context = context;

        // Lấy hoặc tạo MaPhien để quản lý giỏ hàng qua Cookie
        private string LayMaPhien()
        {
            string maPhien = Request.Cookies["MaPhienGioHang"];
            if (string.IsNullOrEmpty(maPhien))
            {
                maPhien = Guid.NewGuid().ToString();
                Response.Cookies.Append("MaPhienGioHang", maPhien, new CookieOptions { Expires = DateTime.Now.AddDays(30) });
            }
            return maPhien;
        }

        // 1. TRANG DANH SÁCH GIỎ HÀNG
        [Route("")]
        public async Task<IActionResult> DanhSach()
        {
            string maPhien = LayMaPhien();
            var gioHang = await _context.GioHangs
                .Include(g => g.SanPham)
                .Where(g => g.MaPhien == maPhien)
                .ToListAsync();
            return View(gioHang);
        }

        // 2. THÊM SẢN PHẨM VÀO GIỎ
        [HttpGet("/GioHang/ThemVaoGio")]
        public async Task<IActionResult> ThemVaoGio(int sanPhamId)
        {
            string maPhien = LayMaPhien();
            var mucGioHang = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaPhien == maPhien && g.SanPhamId == sanPhamId);

            if (mucGioHang == null)
            {
                _context.GioHangs.Add(new GioHang { MaPhien = maPhien, SanPhamId = sanPhamId, SoLuong = 1 });
            }
            else
            {
                mucGioHang.SoLuong++;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("DanhSach");
        }

        // 3. XÓA KHỎI GIỎ
        [HttpPost("xoa/{id}")]
        public async Task<IActionResult> XoaKhoiGio(int id)
        {
            var muc = await _context.GioHangs.FindAsync(id);
            if (muc != null) _context.GioHangs.Remove(muc);
            await _context.SaveChangesAsync();
            return RedirectToAction("DanhSach");
        }

        // 4. CẬP NHẬT SỐ LƯỢNG (AJAX)
        [HttpPost("cap-nhat")]
        public async Task<IActionResult> CapNhatSoLuong(int id, int soLuong)
        {
            var muc = await _context.GioHangs.FindAsync(id);
            if (muc != null && soLuong > 0)
            {
                muc.SoLuong = soLuong;
                await _context.SaveChangesAsync();
            }
            return Ok(new { success = true });
        }

        // 5. TRANG ĐẶT HÀNG (GET)
        [Authorize]
        [Route("dat-hang")]
        public IActionResult DatHang()
        {
            string maPhien = Request.Cookies["MaPhienGioHang"] ?? "";
            var items = _context.GioHangs.Include(g => g.SanPham).Where(g => g.MaPhien == maPhien).ToList();

            if (!items.Any()) return RedirectToAction("DanhSach");

            var vm = new GioHangViewModel
            {
                DanhSachItem = items,
                // Tính tổng tiền dựa trên giá khuyến mãi nếu có
                TongTien = items.Sum(i => i.SoLuong * ((i.SanPham.GiaKhuyenMai > 0 && i.SanPham.GiaKhuyenMai < i.SanPham.GiaTien) ? i.SanPham.GiaKhuyenMai.Value : i.SanPham.GiaTien))
            };
            return View(vm);
        }

        // 6. KIỂM TRA MÃ GIẢM GIÁ (AJAX)
        [HttpPost("kiem-tra-giam-gia")]
        public async Task<IActionResult> KiemTraGiamGia(string maCode, decimal tamTinh)
        {
            var now = DateTime.Now;
            var v = await _context.GiamGias.FirstOrDefaultAsync(x => x.MaVoucher == maCode && x.IsKichHoat
                                                                    && x.NgayBatDau <= now && x.HanSuDung >= now);

            if (v == null) return Json(new { success = false, message = "Mã không tồn tại hoặc đã hết hạn." });
            if (v.SoLuongGioiHan > 0 && v.SoLuongDaDung >= v.SoLuongGioiHan) return Json(new { success = false, message = "Mã ưu đãi này đã hết lượt sử dụng." });
            if ((double)tamTinh < v.DonHangToiThieu) return Json(new { success = false, message = $"Đơn hàng tối thiểu từ {v.DonHangToiThieu:N0}đ để áp dụng mã này." });

            decimal giam = 0;

            if (v.LoaiGiam == LoaiGiam.PhanTram)
            {
                // Tính số tiền giảm theo %
                giam = tamTinh * (decimal)(v.GiaTriGiam / 100);

                // Nếu có quy định số tiền giảm tối đa, thì lấy giá trị nhỏ hơn
                if (v.GiamToiDa.HasValue)
                {
                    giam = Math.Min(giam, (decimal)v.GiamToiDa.Value);
                }
            }
            else
            {
                // Giảm theo số tiền cố định
                giam = (decimal)v.GiaTriGiam;
            }

            // Đảm bảo số tiền giảm không vượt quá tổng tiền tạm tính
            giam = Math.Min(giam, tamTinh);

            return Json(new
            {
                success = true,
                soTienGiam = giam,
                tongTienMoi = tamTinh - giam,
                message = "Áp dụng mã ưu đãi thành công!"
            });
        }

        // 7. XÁC NHẬN ĐẶT HÀNG (POST)
        [HttpPost("xac-nhan")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanDatHang(string SoDienThoai, string DiaChi, string MaGiamGia, List<int> ListSanPhamId, List<int> ListSoLuong)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("DangNhap", "TaiKhoan");
            var userId = int.Parse(userIdClaim.Value);

            // Bước 1: Khởi tạo đơn hàng (Để trống tiền để tính toán server-side cho an toàn)
            var donHang = new DonHang
            {
                NguoiDungId = userId,
                NgayDat = DateTime.Now,
                SoDienThoai = SoDienThoai,
                DiaChi = DiaChi,
                MaGiamGia = MaGiamGia,
                TrangThai = "Chờ xử lý",
                SoTienGiam = 0,
                TongTien = 0
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync(); // Lưu để lấy ID đơn hàng

            decimal tongTamTinh = 0;

            // Bước 2: Lưu chi tiết đơn hàng và tính tổng tạm tính thực tế
            for (int i = 0; i < ListSanPhamId.Count; i++)
            {
                var sanPham = await _context.SanPhams.FindAsync(ListSanPhamId[i]);
                if (sanPham != null)
                {
                    // Lấy giá thực tế (ưu tiên khuyến mãi)
                    var giaHienTai = (sanPham.GiaKhuyenMai > 0 && sanPham.GiaKhuyenMai < sanPham.GiaTien) ? sanPham.GiaKhuyenMai.Value : sanPham.GiaTien;

                    var ct = new ChiTietDonHang
                    {
                        DonHangId = donHang.Id,
                        SanPhamId = sanPham.Id,
                        SoLuong = ListSoLuong[i],
                        GiaLucMua = giaHienTai
                    };
                    _context.ChiTietDonHangs.Add(ct);
                    tongTamTinh += ListSoLuong[i] * giaHienTai;
                }
            }

            // Bước 3: Kiểm tra và áp dụng giảm giá (Tính lại trên Server để chống hack giá)
            decimal soTienGiamThucTe = 0;
            if (!string.IsNullOrEmpty(MaGiamGia))
            {
                var v = await _context.GiamGias.FirstOrDefaultAsync(x => x.MaVoucher == MaGiamGia && x.IsKichHoat);

                // Kiểm tra điều kiện áp dụng mã
                if (v != null && (double)tongTamTinh >= v.DonHangToiThieu)
                {
                    if (v.LoaiGiam == LoaiGiam.PhanTram)
                    {
                        // Tính số tiền giảm dựa trên %
                        decimal phanTramGiam = tongTamTinh * (decimal)(v.GiaTriGiam / 100);

                        // Nếu có GiamToiDa thì lấy giá trị nhỏ hơn, nếu không thì lấy toàn bộ số tiền đã tính theo %
                        if (v.GiamToiDa.HasValue)
                        {
                            soTienGiamThucTe = Math.Min(phanTramGiam, (decimal)v.GiamToiDa.Value);
                        }
                        else
                        {
                            soTienGiamThucTe = phanTramGiam;
                        }
                    }
                    else
                    {
                        // Nếu giảm theo số tiền cố định
                        soTienGiamThucTe = (decimal)v.GiaTriGiam;
                    }

                    // Đảm bảo số tiền giảm không bao giờ lớn hơn tổng giá trị đơn hàng
                    soTienGiamThucTe = Math.Min(soTienGiamThucTe, tongTamTinh);

                    v.SoLuongDaDung++; // Tăng lượt sử dụng
                }
            }
            // Bước 4: Cập nhật số tiền cuối cùng vào Đơn hàng
            donHang.SoTienGiam = soTienGiamThucTe;
            donHang.TongTien = tongTamTinh - soTienGiamThucTe;

            // Bước 5: Xóa giỏ hàng sau khi đặt thành công
            string maPhien = Request.Cookies["MaPhienGioHang"] ?? "";
            var cartItems = _context.GioHangs.Where(g => g.MaPhien == maPhien);
            _context.GioHangs.RemoveRange(cartItems);

            await _context.SaveChangesAsync();
            return View("CamOn");
        }
    }
}