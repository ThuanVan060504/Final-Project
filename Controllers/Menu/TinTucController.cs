using Microsoft.AspNetCore.Mvc;
using Final_Project.Models;
using Final_Project.Services;

namespace Final_Project.Controllers
{
    public class TinTucController : Controller
    {
        // Trang chính hiển thị tin tức từ nhiều nguồn
        public IActionResult Index()
        {
            var tinTucList = NewsService.GetNewsFromAll();
            return View(tinTucList);
        }

        // Mở chi tiết tin bằng link gốc (redirect đến bài viết)
        public IActionResult ChiTiet(string url)
        {
            if (string.IsNullOrEmpty(url))
                return RedirectToAction("Index");

            return Redirect(url);
        }
    }
}
