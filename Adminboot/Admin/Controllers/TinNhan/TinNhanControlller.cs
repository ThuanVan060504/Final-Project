using Final_Project.Models.Chat;
using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;

namespace Final_Project.Adminboot.Admin.Controllers
{
    [Area("Admin")]
    public class TinNhanController : Controller
    {
        private readonly AppDbContext _context;
        private string _connectionString;
        private const int AdminId = 3;

        public TinNhanController(AppDbContext context)
        {
            _context = context;
        }

        // Danh sách người đã chat
        public IActionResult Index(int? userId)
        {
            var users = _context.TinNhans
                .Where(t => t.NguoiNhanId == AdminId || t.NguoiGuiId == AdminId)
                .Select(t => t.NguoiGuiId == AdminId ? t.NguoiNhan : t.NguoiGui)
                .Distinct()
                .ToList();

            List<TinNhan> messages = new List<TinNhan>();
            string userName = null;

            if (userId.HasValue)
            {
                messages = _context.TinNhans
                    .Include(t => t.NguoiGui)
                    .Include(t => t.NguoiNhan)
                    .Where(t => (t.NguoiGuiId == userId.Value && t.NguoiNhanId == AdminId) ||
                                (t.NguoiGuiId == AdminId && t.NguoiNhanId == userId.Value))
                    .OrderBy(t => t.ThoiGianGui)
                    .ToList();

                userName = _context.TaiKhoans
                    .Where(u => u.MaTK == userId.Value)
                    .Select(u => u.HoTen)
                    .FirstOrDefault();
            }

            ViewBag.UserId = userId;
            ViewBag.UserName = userName ?? "Người dùng";

            var model = new Tuple<List<TaiKhoan>, List<TinNhan>>(users, messages);
            return View("~/Adminboot/Admin/Views/TinNhan/Index.cshtml", model);
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

            var userName = _context.TaiKhoans
                .Where(u => u.MaTK == userId)
                .Select(u => u.HoTen)
                .FirstOrDefault();

            ViewBag.UserId = userId;
            ViewBag.UserName = userName ?? "Người dùng";
            return View("~/Adminboot/Admin/Views/TinNhan/ChatWithUser.cshtml", messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(int userId, string noiDung)
        {
            if (string.IsNullOrWhiteSpace(noiDung))
                return RedirectToAction("ChatWithUser", new { userId });

            var message = new TinNhan
            {
                NguoiGuiId = AdminId,  // 3
                NguoiNhanId = userId,
                NoiDung = noiDung,
                ThoiGianGui = DateTime.Now
            };

            _context.TinNhans.Add(message);
            _context.SaveChanges();

            return RedirectToAction("ChatWithUser", new { userId });
        }

    }
}


