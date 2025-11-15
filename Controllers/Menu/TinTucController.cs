using Final_Project.Models;
using Final_Project.Models.Shop;
using Final_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; 
using System.Collections.Generic; 

namespace Final_Project.Controllers
{
    public class TinTucController : Controller
    {
        private readonly AppDbContext _context;

        public TinTucController(AppDbContext context)
        {
            _context = context;
        }

        // Trang chính hiển thị tin tức từ nhiều nguồn
        public IActionResult Index()
        {
            LoadCommonData();

            var tinTucList = NewsService.GetNewsFromAll();
            return View(tinTucList);
        }

        // Mở chi tiết tin bằng link gốc (redirect đến bài viết)
        public IActionResult ChiTiet(string url)
        {
            if (string.IsNullOrEmpty(url))
                return RedirectToAction("Index");

            return Redirect(url);
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