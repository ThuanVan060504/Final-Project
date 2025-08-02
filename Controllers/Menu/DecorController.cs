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

        public IActionResult Index(string category, string search, string sort)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }

            // 💚 Load danh mục
            var danhMucList = _context.DanhMucDecors.AsNoTracking().ToList();
            ViewBag.DanhMuc = danhMucList;
            ViewBag.CategorySelected = category;

            // 💚 Query decor (kèm tên danh mục)
            var decorList = _context.Decors
                                    .Include(d => d.DanhMuc)
                                    .AsQueryable();

            // 🔎 Filter theo danh mục
            if (!string.IsNullOrEmpty(category))
            {
                decorList = decorList.Where(d => d.DanhMuc.TenDanhMuc == category);
            }

            // 🔍 Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(search))
            {
                decorList = decorList.Where(d => d.TenDecor.Contains(search));
            }

            // ⬆⬇ Sắp xếp theo tên decor
            if (sort == "asc")
            {
                decorList = decorList.OrderBy(d => d.TenDecor);
            }
            else if (sort == "desc")
            {
                decorList = decorList.OrderByDescending(d => d.TenDecor);
            }

            var listDecor = decorList.ToList();

            return View(listDecor);
        }
    }
}
