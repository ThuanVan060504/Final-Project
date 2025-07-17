using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Controllers.CaNhan
{
    public class OrderController : Controller
    {
        public IActionResult History()
        {
            // Tạm thời trả về view trống. Sau này có thể truyền danh sách đơn hàng.
            return View();
        }
    }
}
