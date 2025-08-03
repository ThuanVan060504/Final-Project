using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Adminboot.Admin.Controllers.TinNhan
{
    [Area("Admin")]
    public class TinNhanController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Adminboot/Admin/Views/TinNhan/Index.cshtml");
        }
    }
}
