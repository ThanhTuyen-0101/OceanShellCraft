using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OceanShellCraft.Models;
using OceanShellCraft.Models.ViewModels;
using System.IO;
using System.Net;
using System.Net.Mail;
using MiniExcelLibs;

namespace OceanShellCraft.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly MyNgheDbContext _context;

        public AdminController(MyNgheDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public IActionResult Index(string thang)
        {
            DateTime now = DateTime.Now;
            DateTime filterDate = now;

            if (!string.IsNullOrEmpty(thang) && DateTime.TryParseExact(thang, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                filterDate = parsedDate;
            }

            var startOfMonth = new DateTime(filterDate.Year, filterDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var viewModel = new AdminDashboardVM
            {
                TongDonHang = _context.DonHangs.Count(),
                TongKhachHang = _context.NguoiDungs.Count(n => n.VaiTro == "KhachHang"),
                TongSanPham = _context.SanPhams.Count(),
                SanPhamSapHetHang = _context.SanPhams.Count(s => s.SoLuong < 10),
                TongDoanhThu = _context.DonHangs.Where(d => d.TrangThai == "Hoàn thành").Sum(d => d.TongTien),

                DonHangHomNay = _context.DonHangs.Count(d => d.NgayDat.Date == now.Date),
                DoanhThuHomNay = _context.DonHangs.Where(d => d.TrangThai == "Hoàn thành" && d.NgayDat.Date == now.Date).Sum(d => d.TongTien),

                DonHangThangNay = _context.DonHangs.Count(d => d.NgayDat >= startOfMonth && d.NgayDat <= endOfMonth),
                DoanhThuThangNay = _context.DonHangs.Where(d => d.TrangThai == "Hoàn thành" && d.NgayDat >= startOfMonth && d.NgayDat <= endOfMonth).Sum(d => d.TongTien),

                DonChoXuLy = _context.DonHangs.Count(d => d.TrangThai == "Chờ xử lý"),
                DonDangGiao = _context.DonHangs.Count(d => d.TrangThai == "Đang giao"),
                DonHoanThanh = _context.DonHangs.Count(d => d.TrangThai == "Hoàn thành"),

                DonHangGanDay = _context.DonHangs.Include(d => d.NguoiDung).OrderByDescending(d => d.NgayDat).Take(5).ToList(),

                DoanhThuTheoNgay = new List<double>(),
                NhanBieuDo = new List<string>()
            };

            var donHangTrongThang = _context.DonHangs
                .Where(d => d.TrangThai == "Hoàn thành" && d.NgayDat >= startOfMonth && d.NgayDat <= endOfMonth)
                .Select(d => new { d.NgayDat.Day, d.TongTien })
                .ToList();

            var doanhThuGroup = donHangTrongThang
                .GroupBy(d => d.Day)
                .ToDictionary(g => g.Key, g => (double)g.Sum(d => d.TongTien));
            int daysInMonth = DateTime.DaysInMonth(filterDate.Year, filterDate.Month);
            double maxDoanhThu = 0;

            for (int i = 1; i <= daysInMonth; i++)
            {
                double doanhThuNgay = doanhThuGroup.ContainsKey(i) ? doanhThuGroup[i] : 0;

                viewModel.DoanhThuTheoNgay.Add(doanhThuNgay);
                viewModel.NhanBieuDo.Add(i.ToString());

                if (doanhThuNgay > maxDoanhThu) maxDoanhThu = doanhThuNgay;
            }

            viewModel.DoanhThuCaoNhat = maxDoanhThu == 0 ? 1 : maxDoanhThu;

            ViewBag.ThangDuocChon = filterDate.ToString("yyyy-MM");

            return View(viewModel);
        }

        [HttpGet("bai-viet")]
        public async Task<IActionResult> BaiViet(string search)
        {
            var query = _context.BaiViets.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.TieuDe.Contains(search) || b.TacGia.Contains(search));
            }

            var danhSach = await query.OrderByDescending(b => b.NgayTao).ToListAsync();

            ViewBag.Search = search;

            return View(danhSach);
        }

        [HttpPost("xoa-nhieu-bai-viet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaNhieuBaiViet([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "Không có bài viết nào được chọn." });

            try
            {
                var listXoa = await _context.BaiViets.Where(b => ids.Contains(b.Id)).ToListAsync();

                foreach (var item in listXoa)
                {
                    if (!string.IsNullOrEmpty(item.AnhNen))
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", item.AnhNen.TrimStart('/'));
                        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    }
                }

                _context.BaiViets.RemoveRange(listXoa);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã xóa thành công {listXoa.Count} bài viết." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("them-sua-bai-viet/{id?}")]
        public async Task<IActionResult> ThemSuaBaiViet(int? id)
        {
            if (id.HasValue)
            {
                ViewData["Title"] = "Sửa bài giới thiệu";
                var baiViet = await _context.BaiViets.FindAsync(id);
                if (baiViet == null) return NotFound();

                return View(baiViet);
            }

            ViewData["Title"] = "Thêm mới bài giới thiệu";
            return View(new BaiViet());
        }

        [HttpPost("them-sua-bai-viet/{id?}")]
        public async Task<IActionResult> ThemSuaBaiViet(BaiViet model, IFormFile? fileAnhNen)
        {
            if (fileAnhNen != null && fileAnhNen.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(fileAnhNen.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                await fileAnhNen.CopyToAsync(fileStream);

                model.AnhNen = "/images/" + fileName;
            }

            if (model.Id == 0)
            {
                model.NgayTao = DateTime.Now;
                _context.BaiViets.Add(model);
            }
            else
            {
                if (string.IsNullOrEmpty(model.AnhNen))
                {
                    var baiVietCu = await _context.BaiViets
                        .AsNoTracking()
                        .FirstOrDefaultAsync(b => b.Id == model.Id);

                    if (baiVietCu != null)
                        model.AnhNen = baiVietCu.AnhNen;
                }

                _context.BaiViets.Update(model);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("BaiViet");
        }

        [HttpPost("xoa-bai-viet/{id}")]
        public async Task<IActionResult> XoaBaiViet(int id)
        {
            var baiViet = await _context.BaiViets.FindAsync(id);

            if (baiViet != null)
            {
                if (!string.IsNullOrEmpty(baiViet.AnhNen))
                {
                    var filePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        baiViet.AnhNen.TrimStart('/')
                    );

                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _context.BaiViets.Remove(baiViet);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("BaiViet");
        }

        [HttpGet("san-pham")]
        public async Task<IActionResult> SanPham()
        {
            var danhSach = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .ToListAsync();

            return View(danhSach);
        }

        [HttpGet("them-sua-san-pham/{id?}")]
        public async Task<IActionResult> ThemSuaSanPham(int? id)
        {
            var danhMucs = await _context.DanhMucs.ToListAsync();
            ViewBag.DanhMucList = new SelectList(danhMucs, "Id", "TenDanhMuc");

            if (id.HasValue && id > 0)
            {
                var sanPham = await _context.SanPhams.Include(s => s.DanhMuc).FirstOrDefaultAsync(s => s.Id == id);
                if (sanPham == null) return NotFound();
                ViewBag.CurrentDanhMucName = sanPham.DanhMuc?.TenDanhMuc;
                return View(sanPham);
            }

            return View(new SanPham());
        }

        [HttpPost("them-sua-san-pham/{id?}")]
        public async Task<IActionResult> ThemSuaSanPham(SanPham model, IFormFile? fileHinhAnh)
        {
            if (model.DanhMucId <= 0)
            {
                ModelState.AddModelError("DanhMucId", "Vui lòng chọn danh mục sản phẩm.");

                var danhMucs = await _context.DanhMucs.ToListAsync();
                ViewBag.DanhMucList = new SelectList(danhMucs, "Id", "TenDanhMuc");
                return View(model);
            }

            if (fileHinhAnh != null && fileHinhAnh.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "sanpham");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileHinhAnh.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileHinhAnh.CopyToAsync(fileStream);
                }

                model.HinhAnh = "/images/sanpham/" + fileName;
            }

            try
            {
                if (model.Id == 0)
                {
                    _context.SanPhams.Add(model);
                }
                else
                {
                    if (string.IsNullOrEmpty(model.HinhAnh))
                    {
                        var currentSp = await _context.SanPhams.AsNoTracking().FirstOrDefaultAsync(p => p.Id == model.Id);
                        if (currentSp != null) model.HinhAnh = currentSp.HinhAnh;
                    }
                    _context.SanPhams.Update(model);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("SanPham");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Không thể lưu dữ liệu: " + ex.Message);
                var danhMucs = await _context.DanhMucs.ToListAsync();
                ViewBag.DanhMucList = new SelectList(danhMucs, "Id", "TenDanhMuc");
                return View(model);
            }
        }

        [HttpPost("xoa-san-pham/{id}")]
        public async Task<IActionResult> XoaSanPham(int id)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);

            if (sanPham != null)
            {
                if (!string.IsNullOrEmpty(sanPham.HinhAnh))
                {
                    var filePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        sanPham.HinhAnh.TrimStart('/')
                    );

                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _context.SanPhams.Remove(sanPham);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("SanPham");
        }

        [HttpPost("toggle-noi-bat/{id}")]
        public async Task<IActionResult> ToggleNoiBat(int id)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null)
                return NotFound(new { success = false });

            sanPham.IsFeatured = !sanPham.IsFeatured;

            _context.Update(sanPham);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, isFeatured = sanPham.IsFeatured });
        }

        [HttpGet("giam-gia")]
        public async Task<IActionResult> GiamGia(string search, string trangThai)
        {
            var query = _context.GiamGias.Include(g => g.NguoiDung).AsQueryable();

            ViewBag.TongLuotDung = await query.SumAsync(g => g.SoLuongDaDung);
            ViewBag.TongTienGiam = 0;
            ViewBag.MaHieuQua = await query.OrderByDescending(g => g.SoLuongDaDung).Select(g => g.MaVoucher).FirstOrDefaultAsync() ?? "N/A";

            if (!string.IsNullOrEmpty(search))
                query = query.Where(g => g.MaVoucher.Contains(search) || g.TenVoucher.Contains(search));

            if (trangThai == "active")
                query = query.Where(g => g.IsKichHoat && g.HanSuDung >= DateTime.Now);
            else if (trangThai == "expired")
                query = query.Where(g => g.HanSuDung < DateTime.Now);

            return View(await query.OrderByDescending(g => g.Id).ToListAsync());
        }

        [HttpGet("them-sua-giam-gia/{id?}")]
        public async Task<IActionResult> ThemSuaGiamGia(int? id)
        {
            var khachHangs = await _context.NguoiDungs.Where(n => n.VaiTro == "KhachHang").ToListAsync();
            ViewBag.KhachHangList = new SelectList(khachHangs, "Id", "Email");

            if (id.HasValue && id > 0)
            {
                var item = await _context.GiamGias.FindAsync(id);
                if (item == null) return NotFound();
                return View(item);
            }
            return View(new GiamGia { NgayBatDau = DateTime.Now, HanSuDung = DateTime.Now.AddDays(7) });
        }

        [HttpPost("them-sua-giam-gia/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemSuaGiamGia(GiamGia model)
        {
            if (!ModelState.IsValid)
            {
                var khachHangs = await _context.NguoiDungs.Where(n => n.VaiTro == "KhachHang").ToListAsync();
                ViewBag.KhachHangList = new SelectList(khachHangs, "Id", "Email", model.NguoiDungId);
                return View(model);
            }

            try
            {
                model.MaVoucher = model.MaVoucher?.ToUpper().Trim();

                var isExist = await _context.GiamGias.AnyAsync(g => g.MaVoucher == model.MaVoucher && g.Id != model.Id);
                if (isExist)
                {
                    ModelState.AddModelError("MaVoucher", "Mã này đã tồn tại trên hệ thống!");
                    var khachHangs = await _context.NguoiDungs.Where(n => n.VaiTro == "KhachHang").ToListAsync();
                    ViewBag.KhachHangList = new SelectList(khachHangs, "Id", "Email");
                    return View(model);
                }

                if (model.Id == 0) _context.GiamGias.Add(model);
                else _context.Update(model);

                await _context.SaveChangesAsync();
                return RedirectToAction("GiamGia");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost("xoa-giam-gia/{id}")]
        public async Task<IActionResult> XoaGiamGia(int id)
        {
            var item = await _context.GiamGias.FindAsync(id);
            if (item != null)
            {
                _context.GiamGias.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("GiamGia");
        }

        [HttpGet("get-chat-history")]
        public async Task<IActionResult> GetChatHistory(int userId)
        {
            var history = await _context.TinNhans
                .Where(t => t.NguoiDungId == userId)
                .OrderBy(t => t.ThoiGian)
                .Select(t => new
                {
                    t.NoiDung,
                    t.IsAdmin,
                    ThoiGian = t.ThoiGian.ToString("HH:mm"),
                    UserName = t.IsAdmin ? "OceanShellCraft" : "Khách hàng"
                })
                .ToListAsync();
            return Json(history);
        }

        [HttpGet("chat")]
        public async Task<IActionResult> Chat()
        {
            var uniqueUsers = await _context.TinNhans
                .Include(t => t.KhachHang)
                .GroupBy(t => t.NguoiDungId)
                .Select(g => new
                {
                    UserId = g.Key,
                    UserName = g.First().KhachHang.HoTen ?? "Khách hàng",
                    LastMessage = g.OrderByDescending(t => t.ThoiGian).First().NoiDung,
                    LastTime = g.OrderByDescending(t => t.ThoiGian).First().ThoiGian
                })
                .OrderByDescending(x => x.LastTime)
                .ToListAsync();

            ViewBag.CustomerList = uniqueUsers;
            return View();
        }

        [HttpGet("hoa-don/{id?}")]
        public async Task<IActionResult> HoaDon(int? id)
        {
            var query = _context.DonHangs.Include(d => d.NguoiDung).AsQueryable();

            if (id.HasValue)
            {
                query = query.Where(d => d.Id == id.Value);
            }

            var orders = await query.OrderByDescending(d => d.NgayDat).ToListAsync();

            var selectedOrder = id.HasValue
                ? orders.FirstOrDefault(o => o.Id == id)
                : orders.FirstOrDefault();

            if (selectedOrder != null)
            {
                await _context.Entry(selectedOrder)
                    .Collection(o => o.ChiTietDonHangs)
                    .Query()
                    .Include(ct => ct.SanPham)
                    .LoadAsync();
            }

            ViewBag.SelectedOrder = selectedOrder;
            return View(orders);
        }

        [HttpPost("cap-nhat-trang-thai/{id}")]
        public async Task<IActionResult> CapNhatTrangThai(int id)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.NguoiDung)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (donHang == null)
            {
                return NotFound();
            }

            donHang.TrangThai = "Đã thanh toán";
            _context.Update(donHang);
            await _context.SaveChangesAsync();

            if (donHang.NguoiDung != null && !string.IsNullOrEmpty(donHang.NguoiDung.Email))
            {
                await GuiEmailHoaDonAsync(donHang);
            }

            return RedirectToAction("HoaDon", new { id = donHang.Id });
        }

        private async Task GuiEmailHoaDonAsync(DonHang donHang)
        {
            try
            {
                var emailNguoiGui = "tuyenthanhthanh3979@gmail.com";
                var matKhauUngDung = "leqflrdlpvuffrih";

                var mail = new MailMessage();
                mail.From = new MailAddress(emailNguoiGui, "OceanShellCraft");
                mail.To.Add(donHang.NguoiDung.Email);
                mail.Subject = $"Hóa đơn mua hàng - Mã đơn #{donHang.Id}";
                mail.IsBodyHtml = true;

                string htmlBody = $@"
            <div style='font-family: Times New Roman, serif; color: #333;'>
                <h2>Cảm ơn bạn đã mua hàng tại OceanShellCraft!</h2>
                <p>Đơn hàng #{donHang.Id} của bạn đã được xác nhận thanh toán thành công.</p>
                <p>Thông tin giao hàng:</p>
                <ul>
                    <li>Người nhận: {donHang.NguoiDung.HoTen}</li>
                    <li>Điện thoại: {donHang.SoDienThoai}</li>
                    <li>Địa chỉ: {donHang.DiaChi}</li>
                </ul>
                <table border='1' cellpadding='10' style='border-collapse: collapse; width: 100%; border-color: #ccc;'>
                    <thead style='background-color: #f9f9f9;'>
                        <tr>
                            <th align='left' style='font-weight: normal;'>Sản phẩm</th>
                            <th align='center' style='font-weight: normal;'>Số lượng</th>
                            <th align='right' style='font-weight: normal;'>Đơn giá</th>
                            <th align='right' style='font-weight: normal;'>Thành tiền</th>
                        </tr>
                    </thead>
                    <tbody>";

                foreach (var item in donHang.ChiTietDonHangs)
                {
                    htmlBody += $@"
                        <tr>
                            <td>{item.SanPham?.TenSanPham}</td>
                            <td align='center'>{item.SoLuong}</td>
                            <td align='right'>{item.GiaLucMua:N0}₫</td>
                            <td align='right'>{(item.SoLuong * item.GiaLucMua):N0}₫</td>
                        </tr>";
                }

                htmlBody += $@"
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan='3' align='right'>Tổng thanh toán:</td>
                            <td align='right' style='color: red;'>{donHang.TongTien:N0}₫</td>
                        </tr>
                    </tfoot>
                </table>
                <p>Trân trọng,<br>Đội ngũ OceanShellCraft</p>
            </div>";

                mail.Body = htmlBody;

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(emailNguoiGui, matKhauUngDung);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi gửi email: " + ex.Message);
            }
        }

        [HttpPost("xoa-nhieu-san-pham")]
        public async Task<IActionResult> XoaNhieuSanPham([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "Không có sản phẩm nào được chọn." });

            try
            {
                var danhSachXoa = await _context.SanPhams
                    .Where(s => ids.Contains(s.Id))
                    .ToListAsync();

                foreach (var sanPham in danhSachXoa)
                {
                    if (!string.IsNullOrEmpty(sanPham.HinhAnh))
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", sanPham.HinhAnh.TrimStart('/'));
                        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    }
                }

                _context.SanPhams.RemoveRange(danhSachXoa);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã xóa thành công {danhSachXoa.Count} sản phẩm." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost("nhap-excel")]
        public async Task<IActionResult> NhapExcel(IFormFile file)
        {
            if (file == null || file.Length == 0) return RedirectToAction(nameof(SanPham));

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var rows = stream.Query<SanPhamExcelDto>().ToList();
                    var dsSanPham = new List<SanPham>();

                    foreach (var row in rows)
                    {
                        if (string.IsNullOrWhiteSpace(row.TenSanPham)) continue;

                        var sanPhamMoi = new SanPham
                        {
                            TenSanPham = row.TenSanPham,
                            MoTa = row.MoTa,
                            GiaTien = row.GiaTien,
                            HinhAnh = row.HinhAnh,
                            DanhMucId = row.DanhMucId > 0 ? row.DanhMucId : 1,
                            TrangThai = TrangThaiSanPham.DangBan,
                            SoLuong = 10
                        };

                        if (!string.IsNullOrWhiteSpace(row.ChatLieu) || !string.IsNullOrWhiteSpace(row.KichThuoc))
                        {
                            var bienThe = new BienTheSanPham
                            {
                                TenBienThe = "Bản tiêu chuẩn",
                                SoLuongRieng = 10,
                                ChiTietBienThes = new List<ChiTietBienThe>()
                            };

                            if (!string.IsNullOrWhiteSpace(row.ChatLieu))
                                bienThe.ChiTietBienThes.Add(new ChiTietBienThe { TenThuocTinh = "Chất liệu", GiaTri = row.ChatLieu });

                            if (!string.IsNullOrWhiteSpace(row.KichThuoc))
                                bienThe.ChiTietBienThes.Add(new ChiTietBienThe { TenThuocTinh = "Kích thước", GiaTri = row.KichThuoc });

                            sanPhamMoi.BienThes = new List<BienTheSanPham> { bienThe };
                        }

                        dsSanPham.Add(sanPhamMoi);
                    }

                    _context.SanPhams.AddRange(dsSanPham);
                    await _context.SaveChangesAsync();
                }
                TempData["Success"] = "Nhập dữ liệu thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction(nameof(SanPham));
        }

        [HttpGet("tai-file-mau")]
        public IActionResult TaiFileMau()
        {
            var dataMau = new[] {
                new { TenSanPham = "Tên SP", MoTa = "Mô tả", GiaTien = 100000, HinhAnh = "/img/sp.jpg", ChatLieu = "Vỏ ốc", KichThuoc = "10cm", DanhMucId = 1 }
            };
            var stream = new MemoryStream();
            stream.SaveAs(dataMau);
            stream.Position = 0;
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Nhap_San_Pham.xlsx");
        }

        [HttpGet("xuat-excel")]
        public async Task<IActionResult> XuatExcel()
        {
            var data = await _context.SanPhams
                .Select(s => new SanPhamExcelDto
                {
                    TenSanPham = s.TenSanPham,
                    MoTa = s.MoTa ?? "",
                    GiaTien = s.GiaTien,
                    HinhAnh = s.HinhAnh ?? "",
                    ChatLieu = "",
                    KichThuoc = "",
                    DanhMucId = s.DanhMucId
                })
                .ToListAsync();

            var memoryStream = new MemoryStream();
            memoryStream.SaveAs(data);
            memoryStream.Position = 0;

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Danh_Sach_San_Pham_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpPost("xoa-nguoi-dung/{id}")]
        public async Task<IActionResult> XoaNguoiDung(int id)
        {
            var nguoiDung = await _context.NguoiDungs.FindAsync(id);

            if (nguoiDung != null)
            {
                if (nguoiDung.VaiTro == "Admin")
                {
                    TempData["Error"] = "Không thể xóa tài khoản Quản trị viên!";
                    return RedirectToAction(nameof(NguoiDung));
                }

                _context.NguoiDungs.Remove(nguoiDung);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa tài khoản thành công!";
            }

            return RedirectToAction(nameof(NguoiDung));
        }

        [HttpGet("quan-ly-bien-the")]
        public async Task<IActionResult> QuanLyBienThe()
        {
            var model = new CaiDatViewModel
            {
                SanPhams = await _context.SanPhams.OrderByDescending(s => s.Id).ToListAsync(),

                BienThes = await _context.BienTheSanPhams
                                 .Include(b => b.SanPham)
                                 .OrderByDescending(b => b.Id)
                                 .ToListAsync()
            };
            return View(model);
        }

        [HttpPost("luu-danh-sach-bien-the")]
        public async Task<IActionResult> LuuDanhSachBienThe(int ParentProductId, List<BienTheSanPham> Variants)
        {
            if (ParentProductId <= 0 || Variants == null || !Variants.Any())
            {
                TempData["Error"] = "Dữ liệu không hợp lệ hoặc danh sách biến thể trống!";
                return RedirectToAction("QuanLyBienThe");
            }

            try
            {
                foreach (var item in Variants)
                {
                    item.SanPhamId = ParentProductId;
                    _context.BienTheSanPhams.Add(item);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Hệ thống đã khởi tạo thành công {Variants.Count} biến thể cho sản phẩm.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi lưu dữ liệu: " + ex.Message;
            }

            return RedirectToAction("QuanLyBienThe");
        }

        [HttpPost("xoa-bien-the/{id}")]
        public async Task<IActionResult> XoaBienThe(int id)
        {
            var item = await _context.BienTheSanPhams.FindAsync(id);
            if (item != null)
            {
                _context.BienTheSanPhams.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa biến thể khỏi hệ thống.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy biến thể cần xóa!";
            }
            return RedirectToAction("QuanLyBienThe");
        }

        [HttpGet("quan-ly-danh-muc")]
        public async Task<IActionResult> QuanLyDanhMuc()
        {
            var model = new CaiDatViewModel
            {
                DanhMucs = await _context.DanhMucs.OrderByDescending(d => d.Id).ToListAsync()
            };
            return View(model);
        }

        [HttpGet("cai-dat")]
        public IActionResult CaiDat()
        {
            return View(new CaiDatViewModel());
        }

        [HttpPost("them-sua-danh-muc")]
        public async Task<IActionResult> ThemSuaDanhMuc(int Id, string TenDanhMuc)
        {
            if (string.IsNullOrWhiteSpace(TenDanhMuc))
            {
                TempData["Error"] = "Tên danh mục không được để trống!";
                return RedirectToAction("QuanLyDanhMuc");
            }

            if (Id == 0)
            {
                _context.DanhMucs.Add(new DanhMuc { TenDanhMuc = TenDanhMuc });
                TempData["Success"] = "Thêm thành công!";
            }
            else
            {
                var cat = await _context.DanhMucs.FindAsync(Id);
                if (cat != null)
                {
                    cat.TenDanhMuc = TenDanhMuc;
                    _context.Update(cat);
                    TempData["Success"] = "Cập nhật thành công!";
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("QuanLyDanhMuc");
        }

        [HttpPost("xoa-danh-muc/{id}")]
        public async Task<IActionResult> XoaDanhMuc(int id)
        {
            var cat = await _context.DanhMucs.FindAsync(id);
            if (cat != null)
            {
                var hasProducts = await _context.SanPhams.AnyAsync(s => s.DanhMucId == id);
                if (hasProducts)
                {
                    TempData["Error"] = "Danh mục có sản phẩm, không thể xóa!";
                }
                else
                {
                    _context.DanhMucs.Remove(cat);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa thành công!";
                }
            }
            return RedirectToAction("QuanLyDanhMuc");
        }

        [HttpGet("nguoi-dung")]
        public async Task<IActionResult> NguoiDung(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var query = _context.NguoiDungs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(n => (n.HoTen != null && n.HoTen.Contains(searchString)) ||
                                         (n.Email != null && n.Email.Contains(searchString)));
            }

            return View(await query.OrderByDescending(n => n.Id).ToListAsync());
        }

        [HttpGet("sua-nguoi-dung/{id}")]
        public async Task<IActionResult> SuaNguoiDung(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost("sua-nguoi-dung/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaNguoiDung(int id, NguoiDung model)
        {
            if (id != model.Id) return NotFound();

            var userInDb = await _context.NguoiDungs.FindAsync(id);
            if (userInDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                userInDb.HoTen = model.HoTen;
                userInDb.SoDienThoai = model.SoDienThoai;
                userInDb.DiaChi = model.DiaChi;
                userInDb.VaiTro = model.VaiTro;
                userInDb.IsLocked = model.IsLocked;

                _context.Update(userInDb);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(NguoiDung));
            }
            return View(model);
        }

        [HttpPost("toggle-lock-user/{id}")]
        public async Task<IActionResult> ToggleLockUser(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null || user.VaiTro == "Admin") return Json(new { success = false });

            user.IsLocked = !user.IsLocked;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}