using Final_Project.Models.Shop;
using Final_Project.Models.Vnpay;
using Final_Project.Service.Vnpay;
using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly AppDbContext _context;

        public PaymentController(IVnPayService vnPayService, AppDbContext context)
        {
            _vnPayService = vnPayService;
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // nếu form có AntiForgeryToken
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Thông tin thanh toán không hợp lệ.";
                return RedirectToAction("Index", "GioHang");
            }

            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            Console.WriteLine("💸 Amount = " + model.Amount);

            // Gợi ý 1: In ra Output Window
            Console.WriteLine("🔗 URL FULL: " + url);

            // Gợi ý 2: In ra trình duyệt (tạm, dễ thấy)
            TempData["VnpayUrl"] = url;

            return Redirect(url);

        }
    }
}
