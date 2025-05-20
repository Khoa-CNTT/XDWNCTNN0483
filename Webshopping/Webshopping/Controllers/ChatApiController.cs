using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // Đọc config
using ChatBotGemini.Models.Gemini; // Models Gemini request/response
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Cho ToListAsync
using System;
using Webshopping.Repository;
using System.Text.RegularExpressions;

namespace ChatBotGemini.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatApiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly DataContext _dbContext; // EF Core DbContext
        private readonly string _geminiApiKey;
        private readonly string _geminiApiUrl;

        public ChatApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, DataContext dbContext)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _dbContext = dbContext;

            _geminiApiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(_geminiApiKey))
            {
                throw new InvalidOperationException("Gemini API Key chưa được cấu hình trong appsettings.");
            }
            _geminiApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_geminiApiKey}";
        }

        private List<Content> GetChatHistory()
        {
            var historyJson = HttpContext.Session.GetString("ChatHistory");
            if (string.IsNullOrEmpty(historyJson))
            {
                return new List<Content>();
            }
            try
            {
                return JsonSerializer.Deserialize<List<Content>>(historyJson) ?? new List<Content>();
            }
            catch (JsonException)
            {
                return new List<Content>();
            }
        }

        private void SaveChatHistory(List<Content> history)
        {
            var historyJson = JsonSerializer.Serialize(history);
            HttpContext.Session.SetString("ChatHistory", historyJson);
        }

        [HttpPost]
        public async Task<IActionResult> PostMessage([FromBody] ChatInput input)
        {
            if (string.IsNullOrWhiteSpace(input?.Message))
                return BadRequest(new { error = "Tin nhắn không được để trống." });

            try
            {
                // 1. Lấy dữ liệu từ DB (bạn có thể giới hạn số lượng bản ghi nếu DB quá lớn)
                var brands = await _dbContext.Brands.ToListAsync();
                var products = await _dbContext.Products.ToListAsync();
                var ratings = await _dbContext.Ratings.ToListAsync();
                var categories = await _dbContext.Categories.ToListAsync();
                var orders = await _dbContext.Orders.ToListAsync();
                var orderDetails = await _dbContext.OrderDetails.ToListAsync();
                var shippings = await _dbContext.Shippings.ToListAsync();

                var dbData = new
                {
                    Brands = brands,
                    Products = products,
                    Ratings = ratings,
                    Categories = categories,
                    Orders = orders,
                    OrderDetails = orderDetails,
                    Shippings = shippings
                };

                // 2. Chuyển dữ liệu DB thành JSON để đưa vào prompt
                string dbDataJson = JsonSerializer.Serialize(dbData, new JsonSerializerOptions { WriteIndented = true });

                // 3. Lấy lịch sử chat
                var chatHistory = GetChatHistory();

                // 4. Thêm prompt "system" có dữ liệu DB JSON (chỉ thêm lần đầu tiên)
                if (!chatHistory.Any(c => c.Role == "model"))
                {
                    chatHistory.Insert(0, new Content
                    {
                        Role = "model",
                        Parts = new List<Part>
                        {
                            new Part
                            {
                                Text = @$"
                                Bạn là một chuyên gia tư vấn nước hoa cao cấp, am hiểu sâu sắc về các loại nước hoa, thành phần, thương hiệu và cách chọn nước hoa phù hợp với từng người.  Bạn luôn trả lời một cách lịch sự, chuyên nghiệp, và đưa ra các gợi ý chi tiết, dễ hiểu.  Bạn ưu tiên giúp khách hàng tìm được loại nước hoa ưng ý nhất dựa trên sở thích, phong cách và ngân sách của họ. Bạn không đưa ra những thông tin sai lệch và luôn trung thực về những sản phẩm không có sẵn.
                                Thực tế trong cửa hàng dưới đây (định dạng JSON): {dbDataJson}

                                Bạn sẽ dùng dữ liệu này để trả lời các câu hỏi của khách hàng một cách chính xác, trung thực và chi tiết.
                                "
                            }
                        }
                    });
                }

                // 5. Thêm câu hỏi mới của user
                chatHistory.Add(new Content
                {
                    Role = "user",
                    Parts = new List<Part> { new Part { Text = input.Message } }
                });

                // 6. Tạo request payload gửi cho Gemini
                var requestPayload = new GeminiRequest
                {
                    Contents = chatHistory
                };

                var jsonPayload = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                var httpClient = _httpClientFactory.CreateClient();
                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // 7. Gửi POST đến Gemini API
                var response = await httpClient.PostAsync(_geminiApiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseBody);

                    var botMessageContent = geminiResponse?.Candidates?.FirstOrDefault()?.Content;
                    if (botMessageContent != null && botMessageContent.Parts != null && botMessageContent.Parts.Any())
                    {
                        var botReplyText = botMessageContent.Parts.First().Text;

                        var foramtReplyText = CleanPerfumeParagraph(botReplyText, asHtml: true);

                        // 8. Thêm câu trả lời của model vào lịch sử
                        botMessageContent.Role = "model";
                        botMessageContent.Parts.First().Text = botReplyText;   // thay text thô bằng text đã format
                        chatHistory.Add(botMessageContent);

                        // 9. Lưu lịch sử chat
                        SaveChatHistory(chatHistory);

                        // 10. Trả về cả câu trả lời và dữ liệu JSON (bạn có thể bỏ phần dbData nếu không cần)
                        return Ok(new
                        {
                            reply = botReplyText,
                            data = dbData
                        });
                    }
                    else
                    {
                        SaveChatHistory(chatHistory);
                        return Ok(new { reply = "Xin lỗi, tôi không thể tạo phản hồi lúc này." });
                    }
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Lỗi từ Gemini API: {response.StatusCode} - {errorBody}");
                    return StatusCode((int)response.StatusCode, new { error = "Có lỗi xảy ra khi giao tiếp với AI.", details = errorBody });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi nội bộ server: {ex.Message}");
                return StatusCode(500, new { error = "Lỗi máy chủ nội bộ." });
            }
        }

        public string CleanPerfumeParagraph(string input, bool asHtml = true)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            // 1) Bỏ dấu *
            string t = input.Replace("*", "");

            // 2) Xuống dòng trước 1. 2. 3. (sau space hoặc :)
            string pattern = @"(?:(?<=^)|(?<=[\s:]))(\d+)\.(?=\s*[A-Za-zÀ-ỹ])";
            t = Regex.Replace(t, pattern, m => "\n" + m.Value, RegexOptions.Multiline);

            // 3) Khoảng trắng sau dấu chấm
            t = Regex.Replace(t, @"\.(\S)", ". $1");

            // 4) Gom khoảng trắng
            t = Regex.Replace(t, @"[ \t]+", " ").Trim();

            // 5) Xuất
            return asHtml ? t.Replace("\n", "<br/>") : "";
        }
    }

    // Lớp nhận input message
    public class ChatInput
    {
        public string Message { get; set; }
    }
}
