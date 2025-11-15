using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Đảm bảo bạn đã using System.Linq

namespace Final_Project.Controllers.Menu
{
    public class GioiThieuController : Controller
    {
        private readonly AppDbContext _context;

        public GioiThieuController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            LoadCommonData();

            return View();
        }

        private void LoadCommonData()
        {
            // Lấy thông tin User
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }

            // Lấy danh mục
            var danhMucs = _context.DanhMucs
                .Include(d => d.SanPhams)
                .ToList();

            ViewBag.DanhMucs = danhMucs;
        }
    }
}