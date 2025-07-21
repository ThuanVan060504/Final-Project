using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Controllers.Menu
{
    public class GioiThieuController : Controller
    {
        private readonly AppDbContext _context;

        public GioiThieuController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }
            return View();
        }
    }
}
