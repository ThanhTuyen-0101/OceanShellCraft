using Microsoft.AspNetCore.SignalR;
using OceanShellCraft.Models;
using System.Security.Claims;

namespace OceanShellCraft.Hubs
{
    public class ChatHub : Hub
    {
        private readonly MyNgheDbContext _context;
        public ChatHub(MyNgheDbContext context) { _context = context; }

        public async Task CustomerSendMessage(string message)
        {
            var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return;

            int userId = int.Parse(userIdStr);
            var userName = Context.User?.Identity?.Name ?? "Khách hàng";

            var tinNhan = new TinNhan
            {
                NguoiDungId = userId,
                NoiDung = message,
                IsAdmin = false,
                ThoiGian = DateTime.Now
            };
            _context.TinNhans.Add(tinNhan);
            await _context.SaveChangesAsync();

            string timeStr = tinNhan.ThoiGian.ToString("HH:mm");

            // QUAN TRỌNG: Gửi cho chính User đó (để đồng bộ các tab/vị trí chat)
            await Clients.User(userIdStr).SendAsync("ReceiveMessage", userId, userName, message, "KhachHang", timeStr);

            // Gửi cho Admin
            await Clients.Group("Admins").SendAsync("ReceiveMessage", userId, userName, message, "KhachHang", timeStr);
        }
        public async Task AdminReplyMessage(string targetUserId, string message)
        {
            var adminIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdStr) || string.IsNullOrEmpty(targetUserId)) return;

            int targetId = int.Parse(targetUserId);

            // 1. Lưu tin nhắn vào DB
            var tinNhan = new TinNhan
            {
                NguoiDungId = targetId, // Gắn với ID của khách hàng
                NoiDung = message,
                IsAdmin = true,         // Đánh dấu là Admin gửi
                ThoiGian = DateTime.Now
            };
            _context.TinNhans.Add(tinNhan);
            await _context.SaveChangesAsync();

            string timeStr = tinNhan.ThoiGian.ToString("HH:mm");

            // 2. Gửi cho Khách hàng (để họ nhận được tin)
            await Clients.User(targetUserId).SendAsync("ReceiveMessage", adminIdStr, "OceanShellCraft", message, "Admin", timeStr);

            // 3. Gửi ngược lại cho chính Admin (để đồng bộ các tab Admin đang mở)
            await Clients.Group("Admins").SendAsync("ReceiveMessage", targetUserId, "Khách hàng", message, "Admin", timeStr);
        }
    }
}