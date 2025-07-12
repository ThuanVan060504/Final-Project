using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Adminboot.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Adminboot/Admin/Views/Home/Index.cshtml");
        }


    }
}
