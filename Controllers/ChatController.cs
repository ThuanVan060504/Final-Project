using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Final_Project.Controllers.Menu
{
    public class ChatController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        // Constructor
        public ChatController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest model)
        {
            var message = model?.Message;
            var client = _httpClientFactory.CreateClient();

            var requestData = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
            new { role = "user", content = message }
        }
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
            var apiKey = _configuration["OpenRouter:ApiKey"];
            // Sử dụng key OpenRouter
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}"); client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5001"); // bắt buộc (điền domain project hoặc localhost)
            client.DefaultRequestHeaders.Add("X-Title", "Chatbox Demo MVC");

            var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", requestContent);
            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine("💬 OpenRouter trả về:\n" + responseString);

            try
            {
                var jsonDoc = JsonDocument.Parse(responseString);

                if (!jsonDoc.RootElement.TryGetProperty("choices", out var choices))
                {
                    return Json(new { reply = "❗ OpenRouter không trả về choices:\n" + responseString });
                }


                var reply = choices[0].GetProperty("message").GetProperty("content").GetString();
                return Json(new { reply });
            }
            catch (Exception ex)
            {
                return Json(new { reply = "❗ Lỗi xử lý dữ liệu: " + ex.Message });
            }
        }


        public class ChatRequest
        {
            public string Message { get; set; }
        }
    }
}