using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OceanShellCraft.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext với Retry Policy (Giúp kết nối SQL lỳ đòn trên .NET 10)
builder.Services.AddDbContext<MyNgheDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DbConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        }));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/TaiKhoan/DangNhap"; // Trang chuyển hướng khi chưa đăng nhập
        options.AccessDeniedPath = "/TaiKhoan/TuChoiTruyCap"; // Trang khi khách lẻ đòi vào Admin
    });

// 2. Cấu hình giới hạn upload file (Dùng cho tính năng nhập Excel và thêm ảnh)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50MB
});

// 3. Đăng ký Controllers và Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4. Cấu hình Pipeline xử lý request
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/TrangChu/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// 5. Tính năng mới của .NET 10: Tối ưu hóa việc tải file tĩnh (CSS/JS/Ảnh)
app.MapStaticAssets();

app.UseRouting();
app.UseAuthentication(); // Ai là ai?
app.UseAuthorization();  // Được làm gì?

// --- THÊM MỚI TẠI ĐÂY ---
// Cấu hình Route riêng cho khu vực Admin (Phải đặt TRƯỚC route mặc định)
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=BaiViet}/{id?}",
    defaults: new { controller = "Admin" })
    .WithStaticAssets();
// ------------------------

// 6. Cấu hình Route mặc định (Dành cho trang người dùng bên ngoài)
// ĐÃ SỬA LẠI THÀNH TRANG CHỦ TẠI ĐÂY:
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TrangChu}/{action=TrangChu}/{id?}")
    .WithStaticAssets();

app.Run();