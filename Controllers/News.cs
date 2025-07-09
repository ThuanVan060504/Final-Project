using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
