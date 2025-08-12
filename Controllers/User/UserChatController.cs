using Final_Project.Models.Chat;
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_Project.Controllers
{
    public class UserChatController : Controller
    {
        private readonly AppDbContext _context;

        public UserChatController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return RedirectToAction("Login", "Auth");

            // Lấy thông tin tài khoản để hiển thị avatar & họ tên
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
            ViewBag.Avatar = taiKhoan?.Avatar;
            ViewBag.HoTen = taiKhoan?.HoTen;

            // Lấy danh sách tin nhắn giữa user hiện tại và admin (ID = 3)
            var messages = _context.TinNhans
                .Where(m =>
                    (m.NguoiGuiId == maTK && m.NguoiNhanId == 3) ||
                    (m.NguoiGuiId == 3 && m.NguoiNhanId == maTK))
                .OrderBy(m => m.ThoiGianGui)
                .Select(m => new MessageModel
                {
                    MaTinNhan = m.MaTinNhan,
                    NguoiGuiId = m.NguoiGuiId,
                    NguoiNhanId = m.NguoiNhanId,
                    NoiDung = m.NoiDung,
                    ThoiGianGui = m.ThoiGianGui
                })
                .ToList();

            return View("~/Views/User/TinNhan/Index.cshtml", messages);
        }

        [HttpPost]
        public IActionResult GuiTinNhan(string noiDung)
        {
            var userId = HttpContext.Session.GetInt32("MaTK");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var tinNhan = new TinNhan
            {
                NguoiGuiId = userId.Value,
                NguoiNhanId = 3, // ID admin
                NoiDung = noiDung,
                ThoiGianGui = DateTime.Now
            };

            _context.TinNhans.Add(tinNhan);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }

    public class MessageModel
    {
        public int MaTinNhan { get; set; }
        public int NguoiGuiId { get; set; }
        public int NguoiNhanId { get; set; }
        public string NoiDung { get; set; }
        public DateTime ThoiGianGui { get; set; }
    }
}
