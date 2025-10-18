using Final_Project.Models.PayPal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

        public async Task<string> CreatePaymentUrlAsync(PayPalPaymentModel model, HttpContext context)
        {
            string clientId = _config["PayPal:ClientId"];
            string secret = _config["PayPal:ClientSecret"];
            string apiUrl = _config["PayPal:ApiUrl"];

            // 🔐 1. Lấy access token
            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{secret}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

            var tokenResponse = await _httpClient.PostAsync(
                $"{apiUrl}/v1/oauth2/token",
                new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded")
            );

            if (!tokenResponse.IsSuccessStatusCode)
            {
                string err = await tokenResponse.Content.ReadAsStringAsync();
                throw new Exception($"❌ Lỗi khi lấy access token từ PayPal: {tokenResponse.StatusCode}\n{err}");
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            var token = JsonDocument.Parse(tokenJson).RootElement.GetProperty("access_token").GetString();

            // 🔗 2. Tạo payment request
            var returnUrl = _config["PayPal:ReturnUrl"] ??
                            $"{context.Request.Scheme}://{context.Request.Host}/ThanhToan/PayPalCallback";
            var cancelUrl = _config["PayPal:CancelUrl"] ??
                            $"{context.Request.Scheme}://{context.Request.Host}/ThanhToan/PayPalCancel";

            var paymentData = new
            {
                intent = "sale",
                redirect_urls = new
                {
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                },
                payer = new { payment_method = "paypal" },
                transactions = new[]
                {
                    new
                    {
                        amount = new
                            {
                                total = model.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture), // ✅ fix format
                                currency = "USD"
                            },

                        description = model.Description ?? "Thanh toán đơn hàng"
                    }
                }
            };

            var paymentContent = new StringContent(
                JsonSerializer.Serialize(paymentData),
                Encoding.UTF8,
                "application/json"
            );

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // 🔧 3. Gửi yêu cầu tạo payment
            var response = await _httpClient.PostAsync($"{apiUrl}/v1/payments/payment", paymentContent);
            var resultJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"❌ PayPal API lỗi khi tạo payment: {response.StatusCode}\n{resultJson}");
            }

            // ✅ 4. Lấy link thanh toán PayPal (approval_url)
            using var jsonDoc = JsonDocument.Parse(resultJson);
            var approvalUrl = jsonDoc.RootElement
                .GetProperty("links")
                .EnumerateArray()
                .FirstOrDefault(x => x.GetProperty("rel").GetString() == "approval_url")
                .GetProperty("href")
                .GetString();

            if (string.IsNullOrEmpty(approvalUrl))
                throw new Exception("❌ Không tìm thấy approval_url trong phản hồi PayPal.");

            return approvalUrl;
        }

        public async Task<PayPalPaymentResponse> ExecutePaymentAsync(IQueryCollection query)
        {
            return new PayPalPaymentResponse
            {
                Success = query.ContainsKey("paymentId") && query.ContainsKey("PayerID"),
                PaymentId = query["paymentId"],
                PayerId = query["PayerID"]
            };
        }
    }
}
