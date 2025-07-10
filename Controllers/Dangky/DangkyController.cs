using Microsoft.AspNetCore.Mvc;
using System.Linq;

public class DangkyController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View("~/Dangky/DangKy.cshtml");
    }

    [HttpPost]
    public IActionResult Index(string hoTen, string email, string sdt, string newUsername, string newPassword, string confirmPassword)
    {
        // Kiểm tra thông tin bắt buộc
        if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(sdt) || string.IsNullOrWhiteSpace(newUsername) ||
            string.IsNullOrWhiteSpace(newPassword))
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
            return View("~/Dangky/DangKy.cshtml");
        }

        // Kiểm tra định dạng email đơn giản
        if (!email.Contains("@") || !email.Contains("."))
        {
            ViewBag.Error = "Email không hợp lệ.";
            return View("~/Dangky/DangKy.cshtml");
        }

        // Kiểm tra số điện thoại
        if (sdt.Length < 10 || sdt.Length > 11 || !sdt.All(char.IsDigit))
        {
            ViewBag.Error = "Số điện thoại không hợp lệ. Vui lòng nhập 10–11 số.";
            return View("~/Dangky/DangKy.cshtml");
        }

        // Kiểm tra xác nhận mật khẩu
        if (newPassword != confirmPassword)
        {
            ViewBag.Error = "Mật khẩu xác nhận không khớp.";
            return View("~/Dangky/DangKy.cshtml");
        }

        // Tạm thời mô phỏng đăng ký thành công
        TempData["Success"] = $"Đăng ký thành công! Chào {hoTen}. Vui lòng đăng nhập.";
        return RedirectToAction("Index", "Dangnhap");
    }
}
