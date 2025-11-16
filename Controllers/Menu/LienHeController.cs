using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- Thêm using này
using System.Linq; // <-- Thêm using này
using System.Collections.Generic; // <-- Thêm using này

namespace Final_Project.Controllers.Menu
{
    public class LienHeController : Controller
    {
        private readonly AppDbContext _context;

        public LienHeController(AppDbContext context)
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

            // Lấy danh mục (cho _Layout)
            var danhMucs = _context.DanhMucs
                .Include(d => d.SanPhams)
                .ToList();

            ViewBag.DanhMucs = danhMucs;
        }
    }
}