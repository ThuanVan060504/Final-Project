using Azure.Core;
using Final_Project.Models.Shop;
using Final_Project.Models.VnPay;
using Final_Project.Service.VnPay;
using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Controllers.VnPay
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

        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }

        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.Success)
            {
                int maDonHang = int.Parse(response.OrderId);
                var donHang = _context.DonHangs.FirstOrDefault(x => x.MaDonHang == maDonHang);
                if (donHang != null)
                {
                    donHang.TrangThaiThanhToan = "DaThanhToan";
                    _context.SaveChanges();
                }
                TempData["Success"] = "🎉 Thanh toán VNPAY thành công!";
            }
            else
            {
                TempData["Error"] = "❌ Thanh toán VNPAY thất bại hoặc bị hủy.";
            }

            return RedirectToAction("Index", "GioHang");
        }
    }

}
