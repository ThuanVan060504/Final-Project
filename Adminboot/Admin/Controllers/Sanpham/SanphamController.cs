using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Adminboot.Admin.Controllers.Sanpham
{
    [Area("Admin")]
    public class SanphamController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Adminboot/Admin/Views/Sanpham/Index.cshtml");
        }
    }
}



