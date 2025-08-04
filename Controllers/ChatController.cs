using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
