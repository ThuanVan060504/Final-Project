using Final_Project.Models.Momo;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;

public class MomoService
{
    private readonly IConfiguration _config;

    public MomoService(IConfiguration config)
    {
        _config = config;
    }

    public MomoOneTimeResponseModel CreatePayment(decimal amount, string orderId, string redirectUrl)
    {
        var partnerCode = _config["MomoAPI:PartnerCode"];
        var accessKey = _config["MomoAPI:AccessKey"];
        var secretKey = _config["MomoAPI:SecretKey"];
        var ipnUrl = _config["MomoAPI:NotifyUrl"];
        var requestType = _config["MomoAPI:RequestType"] ?? "captureWallet";

        var requestId = Guid.NewGuid().ToString();
        var orderInfo = "ThanhToanMOMO";
        var extraData = "";

        var rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";
        var signature = CreateSignature(rawHash, secretKey);

        var requestModel = new MomoOneTimeRequestModel
        {
            PartnerCode = partnerCode,
            AccessKey = accessKey,
            RequestId = requestId,
            OrderId = orderId,
            OrderInfo = orderInfo,
            RedirectUrl = redirectUrl,
            IpnUrl = ipnUrl,
            Amount = amount.ToString("0"), // không nên dùng ToString() trực tiếp với tiền
            RequestType = requestType,
            ExtraData = extraData,
            Signature = signature,
            Lang = "vi"
        };

        using var client = new HttpClient();
        var json = JsonConvert.SerializeObject(requestModel);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = client.PostAsync("https://test-payment.momo.vn/v2/gateway/api/create", content).Result;
        var result = response.Content.ReadAsStringAsync().Result;

        dynamic data = JsonConvert.DeserializeObject(result);
        return new MomoOneTimeResponseModel
        {
            PayUrl = data?.payUrl,
            Message = data?.message,
            ResultCode = data?.resultCode,
            OrderId = orderId,
            RequestId = requestId
        };
    }

    private string CreateSignature(string rawData, string secretKey)
    {
        using var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        byte[] hashMessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        return BitConverter.ToString(hashMessage).Replace("-", "").ToLower();
    }
}
