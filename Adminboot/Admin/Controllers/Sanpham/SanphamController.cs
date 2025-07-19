
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_Project.Adminboot.Admin.Controllers.Sanpham
{
    [Area("Admin")]
    public class SanphamController : Controller
    {
        private readonly AppDbContext _context;

        public SanphamController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.SanPhams
                .Include(p => p.DanhMuc)
                .Include(p => p.ThuongHieu)
                .ToList();

            return View("~/Adminboot/Admin/Views/Sanpham/Index.cshtml", products);
        }
    }
}
