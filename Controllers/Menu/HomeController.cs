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
