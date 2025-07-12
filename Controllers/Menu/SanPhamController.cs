using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_Project.Controllers.Menu
{
    public class SanPhamController : Controller
    {
        private readonly AppDbContext _context;

        public SanPhamController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string category, string search, string sort)
        {
            var products = _context.SanPhams.AsQueryable();

            // Lọc theo danh mục
            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.DanhMuc.TenDanhMuc == category);
            }

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.TenSP.Contains(search));
            }

            // Sắp xếp
            switch (sort)
            {
                case "asc":
                    products = products.OrderBy(p => p.DonGia);
                    break;
                case "desc":
                    products = products.OrderByDescending(p => p.DonGia);
                    break;
            }

            return View(products.ToList());
        }

        public IActionResult Details(int id)
        {
            var sp = _context.SanPhams
                .Include(s => s.DanhMuc) // nếu có navigation
                .FirstOrDefault(s => s.MaSP == id);

            if (sp == null)
                return NotFound();

            // Lấy sản phẩm tương tự
            var tuongTu = _context.SanPhams
                .Where(s => s.DanhMuc == sp.DanhMuc && s.MaSP != sp.MaSP)
                .Take(4)
                .ToList();

            ViewBag.SanPhamTuongTu = tuongTu;

            return View(sp);
        }

    }
}
