using Final_Project.Models;
using Final_Project.Models.Helpers;
using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
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

        // GET: /Auth/Login
        [HttpGet("Auth/Login")]
        public IActionResult Login()
        {
            return View("Login");
        }

        // POST: /Auth/Login
        [HttpPost("Auth/Login")]
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

            // Tạo claims và sign-in bằng "MyCookie"
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.HoTen ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.VaiTro ?? "Customer")
            };

            var identity = new ClaimsIdentity(claims, "MyCookie");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("MyCookie", principal);

            // Lưu Session (dùng helper SetObjectAsJson của bạn)
            HttpContext.Session.SetObjectAsJson("TaiKhoan", user);
            HttpContext.Session.SetString("UserEmail", user.Email ?? "");
            HttpContext.Session.SetString("UserRole", user.VaiTro ?? "Customer");
            HttpContext.Session.SetInt32("MaTK", user.MaTK);

            // Tính tổng giỏ hàng an toàn (tránh NULL)
            int tongSoLuong = _context.GioHangs
                .Where(g => g.MaTK == user.MaTK)
                .Sum(g => (int?)g.SoLuong ?? 0);

            HttpContext.Session.SetInt32("SoLuongGioHang", tongSoLuong);

            // Debug
            var debugMaTK = HttpContext.Session.GetInt32("MaTK");
            Console.WriteLine("✅ Session MaTK sau khi set: " + debugMaTK);
            TempData["TestSession"] = "MaTK: " + debugMaTK;

            // Điều hướng theo vai trò
            if ((user.VaiTro ?? "").ToLower() == "admin")
            {
                var url = $"{Request.Scheme}://{Request.Host}/Admin/Home/";
                return Redirect(url);
            }

            return RedirectToAction("Index", "Home");
        }

        // Bắt đầu Google login
        [HttpGet("Auth/LoginGoogle")]
        public IActionResult LoginGoogle()
        {
            // RedirectUri sẽ được middleware xử lý: Google sẽ redirect tới /signin-google, sau đó middleware redirect tới RedirectUri
            var redirectUrl = Url.Action("GoogleResponse", "Auth", null, Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // Callback sau khi Google xác thực
        [HttpGet("Auth/GoogleResponse")]
        public async Task<IActionResult> GoogleResponse()
        {
            // Lấy external principal từ Google handler
            var authResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authResult.Succeeded || authResult.Principal == null)
            {
                // Nếu thất bại -> trở về trang login
                return RedirectToAction("Login");
            }

            // Lấy thông tin từ claims do Google trả về
            var email = authResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authResult.Principal.FindFirst(ClaimTypes.Name)?.Value
                       ?? authResult.Principal.FindFirst("name")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                // Nếu Google không trả email thì không tiếp tục
                TempData["Error"] = "Không lấy được email từ Google.";
                return RedirectToAction("Login");
            }

            // Kiểm tra user đã tồn tại chưa, nếu chưa -> tạo user mới (Customer)
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                user = new TaiKhoan
                {
                    HoTen = name ?? email,
                    Email = email,
                    MatKhau = Guid.NewGuid().ToString(), // password random
                    VaiTro = "Customer",
                    NgayTao = DateTime.Now
                };
                _context.TaiKhoans.Add(user);
                _context.SaveChanges();
            }

            // Tạo local cookie (dùng cùng "MyCookie")
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.HoTen ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.VaiTro ?? "Customer")
            };
            var identity = new ClaimsIdentity(claims, "MyCookie");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookie", principal);

            // Lưu session tương tự login thường
            HttpContext.Session.SetInt32("MaTK", user.MaTK);
            HttpContext.Session.SetString("UserEmail", user.Email ?? "");
            HttpContext.Session.SetString("UserRole", user.VaiTro ?? "Customer");

            // Tính giỏ hàng an toàn (có thể không có bản ghi)
            int tongSoLuong = _context.GioHangs
                .Where(g => g.MaTK == user.MaTK)
                .Sum(g => (int?)g.SoLuong ?? 0);
            HttpContext.Session.SetInt32("SoLuongGioHang", tongSoLuong);



            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/Logout
        [HttpGet("Auth/Logout")]
        public async Task<IActionResult> Logout()
        {
            // Sign out local cookie
            await HttpContext.SignOutAsync("MyCookie");
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        // Đăng ký
        [HttpGet("/Dangky")]
        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost("/Dangky")]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Register", model);

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

        // Quên mật khẩu
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

            TempData["Message"] = "Liên kết khôi phục mật khẩu đã được gửi (giả lập).";
            return RedirectToAction("ForgotPassword");
        }
    }
}
