using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Adminboot.Admin.Controllers.Sanpham
{
    [Area("Admin")]
    public class ThemSanphamController : Controller

    {
        public IActionResult Index()
        {
            return View("~/Adminboot/Admin/Views/ThemSanpham/Index.cshtml");

        }
    }
}
