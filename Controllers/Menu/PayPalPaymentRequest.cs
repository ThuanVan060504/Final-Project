// Controllers/ThanhToanController.cs
using Final_Project.Models.PayPal;

namespace Final_Project.Controllers
{
    internal class PayPalPaymentRequest : PayPalPaymentModel
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
        public string Description { get; set; }
    }
}