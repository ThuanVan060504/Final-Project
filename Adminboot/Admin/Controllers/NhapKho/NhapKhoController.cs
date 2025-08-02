using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Adminboot.Admin.Controllers.NhapKho
{
   
     [Area("Admin")]
      public class NhapKhoController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Adminboot/Admin/Views/NhapKhoSP/Index.cshtml");
        }


    }
}
