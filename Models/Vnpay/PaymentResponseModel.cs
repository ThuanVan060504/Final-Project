namespace Final_Project.Models.Vnpay
{
    public class PaymentResponseModel
    {
        public string OrderDescription { get; set; } = string.Empty; // Default value added
        public string TransactionId { get; set; } = string.Empty; // Default value added
        public string OrderId { get; set; } = string.Empty; // Default value added
        public string PaymentMethod { get; set; } = string.Empty; // Default value added
        public string PaymentId { get; set; } = string.Empty; // Default value added
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty; // Default value added
        public string VnPayResponseCode { get; set; } = string.Empty; // Default value added
    }
}
