using Final_Project.Models;
using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final_Project.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Dangnhap
        [HttpGet("/Dangnhap")]
        public IActionResult Login()
        {
            return View("Login");
        }

        // POST: /Dangnhap
        [HttpPost("/Dangnhap")]
        public async Task<IActionResult> Login(LoginViewModel model)

        {
            if (!ModelState.IsValid)
                return View("Login", model);

            var user = _context.TaiKhoans
                .FirstOrDefault(u => u.Email == model.Email && u.MatKhau == model.MatKhau);

            if (user == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View("Login", model);
            }


            var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.HoTen),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.VaiTro)
};

            var identity = new ClaimsIdentity(claims, "MyCookie");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(
    scheme: "MyCookie",  
    principal: principal
);



            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.VaiTro ?? "Customer");


            // ✅ Điều hướng theo vai trò
            if (user.VaiTro?.ToLower() == "admin")
            {
                var url = $"{Request.Scheme}://{Request.Host}/Admin/Home/Index";
                return Redirect(url);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        // GET: /Dangxuat
        [HttpGet("/Dangxuat")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookie"); 
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }




        // GET: /Dangky

        [HttpGet("/Dangky")]
        public IActionResult Register()
        {
            return View("Register");
        }

        // POST: /Dangky
        [HttpPost("/Dangky")]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Register", model);

            // Kiểm tra email đã tồn tại
            if (_context.TaiKhoans.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng");
                return View("Register", model);
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

            TempData["Success"] = "Đăng ký thành công. Bạn có thể đăng nhập.";
            return RedirectToAction("Login", "Auth");

        }

        // GET: /Quenmatkhau
        [HttpGet("/Quenmatkhau")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost("/Quenmatkhau")]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Không tìm thấy email này.");
                return View(model);
            }

            // 👇 Tạm thời chỉ hiển thị thông báo. Gửi email thực sẽ làm sau.
            TempData["Message"] = "Liên kết khôi phục mật khẩu đã được gửi (giả lập).";
            return RedirectToAction("ForgotPassword");
        }
    }
}