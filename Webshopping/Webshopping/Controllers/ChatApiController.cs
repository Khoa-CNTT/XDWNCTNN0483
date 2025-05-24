using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using ChatBotGemini.Models.Gemini;
using Webshopping.Repository;
using Webshopping.Models;

namespace Webshopping.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public partial class ChatAPIController : ControllerBase
    {

        private readonly string _geminiApiUrl;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly DataContext _dbContext; // EF Core DbContext
        private readonly string _geminiApiKey;

        public ChatAPIController(IHttpClientFactory httpClientFactory, IConfiguration configuration, DataContext dbContext)
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

            string message = input.Message.Trim().ToLower();

            // === Xử lý CRUD cơ bản ===
            var userMessage = input.Message;
            // CREATE
            if (userMessage.Contains("thêm danh mục"))
            {
                if (!User.IsInRole("Admin"))
                {
                    // Kiểm tra quyền truy cập
                    var categoryName = ExtractName(userMessage, "thêm danh mục");

                    if (string.IsNullOrWhiteSpace(categoryName))
                        return Ok(new { reply = "⚠️ Vui lòng nhập tên danh mục." });

                    // Tạo mô tả và slug giả định từ tên
                    var description = $"Danh mục {categoryName}";
                    var slug = categoryName.ToLower().Replace(" ", "-"); // bạn có thể dùng helper Slugify nếu có
                    var status = 1;

                    var newCategory = new CategoryModel
                    {
                        Name = categoryName,
                        Description = description,
                        Slug = slug,
                        Status = status
                    };

                    try
                    {
                        _dbContext.Categories.Add(newCategory);
                        await _dbContext.SaveChangesAsync();
                        return Ok(new { reply = $"✅ Đã thêm danh mục: {categoryName}" });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new { reply = $"❌ Lỗi khi thêm danh mục: {ex.Message}" });
                    }
                }


                if (userMessage.Contains("thêm thương hiệu"))
                {
                    var brandName = ExtractName(userMessage, "thêm thương hiệu");

                    if (string.IsNullOrWhiteSpace(brandName))
                        return Ok(new { reply = "⚠️ Vui lòng nhập tên thương hiệu." });

                    // Tạo mô tả và slug giả định từ tên
                    var description = $"Thương hiệu {brandName}";
                    var slug = brandName.ToLower().Replace(" ", "-"); // bạn có thể dùng helper Slugify nếu có
                    var status = 1;

                    var newBrand = new BrandModel
                    {
                        Name = brandName,
                        Description = description,
                        Slug = slug,
                        Status = status
                    };

                    try
                    {
                        _dbContext.Brands.Add(newBrand);
                        await _dbContext.SaveChangesAsync();
                        return Ok(new { reply = $"✅ Đã thêm thương hiệu: {brandName}" });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new { reply = $"❌ Lỗi khi thêm thương hiệu: {ex.Message}" });
                    }
                     }
            else
            {
                return Ok(new { reply = "⚠️ chỉ nhân viên mới được thêm mới" });
            }
                }
           

            // READ
                if (userMessage.Contains("xem danh mục"))
                {
                    var categories = _dbContext.Categories.Select(c => c.Name).ToList();
                    return Ok(new { reply = "📋 Danh mục hiện có: " + string.Join(", ", categories) });
                }

            if (userMessage.Contains("xem thương hiệu"))
            {
                var brands = _dbContext.Brands.Select(b => b.Name).ToList();
                return Ok(new { reply = "📋 Thương hiệu hiện có: " + string.Join(", ", brands) });
            }


            // UPDATE
            if (userMessage.Contains("sửa danh mục"))
            {
                var (oldName, newName) = ExtractUpdateNames(userMessage, "sửa danh mục");
                var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == oldName);
                if (category == null) return Ok(new { reply = $"❌ Không tìm thấy danh mục {oldName}" });
                category.Name = newName;
                await _dbContext.SaveChangesAsync();
                return Ok(new { reply = $"✅ Đã cập nhật danh mục {oldName} thành {newName}" });
            }

            if (userMessage.Contains("sửa thương hiệu"))
            {
                var (oldName, newName) = ExtractUpdateNames(userMessage, "sửa thương hiệu");
                var brand = await _dbContext.Brands.FirstOrDefaultAsync(b => b.Name == oldName);
                if (brand == null) return Ok(new { reply = $"❌ Không tìm thấy thương hiệu {oldName}" });
                brand.Name = newName;
                await _dbContext.SaveChangesAsync();
                return Ok(new { reply = $"✅ Đã cập nhật thương hiệu {oldName} thành {newName}" });
            }

            // DELETE
            if (userMessage.Contains("xóa danh mục"))
            {
                var name = ExtractName(userMessage, "xóa danh mục");
                var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == name);
                if (category == null) return Ok(new { reply = $"❌ Không tìm thấy danh mục {name}" });
                _dbContext.Categories.Remove(category);
                await _dbContext.SaveChangesAsync();
                return Ok(new { reply = $"🗑️ Đã xóa danh mục: {name}" });
            }

            if (userMessage.Contains("xóa thương hiệu"))
            {
                var name = ExtractName(userMessage, "xóa thương hiệu");
                if (string.IsNullOrWhiteSpace(name))
                    return Ok(new { reply = "⚠️ Bạn vui lòng cung cấp tên thương hiệu cần xoá." });

                var brand = await _dbContext.Brands.FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower());
                if (brand == null)
                    return Ok(new { reply = $"❌ Không tìm thấy thương hiệu {name}" });

                _dbContext.Brands.Remove(brand);
                await _dbContext.SaveChangesAsync();
                return Ok(new { reply = $"🗑️ Đã xóa thương hiệu: {name}" });
            }

            // Nếu không phải CRUD → chuyển qua Gemini xử lý
            Console.WriteLine("⚠️ Không khớp CRUD, chuyển sang Gemini...");

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
        private string ExtractName(string userMessage, string keyword)
        {
            var name = userMessage.Substring(userMessage.ToLower().IndexOf(keyword.ToLower()) + keyword.Length).Trim();
            return name;
        }

        private (string, string) ExtractUpdateNames(string userMessage, string keyword)
        {
            var parts = userMessage.Split(keyword);
            if (parts.Length < 2) return ("", "");
            var names = parts[1].Split("thành");
            return names.Length == 2 ? (names[0].Trim(), names[1].Trim()) : ("", "");
        }
    }

    // Lớp nhận input message
    public class ChatInput
    {
        public string Message { get; set; }
    }

    // End of ChatAPIController

}