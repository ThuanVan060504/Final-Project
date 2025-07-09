using Microsoft.AspNetCore.Mvc;

public class DangnhapController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View("~/Dangnhap/dangnhap.cshtml");


    }

    [HttpPost]
    public IActionResult Index(string username, string password)
    {
        if (username == "admin" && password == "123")
        {
            TempData["LoginSuccess"] = "Đăng nhập thành công!";
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
        return View("~/Dangnhap/dangnhap.cshtml");


    }
}
