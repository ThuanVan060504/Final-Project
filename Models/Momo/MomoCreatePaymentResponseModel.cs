namespace Final_Project.Models.Momo
{
    public class MomoCreatePaymentResponseModel
    {
        public string PayUrl { get; set; }
        public string QrCodeUrl { get; set; }
        public string OrderId { get; set; }
        public int ErrorCode { get; set; }
        public string Message { get; set; }
    }



}
