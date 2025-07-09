using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Controllers
{
    public class ProjectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
