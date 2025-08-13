using Final_Project.Models.Chat;
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Final_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BaseAdminController : Controller
    {
        protected readonly AppDbContext _context;

        public BaseAdminController(AppDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            // Lấy tin nhắn mới cho admin
            var messages = _context.TinNhans
                .Include(t => t.NguoiGui)
                .Where(t => t.NguoiNhan.VaiTro == "Admin")
                .OrderByDescending(t => t.ThoiGianGui)
                .Take(5)
                .ToList();

            ViewBag.RecentMessages = messages;

            base.OnActionExecuting(context);
        }
    }
}
