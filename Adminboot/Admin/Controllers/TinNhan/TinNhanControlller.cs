using Final_Project.Models.Chat;
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Final_Project.Adminboot.Admin.Controllers
{
    [Area("Admin")]
    public class TinNhanController : Controller
    {
        private readonly AppDbContext _context;
        private const int AdminId = 3;

        public TinNhanController(AppDbContext context)
        {
            _context = context;
        }

        // Danh sách người đã chat
        public IActionResult Index()
        {
            var users = _context.TinNhans
                .Where(t => t.NguoiNhanId == AdminId || t.NguoiGuiId == AdminId)
                .Select(t => t.NguoiGuiId == AdminId ? t.NguoiNhan : t.NguoiGui)
                .Distinct()
                .ToList();

            return View("~/Adminboot/Admin/Views/TinNhan/Index.cshtml", users);
        }

        // Xem chat với 1 user
        public IActionResult ChatWithUser(int userId)
        {
            var messages = _context.TinNhans
                .Include(t => t.NguoiGui)
                .Include(t => t.NguoiNhan)
                .Where(t => (t.NguoiGuiId == userId && t.NguoiNhanId == AdminId) ||
                            (t.NguoiGuiId == AdminId && t.NguoiNhanId == userId))
                .OrderBy(t => t.ThoiGianGui)
                .ToList();

            ViewBag.UserId = userId;
            return View("~/Adminboot/Admin/Views/TinNhan/ChatWithUser.cshtml", messages);
        }

        [HttpPost]
        public IActionResult SendMessage(int userId, string noiDung)
        {
            if (!string.IsNullOrWhiteSpace(noiDung))
            {
                var message = new TinNhan
                {
                    NguoiGuiId = AdminId,
                    NguoiNhanId = userId,
                    NoiDung = noiDung,
                    ThoiGianGui = DateTime.Now
                };

                _context.TinNhans.Add(message);
                _context.SaveChanges();
            }

            return RedirectToAction("ChatWithUser", new { userId });
        }
    }
}
