using Final_Project.Models.PayPal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Linq;

namespace Final_Project.Services.PayPal
{
    public class PayPalService : IPayPalService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public PayPalService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
        }

        // ✅ HÀM MỚI: Tách logic lấy Token ra để dùng chung
        private async Task<string> GetAccessTokenAsync()
        {
            string clientId = _config["PayPal:ClientId"];
            string secret = _config["PayPal:ClientSecret"];
            string apiUrl = _config["PayPal:ApiUrl"];

            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{secret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string err = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi lấy Access Token: {response.StatusCode}\n{err}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString();
        }

        public async Task<string> CreatePaymentUrlAsync(PayPalPaymentModel model, HttpContext context)
        {
            string apiUrl = _config["PayPal:ApiUrl"];
            string token = await GetAccessTokenAsync(); // Gọi hàm lấy token

            var returnUrl = model.ReturnUrl ?? _config["PayPal:ReturnUrl"] ??
                    $"{context.Request.Scheme}://{context.Request.Host}/ThanhToan/PayPalSuccess";

            var cancelUrl = model.CancelUrl ?? _config["PayPal:CancelUrl"] ??
                            $"{context.Request.Scheme}://{context.Request.Host}/ThanhToan/PayPalCancel";

            var paymentData = new
            {
                intent = "sale",
                redirect_urls = new { return_url = returnUrl, cancel_url = cancelUrl },
                payer = new { payment_method = "paypal" },
                transactions = new[]
                {
                    new
                    {
                        amount = new
                        {
                            total = model.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                            currency = "USD"
                        },
                        description = model.Description ?? "Thanh toán đơn hàng"
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(paymentData), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync($"{apiUrl}/v1/payments/payment", content);
            var resultJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Lỗi tạo Payment: {resultJson}");
            }

            using var doc = JsonDocument.Parse(resultJson);
            var links = doc.RootElement.GetProperty("links");
            foreach (var link in links.EnumerateArray())
            {
                if (link.GetProperty("rel").GetString() == "approval_url")
                {
                    return link.GetProperty("href").GetString();
                }
            }

            throw new Exception("Không tìm thấy approval_url");
        }

        // 🔥 QUAN TRỌNG: HÀM NÀY ĐÃ ĐƯỢC VIẾT LẠI ĐỂ THỰC THI THANH TOÁN
        public async Task<PayPalPaymentResponse> ExecutePaymentAsync(IQueryCollection query)
        {
            try
            {
                // 1. Kiểm tra tham số
                string paymentId = query["paymentId"];
                string payerId = query["PayerID"];

                if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(payerId))
                {
                    return new PayPalPaymentResponse { Success = false, Message = "Thiếu paymentId hoặc PayerID" };
                }

                string apiUrl = _config["PayPal:ApiUrl"];
                string token = await GetAccessTokenAsync(); // Lấy token mới

                // 2. Tạo body cho request Execute
                var executeData = new { payer_id = payerId };
                var content = new StringContent(JsonSerializer.Serialize(executeData), Encoding.UTF8, "application/json");

                // 3. Gửi request đến endpoint /execute
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync($"{apiUrl}/v1/payments/payment/{paymentId}/execute", content);
                var resultJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new PayPalPaymentResponse { Success = false, Message = $"PayPal API Error: {resultJson}" };
                }

                // 4. Kiểm tra trạng thái "state" trong phản hồi
                using var doc = JsonDocument.Parse(resultJson);
                var state = doc.RootElement.GetProperty("state").GetString();

                if (state == "approved")
                {
                    return new PayPalPaymentResponse { Success = true, PaymentId = paymentId, PayerId = payerId };
                }
                else
                {
                    return new PayPalPaymentResponse { Success = false, Message = $"Thanh toán không thành công. State: {state}" };
                }
            }
            catch (Exception ex)
            {
                return new PayPalPaymentResponse { Success = false, Message = ex.Message };
            }
        }
    }
}