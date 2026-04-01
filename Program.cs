using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using OceanShellCraft.Models;
using OceanShellCraft.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext
builder.Services.AddDbContext<MyNgheDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DbConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        }));

// 2. Cấu hình Authentication (ĐÃ THÊM GOOGLE VÀ SỬA LOGIN PATH)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    // Đã đổi thành đường dẫn chuẩn SEO
    options.LoginPath = "/dang-nhap";
    options.AccessDeniedPath = "/tu-choi-truy-cap";
})
.AddGoogle(options =>
{
    // Cấu hình ID và Secret từ Google Cloud Console
    options.ClientId = "486967841291-5q452uhrabmjkhqsm59734vdqp326vtm.apps.googleusercontent.com";
    options.ClientSecret = "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";
});

// 3. Cấu hình upload file
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50MB
});

builder.Services.AddControllersWithViews();

// ==========================================
// QUAN TRỌNG: THÊM SIGNALR VÀO SERVICES
// ==========================================
builder.Services.AddSignalR();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/TrangChu/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();

app.UseRouting();

// Thêm Middleware xác thực và phân quyền
app.UseAuthentication();
app.UseAuthorization();

// ==========================================
// QUAN TRỌNG: ĐỊNH TUYẾN CHAT HUB
// ==========================================
app.MapHub<ChatHub>("/chatHub");

// Cấu hình Routing
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "KhachHang",
    pattern: "KhachHang/{action=TongQuan}/{id?}",
    defaults: new { controller = "KhachHang" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TrangChu}/{action=TrangChu}/{id?}")
    .WithStaticAssets();

app.Run();