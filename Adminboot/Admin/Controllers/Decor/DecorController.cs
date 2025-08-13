using Final_Project.Areas.Admin.Controllers;
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Final_Project.Adminboot.Admin.Controllers.Decor
{
    [Area("Admin")]
    public class DecorController : BaseAdminController
    {
        private readonly AppDbContext _context;

        public DecorController(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // GET: Index
        public async Task<IActionResult> Index()
        {
            var decors = await _context.Decors
                .Include(d => d.DanhMuc)
                .ToListAsync();

            ViewBag.DanhMucList = await _context.DanhMucDecors.ToListAsync();
            return View("~/Adminboot/Admin/Views/AdminDecor/Index.cshtml", decors);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string TenDecor, string MoTa, int? MaDanhMuc, string Link3D)
        {
            if (string.IsNullOrWhiteSpace(TenDecor) || string.IsNullOrWhiteSpace(Link3D))
            {
                TempData["Error"] = "Tên Decor và Link 3D không được để trống.";
                return RedirectToAction(nameof(Index));
            }

            var newDecor = new Final_Project.Models.Shop.Decor
            {
                TenDecor = TenDecor,
                MoTa = MoTa,
                MaDanhMuc = MaDanhMuc ?? 0,
                Link3D = Link3D
            };

            _context.Decors.Add(newDecor);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm Decor thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var decor = await _context.Decors.FindAsync(id);
            if (decor == null) return NotFound();

            ViewBag.DanhMucList = await _context.DanhMucDecors.ToListAsync();
            return View("~/Adminboot/Admin/Views/AdminDecor/Edit.cshtml", decor);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Final_Project.Models.Shop.Decor decor)
        {
            if (id != decor.MaDecor) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(decor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật Decor thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.DanhMucList = await _context.DanhMucDecors.ToListAsync();
            return View(decor);
        }

    }
}

