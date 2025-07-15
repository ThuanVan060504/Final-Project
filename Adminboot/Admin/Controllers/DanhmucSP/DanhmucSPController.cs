using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Adminboot.Admin.Controllers.DanhmucSP
{

    [Area("Admin")]
    public class DanhmucSPController : Controller
    {
        public IActionResult Index()
        {

            return View("~/Adminboot/Admin/Views/DanhmucSP/Index.cshtml");
        }
    }
}
    
