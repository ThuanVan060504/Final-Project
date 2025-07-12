using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Controllers.Menu
{
    public class NewsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
