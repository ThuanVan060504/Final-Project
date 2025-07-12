using Final_Project.Models;
using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace Final_Project.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Gắn route chính thức: /Dangnhap
        [HttpGet("/Dangnhap")]
        public IActionResult Login()
        {
            return View();
        }

        // ✅ Route POST cũng là /Dangnhap
        [HttpPost("/Dangnhap")]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.TaiKhoans
                .FirstOrDefault(u => u.Email == model.Email && u.MatKhau == model.MatKhau);

            if (user == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View(model);
            }

            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.VaiTro);

            return RedirectToAction("Index", "Home");
        }

        // Đăng xuất
        [HttpGet("/Dangxuat")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/Dangnhap");
        }

        [HttpGet("/Dangky")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("/Dangky")]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kiểm tra Email đã tồn tại chưa
            if (_context.TaiKhoans.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng");
                return View(model);
            }

            var user = new TaiKhoan
            {
                HoTen = model.HoTen,
                Email = model.Email,
                SDT = model.SDT,
                DiaChi = model.DiaChi,
                MatKhau = model.MatKhau, // Nếu muốn: mã hóa ở đây
                VaiTro = "Customer",

                NgayTao = DateTime.Now
            };

            _context.TaiKhoans.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký thành công. Bạn có thể đăng nhập.";
            return Redirect("/Dangnhap");
        }


    }
}

