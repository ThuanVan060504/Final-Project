using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final_Project.Controllers
{
    [Authorize] // Chặn chưa login
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Profile()
        {
            // Lấy Email của người dùng từ Claims
            var email = User.FindFirstValue(ClaimTypes.Email);

            // Tìm user theo Email
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            return View(user); // Truyền user qua view
        }
    }
}
