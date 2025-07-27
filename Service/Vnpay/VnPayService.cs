using Final_Project.Libraries;
using Final_Project.Models.Vnpay;
using System.Net;

namespace Final_Project.Service.Vnpay
{
    public class VnPayService : IVnPayService 
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneId = _configuration["TimeZoneId"];
            if (string.IsNullOrEmpty(timeZoneId))
            {
                throw new ArgumentException("TimeZoneId configuration is missing or empty.");
            }

            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["Vnpay:PaymentBackReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"] ?? string.Empty);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"] ?? string.Empty);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"] ?? string.Empty);
            pay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString());


            pay.AddRequestData("vnp_OrderInfo", model.OrderDescription);


            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"] ?? string.Empty);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"] ?? string.Empty);
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack ?? string.Empty);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl = pay.CreateRequestUrl(
                _configuration["Vnpay:BaseUrl"],
                _configuration["Vnpay:HashSecret"]
            );
            Console.WriteLine("✅ PAYMENT URL: " + paymentUrl); // log ra check
            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var hashSecret = _configuration["Vnpay:HashSecret"];
            
            if (string.IsNullOrEmpty(hashSecret))
            {
                throw new ArgumentException("HashSecret configuration is missing or empty.");
            }

            var response = pay.GetFullResponseData(collections, hashSecret);

            return response;
        }

    }
}
