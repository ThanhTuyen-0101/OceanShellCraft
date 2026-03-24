using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace OceanShellCraft.Controllers
{
    public class ChatController : Controller
    {
        public class ChatRequest
        {
            public string Message { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> AskAI([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request?.Message))
            {
                return Json(new { reply = "Bạn chưa nhập tin nhắn." });
            }

            string userMessage = request.Message;
            string aiResponse = "";

            try
            {
                // ==========================================
                // THÔNG TIN NGỮ CẢNH CỦA CỬA HÀNG (SYSTEM PROMPT)
                // ==========================================
                string thongTinCuaHang = @"
Bạn là nhân viên tư vấn nhiệt tình, thân thiện của cửa hàng đồ mỹ nghệ OceanShellCraft.
Dưới đây là thông tin về cửa hàng để bạn trả lời khách:
- Cửa hàng chuyên bán các sản phẩm thủ công mỹ nghệ làm từ vỏ ốc, sao biển, ngọc trai tự nhiên.
- Các sản phẩm tiêu biểu: Vỏ ốc trang trí, vòng ngọc trai, chuông gió vỏ ốc, bông tai vỏ sò, tượng vỏ sò.
- Địa chỉ cửa hàng: 123 Đường Biển, Thành phố Nha Trang, Việt Nam.
- Số điện thoại liên hệ: 0909 123 456.
- Chính sách giao hàng: Miễn phí giao hàng toàn quốc cho đơn hàng từ 500.000 VNĐ trở lên.
- Chính sách bảo hành: Đổi trả miễn phí trong 7 ngày nếu sản phẩm bị lỗi do vận chuyển.
- Lịch làm việc: 8h00 sáng đến 10h00 tối các ngày trong tuần.

Nguyên tắc trả lời:
1. Luôn xưng hô lịch sự (dạ, vâng, ạ, anh/chị/bạn).
2. Chỉ trả lời dựa trên thông tin trên. Nếu khách hỏi giá cụ thể hoặc thông tin ngoài lề không có ở trên, hãy nói: 'Dạ, chi tiết này anh/chị vui lòng liên hệ Chat trực tiếp với nhân viên hoặc gọi vào hotline 0909 123 456 để được hỗ trợ chính xác nhất ạ'.
3. Trả lời ngắn gọn, đi thẳng vào vấn đề.
";

                // API Key của bạn
                string apiKey = "AIzaSyCY8KIHeLA_PpP_VUNQap9-0c6CNEl2LUg";

                using (var httpClient = new HttpClient())
                {
                    // ĐÃ SỬA: Đổi lại model name thành bản ổn định nhất là gemini-pro
                    var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";

                    // Kết hợp thông tin cửa hàng + Cấu trúc yêu cầu + Câu hỏi của khách
                    string finalPrompt = thongTinCuaHang + "\n\nCâu hỏi của khách hàng: " + userMessage;

                    var payload = new
                    {
                        contents = new[]
                        {
                            new { parts = new[] { new { text = finalPrompt } } }
                        }
                    };

                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        using (var doc = JsonDocument.Parse(jsonString))
                        {
                            // Trích xuất text từ JSON trả về của Gemini
                            aiResponse = doc.RootElement
                                .GetProperty("candidates")[0]
                                .GetProperty("content")
                                .GetProperty("parts")[0]
                                .GetProperty("text").GetString();
                        }
                    }
                    else
                    {
                        // Giữ nguyên việc đọc chi tiết lỗi để kiểm tra API Key
                        string errorDetails = await response.Content.ReadAsStringAsync();
                        aiResponse = $"Lỗi từ Google (Mã {response.StatusCode}): {errorDetails}";
                    }
                }
            }
            catch (Exception ex)
            {
                aiResponse = "Đã xảy ra lỗi kết nối C#: " + ex.Message;
            }

            // Trả kết quả về cho JavaScript
            return Json(new { reply = aiResponse });
        }
    }
}