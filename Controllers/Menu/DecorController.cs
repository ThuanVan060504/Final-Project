using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_Project.Controllers.Menu
{
    public class DecorController : Controller
    {
        private readonly AppDbContext _context;

        public DecorController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(int? category)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }

            // 💚 load danh mục để làm sidebar / dropdown
            var danhMucList = _context.DanhMucDecors.AsNoTracking().ToList();
            ViewBag.DanhMuc = danhMucList;
            ViewBag.CategorySelected = category;   // để highlight dropdown/danh mục

            // 💚 query decor (kèm tên danh mục)
            var query = _context.Decors
                                .Include(d => d.DanhMuc)
                                .AsQueryable();

            if (category.HasValue)
                query = query.Where(d => d.MaDanhMuc == category.Value);

            var listDecor = query.ToList(); // ⚠ sửa ở đây, đừng dùng _context.Decors.ToList()

            return View(listDecor);
        }

    }
}
