using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace OceanShellCraft.Hubs
{
    public class ChatHub : Hub
    {
        // Khi khách hàng gửi tin nhắn
        public async Task CustomerSendMessage(string message)
        {
            var userName = Context.User?.Identity?.Name ?? "Khách lạ";
            var userId = Context.ConnectionId; // Định danh phiên kết nối

            // Gửi cho tất cả mọi người (bao gồm cả Admin)
            await Clients.All.SendAsync("ReceiveMessage", userId, userName, message, "KhachHang", DateTime.Now.ToString("HH:mm"));
        }

        // Khi Admin/Nhân viên trả lời
        
        public async Task AdminReplyMessage(string toUserId, string message)
        {
            // Tên hiển thị cố định của cửa hàng khi Admin chat
            string shopName = "OceanShellCraft";

            // Gửi đến khách hàng mục tiêu
            await Clients.All.SendAsync("ReceiveMessage", toUserId, shopName, message, "Admin", DateTime.Now.ToString("HH:mm"));
        }
    }

}
