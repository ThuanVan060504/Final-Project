using Microsoft.AspNetCore.Mvc;

public class DangnhapController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View("/Dangnhap/Dangnhap.cshtml");
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
        return View("~/Dangnhap/Dangnhap.cshtml");
    }

    [HttpGet]
    public IActionResult DangKy()
    {
        return View("~/Dangky/DangKy.cshtml");
    }

    [HttpPost]
    public IActionResult DangKy(string newUsername, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(newUsername) || string.IsNullOrWhiteSpace(newPassword))
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
            return View("~/Dangky/DangKy.cshtml");
        }

        if (newPassword != confirmPassword)
        {
            ViewBag.Error = "Mật khẩu xác nhận không khớp.";
            return View("~/Dangky/DangKy.cshtml");
        }

        // Tạm thời mô phỏng đăng ký thành công
        TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
        return RedirectToAction("Index");
    }
}
