namespace Final_Project.Models.PayPal
{
    public class PayPalPaymentModel
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string? ReturnUrl { get; internal set; }
        public string? CancelUrl { get; internal set; }
    }

    public class PayPalPaymentResponse
    {
        public bool Success { get; set; }
        public string PaymentId { get; set; }
        public string PayerId { get; set; }
    }
}
