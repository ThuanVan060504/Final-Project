namespace Final_Project.Models.Momo
{
    public class MomoOneTimeRequestModel
    {
        public string PartnerCode { get; set; }
        public string AccessKey { get; set; }
        public string RequestId { get; set; }
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
        public string RedirectUrl { get; set; }
        public string IpnUrl { get; set; }
        public string Amount { get; set; }
        public string RequestType { get; set; } = "captureWallet";
        public string ExtraData { get; set; } = "";
        public string Lang { get; set; } = "vi";
        public string Signature { get; set; }
    }

}
