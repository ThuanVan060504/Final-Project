using Final_Project.Models;
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Final_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThuongHieuSPController : Controller
    {
        private readonly AppDbContext _context;

        public ThuongHieuSPController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var thuongHieuList = _context.ThuongHieus.ToList();
            return View("~/Adminboot/Admin/Views/ThuongHieuSP/Index.cshtml", thuongHieuList);
        }
    }
}
