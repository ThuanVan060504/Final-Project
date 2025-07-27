using Final_Project.Models.Shop;
using Final_Project.Service.Vnpay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Add this if DataContext is part of Entity Framework Core

namespace Final_Project.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly AppDbContext _context;

        public CheckoutController(IVnPayService vnPayService, AppDbContext context) // Fixed parameter name
        {
            _vnPayService = vnPayService;
            _context = context; // No longer causes CS0103
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            return Json(response);
        }
    }
}
