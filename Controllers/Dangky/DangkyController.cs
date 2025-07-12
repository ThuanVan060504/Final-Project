using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

public class DangkyController : Controller
{
    private readonly AppDbContext _context;

    public DangkyController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View("~/Views/Auth/Register.cshtml");
    }

    [HttpPost]
    public IActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Auth/Register.cshtml", model);

        if (_context.TaiKhoans.Any(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email đã được sử dụng.");
            return View("~/Views/Auth/Register.cshtml", model);
        }

        var user = new TaiKhoan
        {
            HoTen = model.HoTen,
            Email = model.Email,
            SDT = model.SDT,
            DiaChi = model.DiaChi,
            MatKhau = model.MatKhau,
            VaiTro = "Customer",
            NgayTao = DateTime.Now
        };

        _context.TaiKhoans.Add(user);
        _context.SaveChanges();

        TempData["Success"] = $"Đăng ký thành công! Chào {model.HoTen}";
        return Redirect("/Dangnhap");
    }
}
