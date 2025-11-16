using Final_Project.Models.Shop; // <-- Thêm using này
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- Thêm using này
using System.Linq; // <-- Thêm using này
using System.Collections.Generic; // <-- Thêm using này

namespace Final_Project.Controllers.Menu
{
    public class ThongTinController : Controller
    {
        // ✅ BƯỚC 1: Inject AppDbContext
        private readonly AppDbContext _context;

        public ThongTinController(AppDbContext context)
        {
            _context = context;
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

        // /Menu/ThongTin/DieuKhoan
        public IActionResult DieuKhoan()
        {
            LoadCommonData();
            return View();
        }

        // /Menu/ThongTin/BaoMat
        public IActionResult BaoMat()
        {
            LoadCommonData();
            return View();
        }
    }
}