using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_Project.Controllers
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
            var product = _context.SanPhams
                .FirstOrDefault(p => p.MaSP == id);

            if (product == null)
                return NotFound();

            return View(product);
        }
        }
}
