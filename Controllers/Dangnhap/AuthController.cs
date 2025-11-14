using Final_Project.Models;
using Final_Project.Models.Helpers;
using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Final_Project.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook; 
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final_Project.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        private readonly IEmailService _emailService;

        public AuthController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
            var authResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authResult.Succeeded || authResult.Principal == null)
                return RedirectToAction("Login");

            var email = authResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authResult.Principal.FindFirst(ClaimTypes.Name)?.Value ?? email;

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Không lấy được email từ Google.";
                return RedirectToAction("Login");
            }

            bool isNewUser = false;
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                user = new TaiKhoan
                {
                    HoTen = name,
                    Email = email,
                    MatKhau = Guid.NewGuid().ToString(),
                    VaiTro = "Customer",
                    NgayTao = DateTime.Now
                };
                _context.TaiKhoans.Add(user);
                _context.SaveChanges();
                isNewUser = true;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.HoTen ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.VaiTro ?? "Customer")
            };
            await HttpContext.SignInAsync("MyCookie", new ClaimsPrincipal(new ClaimsIdentity(claims, "MyCookie")));

            HttpContext.Session.SetInt32("MaTK", user.MaTK);
            HttpContext.Session.SetString("UserEmail", user.Email ?? "");
            HttpContext.Session.SetString("UserRole", user.VaiTro ?? "Customer");

            // Gửi email khác nhau cho user mới và user cũ
            if (isNewUser)
            {
                string htmlContent = $@"
                            <table style='width:100%; max-width:600px; margin:auto; font-family:Arial, sans-serif; border:1px solid #ddd; border-radius:8px; overflow:hidden;'>
                              <tr style='background-color:#f4f4f4;'>
                                <td style='padding:20px; text-align:center;'>
                                  <img src='https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTrud2n7QkcJe76V36LbSU6lLsTCAHStSYNAw&s' alt='Shop Nội Thất G3TD' style='height:80px;'>
                                </td>
                              </tr>
                              <tr>
                                <td style='padding:20px;'>
                                  <h2 style='color:#333;'>Chào mừng {user.HoTen}!</h2>
                                  <p style='font-size:16px; color:#555;'>
                                    Cảm ơn bạn đã đăng ký tài khoản tại <strong>Shop Nội Thất G3TD</strong> thông qua Google.
                                  </p>
                                  <p style='font-size:16px; color:#555;'>
                                    Chúng tôi sẽ luôn đồng hành cùng bạn để mang đến những sản phẩm nội thất chất lượng nhất.
                                  </p>
                                  <div style='margin-top:20px; text-align:center;'>
                                    <a href='https://g3tdshop.com' style='background-color:#ff6600; color:#fff; padding:12px 20px; text-decoration:none; border-radius:5px; font-size:16px;'>
                                      Khám phá ngay
                                    </a>
                                  </div>
                                </td>
                              </tr>
                              <tr style='background-color:#f4f4f4;'>
                                <td style='padding:15px; text-align:center; font-size:14px; color:#888;'>
                                  <img src='https://hienlaptop.com/wp-content/uploads/2024/10/460630409_519358010845041_4976973150130837642_n.jpg' alt='Chủ shop' style='width:50px; height:50px; border-radius:50%; margin-bottom:8px;'><br>
                                  Chủ shop: G3TD  - <a href='mailto:shop.g3td@gmail.com'>shop.g3td@gmail.com</a>
                                </td>
                              </tr>
                            </table>";

                await _emailService.SendEmailAsync(
                    user.Email,
                    "🎉 Chào mừng bạn đến với Shop Nội Thất G3TD!",
                    htmlContent
                );
            }
            else
            {
                string htmlContentLogin = $@"
                            <table style='width:100%; max-width:600px; margin:auto; font-family:Arial, sans-serif; border:1px solid #ddd; border-radius:8px;'>
                              <tr style='background-color:#f4f4f4;'>
                                <td style='padding:20px; text-align:center;'>
                                  <img src='https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTrud2n7QkcJe76V36LbSU6lLsTCAHStSYNAw&s' alt='Shop Nội Thất G3TD' style='height:80px;'>
                                </td>
                              </tr>
                              <tr>
                                <td style='padding:20px;'>
                                  <h2>Xin chào {user.HoTen},</h2>
                                  <p>Bạn vừa đăng nhập vào tài khoản của mình tại <strong>Shop Nội Thất G3TD</strong>.</p>
                                  <p>Nếu không phải bạn, vui lòng liên hệ ngay với chúng tôi để được hỗ trợ.</p>
                                  <p style='color:#555; font-size:14px;'>Email hỗ trợ: <a href='mailto:shop.g3td@gmail.com'>shop.g3td@gmail.com</a></p>
                                </td>
                              </tr>
                              <tr style='background-color:#f4f4f4;'>
                                <td style='padding:15px; text-align:center; font-size:14px; color:#888;'>
                                  <img src='https://hienlaptop.com/wp-content/uploads/2024/10/460630409_519358010845041_4976973150130837642_n.jpg' alt='Chủ shop' style='width:50px; height:50px; border-radius:50%;'><br>
                                  Chủ shop: G3TD
                                </td>
                              </tr>
                            </table>";

                await _emailService.SendEmailAsync(
                    user.Email,
                    "🔔 Thông báo đăng nhập từ Shop Nội Thất G3TD",
                    htmlContentLogin
                );
            }


            return RedirectToAction("Index", "Home");
        }

        // ===============================================
        // == BẮT ĐẦU PHẦN THÊM MỚI CHO FACEBOOK ==
        // ===============================================

        // Bắt đầu Facebook login
        [HttpGet("Auth/LoginFacebook")]
        public IActionResult LoginFacebook()
        {
            var redirectUrl = Url.Action("FacebookResponse", "Auth", null, Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        // Callback sau khi Facebook xác thực
        [HttpGet("Auth/FacebookResponse")]
        public async Task<IActionResult> FacebookResponse()
        {
            var authResult = await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);

            if (!authResult.Succeeded || authResult.Principal == null)
            {
                TempData["Error"] = "Đăng nhập Facebook không thành công.";
                return RedirectToAction("Login");
            }

            var email = authResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authResult.Principal.FindFirst(ClaimTypes.Name)?.Value;

            // Facebook có thể không trả về email nếu người dùng không cấp quyền
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Không thể lấy email từ Facebook. Vui lòng đảm bảo bạn đã cấp quyền truy cập email cho ứng dụng.";
                return RedirectToAction("Login");
            }
            
            if (string.IsNullOrEmpty(name))
            {
                name = email; // Lấy email làm tên nếu không có tên
            }

            bool isNewUser = false;
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                user = new TaiKhoan
                {
                    HoTen = name,
                    Email = email,
                    MatKhau = Guid.NewGuid().ToString(), // Tạo mật khẩu ngẫu nhiên
                    VaiTro = "Customer",
                    NgayTao = DateTime.Now
                };
                _context.TaiKhoans.Add(user);
                _context.SaveChanges();
                isNewUser = true;
            }

            // Đăng nhập user (tương tự logic của GoogleResponse)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.HoTen ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.VaiTro ?? "Customer")
            };
            await HttpContext.SignInAsync("MyCookie", new ClaimsPrincipal(new ClaimsIdentity(claims, "MyCookie")));

            HttpContext.Session.SetInt32("MaTK", user.MaTK);
            HttpContext.Session.SetString("UserEmail", user.Email ?? "");
            HttpContext.Session.SetString("UserRole", user.VaiTro ?? "Customer");

            // Gửi email (tái sử dụng logic email của Google)
            if (isNewUser)
            {
                string htmlContent = $@"
                    <table style='width:100%; max-width:600px; margin:auto; font-family:Arial, sans-serif; border:1px solid #ddd; border-radius:8px; overflow:hidden;'>
                      <tr style='background-color:#f4f4f4;'>
                        <td style='padding:20px; text-align:center;'>
                          <img src='https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTrud2n7QkcJe76V36LbSU6lLsTCAHStSYNAw&s' alt='Shop Nội Thất G3TD' style='height:80px;'>
                        </td>
                      </tr>
                      <tr>
                        <td style='padding:20px;'>
                          <h2 style='color:#333;'>Chào mừng {user.HoTen}!</h2>
                          <p style='font-size:16px; color:#555;'>
                            Cảm ơn bạn đã đăng ký tài khoản tại <strong>Shop Nội Thất G3TD</strong> thông qua Facebook.
                          </p>
                          <p style='font-size:16px; color:#555;'>
                            Chúng tôi sẽ luôn đồng hành cùng bạn để mang đến những sản phẩm nội thất chất lượng nhất.
                          </p>
                          <div style='margin-top:20px; text-align:center;'>
                            <a href='https://g3tdshop.com' style='background-color:#ff6600; color:#fff; padding:12px 20px; text-decoration:none; border-radius:5px; font-size:16px;'>
                              Khám phá ngay
                            </a>
                          </div>
                        </td>
                      </tr>
                      <tr style='background-color:#f4f4f4;'>
                        <td style='padding:15px; text-align:center; font-size:14px; color:#888;'>
                          <img src='https://hienlaptop.com/wp-content/uploads/2024/10/460630409_519358010845041_4976973150130837642_n.jpg' alt='Chủ shop' style='width:50px; height:50px; border-radius:50%; margin-bottom:8px;'><br>
                          Chủ shop: G3TD  - <a href='mailto:shop.g3td@gmail.com'>shop.g3td@gmail.com</a>
                        </td>
                      </tr>
                    </table>";

                await _emailService.SendEmailAsync(
                    user.Email,
                    "🎉 Chào mừng bạn đến với Shop Nội Thất G3TD!",
                    htmlContent
                );
            }
            else
            {
                string htmlContentLogin = $@"
                    <table style='width:100%; max-width:600px; margin:auto; font-family:Arial, sans-serif; border:1px solid #ddd; border-radius:8px;'>
                      <tr style='background-color:#f4f4f4;'>
                        <td style='padding:20px; text-align:center;'>
                          <img src='https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTrud2n7QkcJe76V36LbSU6lLsTCAHStSYNAw&s' alt='Shop Nội Thất G3TD' style='height:80px;'>
                        </td>
                      </tr>
                      <tr>
                        <td style='padding:20px;'>
                          <h2>Xin chào {user.HoTen},</h2>
                          <p>Bạn vừa đăng nhập vào tài khoản của mình tại <strong>Shop Nội Thất G3TD</strong>.</p>
                          <p>Nếu không phải bạn, vui lòng liên hệ ngay với chúng tôi để được hỗ trợ.</p>
                          <p style='color:#555; font-size:14px;'>Email hỗ trợ: <a href='mailto:shop.g3td@gmail.com'>shop.g3td@gmail.com</a></p>
                        </td>
                      </tr>
                      <tr style='background-color:#f4f4f4;'>
                        <td style='padding:15px; text-align:center; font-size:14px; color:#888;'>
                          <img src='https://hienlaptop.com/wp-content/uploads/2024/10/460630409_519358010845041_4976973150130837642_n.jpg' alt='Chủ shop' style='width:50px; height:50px; border-radius:50%;'><br>
                          Chủ shop: G3TD
                        </td>
                      </tr>
                    </table>";

                await _emailService.SendEmailAsync(
                    user.Email,
                    "🔔 Thông báo đăng nhập từ Shop Nội Thất G3TD",
                    htmlContentLogin
                );
            }

            return RedirectToAction("Index", "Home");
        }


        // ===============================================
        // == KẾT THÚC PHẦN THÊM MỚI CHO FACEBOOK ==
        // ===============================================


        // GET: /Auth/Logout
        [HttpGet("Auth/Logout")]
        public async Task<IActionResult> Logout()
        {
            // Xoá cookie + session
            await HttpContext.SignOutAsync("MyCookie");
            HttpContext.Session.Clear();

            // Redirect về trang Home chính
            return RedirectToAction("Index", "Home");
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

        // GET quên mật khẩu - bước 1: Nhập email
        [HttpGet("/Quenmatkhau")]
        public IActionResult ForgotPassword()
        {
            ViewBag.Step = "Email"; // dùng ViewBag để phân biệt bước
            return View();
        }

        // POST quên mật khẩu - gửi OTP đến email
        [HttpPost("/Quenmatkhau")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Step = "Email";
                return View(model);
            }

            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Tài khoản không tồn tại.");
                ViewBag.Step = "Email";
                return View(model);
            }

            // Tạo mã OTP 6 chữ số
            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("ForgotPasswordEmail", user.Email);
            HttpContext.Session.SetString("ForgotPasswordOtp", otp);
            HttpContext.Session.SetString("OtpExpiry", DateTime.Now.AddMinutes(5).ToString());

            // Gửi mail OTP
            string content = $"Mã OTP của bạn là: <b>{otp}</b>. Mã có hiệu lực trong 5 phút.";
            await _emailService.SendEmailAsync(user.Email, "Mã OTP lấy lại mật khẩu", content);

            TempData["Message"] = $"Mã OTP đã được gửi tới email {user.Email}. Vui lòng kiểm tra hộp thư.";
            ViewBag.Step = "VerifyOtp";

            // Giữ model để giữ lại email khi chuyển sang form nhập OTP + mật khẩu mới
            return View(model);
        }
        [HttpGet]
        public IActionResult CheckLogin()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            bool loggedIn = maTK != null;
            return Json(new { loggedIn });
        }

        // POST xác nhận OTP và đổi mật khẩu
        [HttpPost("/Xacnhanotp")]
        public IActionResult VerifyOtp(ForgotPasswordViewModel model, string Otp, string NewPassword, string ConfirmPassword)
        {
            ViewBag.Step = "VerifyOtp";

            var sessionOtp = HttpContext.Session.GetString("ForgotPasswordOtp");
            var sessionEmail = HttpContext.Session.GetString("ForgotPasswordEmail");
            var expiryStr = HttpContext.Session.GetString("OtpExpiry");

            if (string.IsNullOrEmpty(sessionOtp) || string.IsNullOrEmpty(sessionEmail) || string.IsNullOrEmpty(expiryStr))
            {
                ModelState.AddModelError("", "Phiên làm việc đã hết hạn. Vui lòng thử lại.");
                return View("ForgotPassword", model);
            }

            if (!DateTime.TryParse(expiryStr, out var expiry))
            {
                ModelState.AddModelError("", "Phiên làm việc không hợp lệ. Vui lòng thử lại.");
                return View("ForgotPassword", model);
            }

            if (DateTime.Now > expiry)
            {
                ModelState.AddModelError("", "Mã OTP đã hết hạn. Vui lòng gửi lại yêu cầu.");
                return View("ForgotPassword", model);
            }

            if (Otp != sessionOtp)
            {
                ModelState.AddModelError("Otp", "Mã OTP không đúng.");
                return View("ForgotPassword", model);
            }

            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                return View("ForgotPassword", model);
            }

            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == sessionEmail);
            if (user == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View("ForgotPassword", model);
            }

            // Cập nhật mật khẩu mới
            user.MatKhau = NewPassword;
            _context.SaveChanges();

            // Xóa session OTP
            HttpContext.Session.Remove("ForgotPasswordEmail");
            HttpContext.Session.Remove("ForgotPasswordOtp");
            HttpContext.Session.Remove("OtpExpiry");

            TempData["Success"] = "Mật khẩu đã được cập nhật thành công. Bạn có thể đăng nhập lại.";
            return RedirectToAction("Login", "Auth");
        }

    }
}