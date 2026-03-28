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
    public class AdminController : Controller
    {
        private readonly MyNgheDbContext _context;

        public AdminController(MyNgheDbContext context)
        {
            _context = context;
        }



        public async Task<IActionResult> BaiViet()
        {
            var danhSach = await _context.BaiViets
                .OrderByDescending(b => b.NgayTao)
                .ToListAsync();

            return View(danhSach);
        }

        [HttpGet]
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

        [HttpPost]
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

        public async Task<IActionResult> SanPham()
        {
            var danhSach = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .ToListAsync();

            return View(danhSach);
        }

        [HttpGet]
        public async Task<IActionResult> ThemSuaSanPham(int? id)
        {
            var danhMucs = await _context.DanhMucs.ToListAsync();
            ViewBag.DanhMucList = new SelectList(danhMucs, "Id", "TenDanhMuc");

            if (id.HasValue)
            {
                ViewData["Title"] = "Sửa sản phẩm";
                var sanPham = await _context.SanPhams.FindAsync(id);
                if (sanPham == null) return NotFound();

                return View(sanPham);
            }

            ViewData["Title"] = "Thêm mới sản phẩm";
            return View(new SanPham());
        }

        [HttpPost]
        public async Task<IActionResult> ThemSuaSanPham(SanPham model, IFormFile? fileHinhAnh)
        {
            if (fileHinhAnh != null && fileHinhAnh.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    "sanpham"
                );

                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(fileHinhAnh.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                await fileHinhAnh.CopyToAsync(fileStream);

                model.HinhAnh = "/images/sanpham/" + fileName;
            }

            if (model.Id == 0)
            {
                _context.SanPhams.Add(model);
            }
            else
            {
                if (string.IsNullOrEmpty(model.HinhAnh))
                {
                    var spCu = await _context.SanPhams
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == model.Id);

                    if (spCu != null)
                        model.HinhAnh = spCu.HinhAnh;
                }

                _context.SanPhams.Update(model);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("SanPham");
        }

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

        [HttpPost]
        [Route("Admin/ToggleNoiBat/{id}")]
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

        // 1. Hiển thị danh sách mã giảm giá
        public async Task<IActionResult> GiamGia()
        {
            var danhSach = await _context.GiamGias
                .Include(g => g.NguoiDung)
                .OrderByDescending(g => g.Id)
                .ToListAsync();

            return View(danhSach);
        }

        // 2. Mở form Thêm hoặc Sửa
        [HttpGet]
        public async Task<IActionResult> ThemSuaGiamGia(int? id)
        {
            // Cần có ViewBag này để đổ dữ liệu vào thẻ <select> Khách hàng trong View, nếu không sẽ bị lỗi tiếp
            var khachHangs = await _context.NguoiDungs
                .Where(n => n.VaiTro == "KhachHang")
                .ToListAsync();
            ViewBag.KhachHangList = new SelectList(khachHangs, "Id", "Email");

            if (id.HasValue)
            {
                // Nếu là sửa: Tìm model trong database và truyền sang View
                var giamGia = await _context.GiamGias.FindAsync(id);
                if (giamGia == null) return NotFound();
                return View(giamGia);
            }

            // NẾU LÀ THÊM MỚI: Truyền một đối tượng mới tinh sang View để tránh lỗi "Model is null"
            return View(new GiamGia { HanSuDung = DateTime.Now.AddDays(7) });
        }

        // 3. Xử lý lưu dữ liệu khi bấm nút "Lưu thông tin"
        [HttpPost]
        public async Task<IActionResult> ThemSuaGiamGia(GiamGia model)
        {
            if (!string.IsNullOrEmpty(model.MaVoucher))
            {
                model.MaVoucher = model.MaVoucher.ToUpper().Trim();
            }

            if (model.Id == 0)
            {
                _context.GiamGias.Add(model); // Thêm mới
            }
            else
            {
                _context.GiamGias.Update(model); // Cập nhật
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("GiamGia");
        }
        // Thêm API lấy lịch sử chat
        [HttpGet]
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

        // Cập nhật trang Messages để load danh sách khách hàng đã từng chat
        public async Task<IActionResult> Chat()
        {
            // Lấy danh sách những người dùng đã từng gửi tin nhắn (Unique)
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
        public async Task<IActionResult> HoaDon(int? id)
        {
            // Lấy toàn bộ danh sách hoặc lọc theo ID nếu có tìm kiếm
            var query = _context.DonHangs.Include(d => d.NguoiDung).AsQueryable();

            if (id.HasValue)
            {
                // Nếu tìm kiếm theo mã ID cụ thể
                query = query.Where(d => d.Id == id.Value);
            }

            var orders = await query.OrderByDescending(d => d.NgayDat).ToListAsync();

            // Chọn đơn hàng hiển thị chi tiết: 
            // Nếu có ID tìm kiếm thì chọn đơn đó, nếu không chọn đơn mới nhất
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
        [HttpPost]
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
        public IActionResult Index(string thang)
        {
            // 1. Xác định thời gian cần lọc
            DateTime now = DateTime.Now;
            DateTime filterDate = now;

            // Nếu người dùng có chọn tháng, parse giá trị đó
            if (!string.IsNullOrEmpty(thang) && DateTime.TryParseExact(thang, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                filterDate = parsedDate;
            }

            // Ngày đầu tháng và ngày cuối tháng của tháng được chọn
            var startOfMonth = new DateTime(filterDate.Year, filterDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // 2. Lấy dữ liệu thống kê tổng quan
            var viewModel = new AdminDashboardVM
            {
                // Thống kê tổng (toàn thời gian)
                TongDonHang = _context.DonHangs.Count(),
                TongKhachHang = _context.NguoiDungs.Count(n => n.VaiTro == "KhachHang"),
                TongSanPham = _context.SanPhams.Count(),
                SanPhamSapHetHang = _context.SanPhams.Count(s => s.SoLuong < 10),
                TongDoanhThu = _context.DonHangs.Where(d => d.TrangThai == "Hoàn thành").Sum(d => d.TongTien),

                // Thống kê theo "Hôm nay" (luôn là ngày hiện tại)
                DonHangHomNay = _context.DonHangs.Count(d => d.NgayDat.Date == now.Date),
                DoanhThuHomNay = _context.DonHangs.Where(d => d.TrangThai == "Hoàn thành" && d.NgayDat.Date == now.Date).Sum(d => d.TongTien),

                // Thống kê theo "Tháng" (Dựa vào tháng đang được filter)
                DonHangThangNay = _context.DonHangs.Count(d => d.NgayDat >= startOfMonth && d.NgayDat <= endOfMonth),
                DoanhThuThangNay = _context.DonHangs.Where(d => d.TrangThai == "Hoàn thành" && d.NgayDat >= startOfMonth && d.NgayDat <= endOfMonth).Sum(d => d.TongTien),

                // Phân loại trạng thái đơn (toàn thời gian)
                DonChoXuLy = _context.DonHangs.Count(d => d.TrangThai == "Chờ xử lý"),
                DonDangGiao = _context.DonHangs.Count(d => d.TrangThai == "Đang giao"),
                DonHoanThanh = _context.DonHangs.Count(d => d.TrangThai == "Hoàn thành"),

                // Danh sách đơn gần đây
                DonHangGanDay = _context.DonHangs.Include(d => d.NguoiDung).OrderByDescending(d => d.NgayDat).Take(5).ToList(),

                // Khởi tạo list cho biểu đồ
                DoanhThuTheoNgay = new List<double>(),
                NhanBieuDo = new List<string>()
            };

            // 3. Xử lý dữ liệu thực tế cho Biểu đồ
            // Lấy tất cả các đơn hoàn thành trong tháng được chọn (chỉ lấy Ngày và Tổng Tiền để tối ưu)
            var donHangTrongThang = _context.DonHangs
                .Where(d => d.TrangThai == "Hoàn thành" && d.NgayDat >= startOfMonth && d.NgayDat <= endOfMonth)
                .Select(d => new { d.NgayDat.Day, d.TongTien })
                .ToList();

            // Gom nhóm doanh thu theo từng ngày
            // Gom nhóm doanh thu theo từng ngày
            var doanhThuGroup = donHangTrongThang
                .GroupBy(d => d.Day)
                .ToDictionary(g => g.Key, g => (double)g.Sum(d => d.TongTien));
            int daysInMonth = DateTime.DaysInMonth(filterDate.Year, filterDate.Month);
            double maxDoanhThu = 0;

            for (int i = 1; i <= daysInMonth; i++)
            {
                // Nếu ngày đó có doanh thu thì lấy, không thì gán bằng 0
                double doanhThuNgay = doanhThuGroup.ContainsKey(i) ? doanhThuGroup[i] : 0;

                viewModel.DoanhThuTheoNgay.Add(doanhThuNgay);
                viewModel.NhanBieuDo.Add(i.ToString());

                if (doanhThuNgay > maxDoanhThu) maxDoanhThu = doanhThuNgay;
            }

            viewModel.DoanhThuCaoNhat = maxDoanhThu == 0 ? 1 : maxDoanhThu;

            // Truyền tháng đang chọn ra View để gán vào input <input type="month">
            ViewBag.ThangDuocChon = filterDate.ToString("yyyy-MM");

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> NhapExcel(IFormFile file)
        {
            if (file == null || file.Length == 0) return RedirectToAction(nameof(SanPham));

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    // Đọc dữ liệu từ Excel (bỏ qua dòng tiêu đề nếu cần)
                    var rows = stream.Query().ToList();
                    var dsSanPham = new List<SanPham>();

                    foreach (var row in rows.Skip(1))
                    {
                        dsSanPham.Add(new SanPham
                        {
                            TenSanPham = row.A?.ToString(),
                            MoTa = row.B?.ToString() ?? "",
                            GiaTien = decimal.TryParse(row.C?.ToString(), out decimal gia) ? gia : 0,
                            HinhAnh = row.D?.ToString() ?? "",
                            ChatLieu = row.E?.ToString() ?? "",
                            KichThuoc = row.F?.ToString() ?? "",
                            DanhMucId = int.TryParse(row.G?.ToString(), out int id) ? id : 1,
                            TrangThai = TrangThaiSanPham.DangBan // Mặc định khi nhập mới
                        });
                    }

                    _context.SanPhams.AddRange(dsSanPham);
                    await _context.SaveChangesAsync();
                }
                TempData["Success"] = "Nhập dữ liệu thành công!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra khi đọc file Excel.";
            }

            return RedirectToAction(nameof(SanPham));
        }

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
        [HttpGet]
        public async Task<IActionResult> XuatExcel()
        {
            // 1. Lấy dữ liệu sản phẩm kèm theo tên danh mục
            var data = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .OrderByDescending(s => s.Id)
                .Select(s => new
                {
                    MaSanPham = s.Id,
                    TenSanPham = s.TenSanPham,
                    DanhMuc = s.DanhMuc != null ? s.DanhMuc.TenDanhMuc : "N/A",
                    GiaBan = s.GiaTien,
                    GiaKhuyenMai = s.GiaKhuyenMai ?? 0,
                    TonKho = s.SoLuong,
                    DaBan = s.SoLuongDaBan,
                    ChatLieu = s.ChatLieu ?? "",
                    KichThuoc = s.KichThuoc ?? "",
                    TrangThai = s.TrangThai == TrangThaiSanPham.DangBan ? "Đang bán" :
                                s.TrangThai == TrangThaiSanPham.HetHang ? "Hết hàng" :
                                s.TrangThai == TrangThaiSanPham.An ? "Đang ẩn" : "Ngừng kinh doanh"
                })
                .ToListAsync();

            // 2. Sử dụng MemoryStream để tạo file Excel trong bộ nhớ
            var memoryStream = new MemoryStream();

            // MiniExcel sẽ tự động lấy tên thuộc tính làm tiêu đề cột (MaSanPham, TenSanPham...)
            memoryStream.SaveAs(data);
            memoryStream.Position = 0;

            // 3. Đặt tên file theo ngày giờ hiện tại để tránh trùng lặp
            string fileName = $"Danh_Sach_San_Pham_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            // 4. Trả về file cho trình duyệt tải xuống
            return File(
                memoryStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
        // =========================================================
        //                  QUẢN LÝ NGƯỜI DÙNG
        // =========================================================

        // 1. Hiển thị danh sách và Tìm kiếm người dùng
        public async Task<IActionResult> NguoiDung(string searchString)
        {
            // Giữ lại từ khóa tìm kiếm trên ô input
            ViewData["CurrentFilter"] = searchString;

            // Lấy toàn bộ danh sách người dùng
            var usersQuery = from n in _context.NguoiDungs
                             select n;

            // Nếu có từ khóa tìm kiếm, tiến hành lọc theo Tên hoặc Email
            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(n =>
                    (n.HoTen != null && n.HoTen.Contains(searchString)) ||
                    (n.Email != null && n.Email.Contains(searchString))
                );
            }

            // Sắp xếp người mới đăng ký lên đầu
            var danhSach = await usersQuery.OrderByDescending(n => n.Id).ToListAsync();

            return View(danhSach);
        }

        

        // 3. Xóa người dùng
        public async Task<IActionResult> XoaNguoiDung(int id)
        {
            var nguoiDung = await _context.NguoiDungs.FindAsync(id);

            if (nguoiDung != null)
            {
                // Kiểm tra an toàn: Không cho phép tự ý xóa tài khoản Admin
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
        // Mở form chỉnh sửa người dùng
        [HttpGet]
        public async Task<IActionResult> SuaNguoiDung(int? id)
        {
            if (id == null) return NotFound();

            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null) return NotFound();

            return View(nguoiDung);
        }

        // Lưu dữ liệu khi Admin bấm Cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaNguoiDung(int id, NguoiDung model)
        {
            if (id != model.Id) return NotFound();

            try
            {
                // Lấy user cũ từ Database
                var userInDb = await _context.NguoiDungs.FindAsync(id);
                if (userInDb == null) return NotFound();

                // Chỉ cập nhật những trường cho phép (Không cập nhật Email hay Mật khẩu ở đây để bảo mật)
                userInDb.HoTen = model.HoTen;
                userInDb.SoDienThoai = model.SoDienThoai;
                userInDb.VaiTro = model.VaiTro; // Có thể cấp quyền Admin tại đây

                _context.Update(userInDb);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật thông tin người dùng thành công!";
                return RedirectToAction(nameof(NguoiDung));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.NguoiDungs.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
        }
        // =========================================================
        //                  CÀI ĐẶT HỆ THỐNG
        // =========================================================
        public IActionResult CaiDat()
        {
            // Hiện tại trang Cài đặt (Bento Grid) chủ yếu là giao diện tĩnh chứa các liên kết.
            // Nếu sau này bạn cần truy xuất cấu hình từ database (ví dụ: Logo, Phí ship mặc định...)
            // thì có thể gọi qua _context ở đây rồi truyền sang View.
            return View();
        }

    }

}