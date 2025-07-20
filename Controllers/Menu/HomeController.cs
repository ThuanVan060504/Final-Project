using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Final_Project.Controllers.Menu
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy thông tin tài khoản đang đăng nhập
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }

            // Lấy danh sách sản phẩm
            var sanPhams = _context.SanPhams
                .Include(sp => sp.DanhMuc)
                .Include(p => p.ThuongHieu)
                .OrderByDescending(sp => sp.MaSP)
                .Take(6)
                .ToList();

            return View(sanPhams);
        }


        public IActionResult Details(int id)
        {
            var product = _context.SanPhams
                .FirstOrDefault(p => p.MaSP == id);

            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}
