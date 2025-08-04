using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Controllers.Menu
{
    public class ThongTinController : Controller
    {
        // /Menu/ThongTin/DieuKhoan
        public IActionResult DieuKhoan()
        {
            return View();
        }

        // /Menu/ThongTin/BaoMat
        public IActionResult BaoMat()
        {
            return View();
        }
    }
}
