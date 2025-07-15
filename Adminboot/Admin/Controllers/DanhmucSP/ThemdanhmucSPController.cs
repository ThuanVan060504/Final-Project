using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Adminboot.Admin.Controllers.DanhmucSP
{
    [Area("Admin")]
    public class ThemdanhmucSPController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Adminboot/Admin/Views/ThemDanhmucSP/Index.cshtml");

        }
    }
}

