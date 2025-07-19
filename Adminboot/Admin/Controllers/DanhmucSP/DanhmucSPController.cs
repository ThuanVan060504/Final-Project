using Final_Project.Models;
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Final_Project.Adminboot.Admin.Controllers.DanhmucSP
{
    [Area("Admin")]
    public class DanhmucSPController : Controller
    {
        private readonly AppDbContext _context;

        public DanhmucSPController(AppDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            var danhMucList = _context.DanhMucs
                                      .Include(dm => dm.SanPhams) // Để đếm tổng sản phẩm
                                      .ToList();
            return View("~/Adminboot/Admin/Views/DanhmucSP/Index.cshtml", danhMucList);
        }

    }
}
