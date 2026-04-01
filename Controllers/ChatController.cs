using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using OceanShellCraft.Models; // ĐẢM BẢO CÓ DÒNG NÀY để gọi MyNgheDbContext
using Microsoft.EntityFrameworkCore;

namespace OceanShellCraft.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : Controller
    {
        // 1. GỌI DATABASE CONTEXT VÀO CONTROLLER
        private readonly MyNgheDbContext _context;

        public ChatController(MyNgheDbContext context)
        {
            _context = context;
        }

        public class ChatRequest
        {
            public string NoiDung { get; set; }
        }

        [HttpPost("ask-ai")]
        public async Task<IActionResult> AskAI([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request?.NoiDung))
            {
                return Ok(new { reply = "Bạn chưa nhập tin nhắn." });
            }

            string userMessage = request.NoiDung;
            string aiResponse = "";

            try
            {
                // ==========================================
                // 2. LẤY DỮ LIỆU TỪ DATABASE (CSDL)
                // Lấy danh sách tên và giá sản phẩm (Giả sử bảng tên là SanPhams)
                // ==========================================
                var rawData = await _context.SanPhams
                    .Where(sp => sp.TrangThai == OceanShellCraft.Models.TrangThaiSanPham.DangBan)
                    .Select(sp => new { sp.TenSanPham, sp.GiaTien })
                    .ToListAsync();

                // Bước 2.2: Dùng C# format thêm dấu phẩy vào giá tiền
                var danhSachSanPham = rawData
                    .Select(sp => $"- {sp.TenSanPham}: {sp.GiaTien:N0} VNĐ")
                    .ToList();

                string dataSanPham = string.Join("\n", danhSachSanPham);
                
                // ==========================================
                // 3. CẬP NHẬT LẠI SYSTEM PROMPT (THÊM DATA VÀO)
                // ==========================================
                string thongTinCuaHang = $@"
Bạn là nhân viên tư vấn nhiệt tình, thân thiện của cửa hàng đồ mỹ nghệ OceanShellCraft.
Dưới đây là thông tin về cửa hàng để bạn trả lời khách:
- Cửa hàng chuyên bán các sản phẩm thủ công mỹ nghệ làm từ vỏ ốc, sao biển, ngọc trai tự nhiên.
- Địa chỉ cửa hàng: 123 Đường Biển, Thành phố Nha Trang, Việt Nam.
- Số điện thoại liên hệ: 0909 123 456.
- Chính sách giao hàng: Miễn phí giao hàng toàn quốc cho đơn hàng từ 500.000 VNĐ trở lên.
- Chính sách bảo hành: Đổi trả miễn phí trong 7 ngày nếu sản phẩm bị lỗi do vận chuyển.

DANH SÁCH SẢN PHẨM VÀ GIÁ BÁN HIỆN TẠI TẠI CỬA HÀNG:
{dataSanPham}

Nguyên tắc trả lời:
1. Luôn xưng hô lịch sự (dạ, vâng, ạ, anh/chị/bạn).
2. Khi khách hỏi giá, hãy tìm trong danh sách sản phẩm ở trên để báo giá cho khách.
3. Nếu khách hỏi sản phẩm không có trong danh sách, hãy nói: 'Dạ hiện tại bên em chưa có sẵn thông tin của sản phẩm này, anh/chị vui lòng để lại số điện thoại hoặc gọi 0909 123 456 để nhân viên kiểm tra kho ạ'.
4. Trả lời tự nhiên, ngắn gọn và thân thiện.
";

                // API Key của bạn
                string apiKey = "AIzaSyDgBOtnH0-VOFtDvWm2qJzEO70bj22ZnOI"; // <-- ĐÃ THAY BẰNG KEY MỚI

                using (var httpClient = new HttpClient())
                {
                    var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
                    string finalPrompt = thongTinCuaHang + "\n\nCâu hỏi của khách hàng: " + userMessage;

                    var payload = new
                    {
                        contents = new[] { new { parts = new[] { new { text = finalPrompt } } } }
                    };

                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        using (var doc = JsonDocument.Parse(jsonString))
                        {
                            try
                            {
                                aiResponse = doc.RootElement
                                    .GetProperty("candidates")[0]
                                    .GetProperty("content")
                                    .GetProperty("parts")[0]
                                    .GetProperty("text").GetString();
                            }
                            catch
                            {
                                aiResponse = "Dạ xin lỗi, câu hỏi này nằm ngoài khả năng xử lý của mình. Bạn vui lòng gọi hotline nhé!";
                            }
                        }
                    }
                    else
                    {
                        aiResponse = "Hệ thống AI đang bảo trì, bạn đợi xíu nhé!";
                    }
                }
            }
            catch (Exception ex)
            {
                aiResponse = "Hệ thống đang bận. Bạn vui lòng đợi một lát nhé.";
                Console.WriteLine("Lỗi: " + ex.Message);
            }

            return Ok(new { reply = aiResponse });
        }
    }
}