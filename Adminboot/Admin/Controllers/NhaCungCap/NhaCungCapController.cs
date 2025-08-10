using Microsoft.AspNetCore.Mvc;
using Final_Project.Models.Shop;
using System.Linq;

namespace Final_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhaCungCapController : Controller
    {
        private readonly AppDbContext _context;

        public NhaCungCapController(AppDbContext context)
        {
            _context = context;
        }

        // Index
        public IActionResult Index()
        {
            var list = _context.NhaCungCaps.ToList();
            return View("~/Adminboot/Admin/Views/NhaCungCap/Index.cshtml", list);
        }

        // Create GET
        public IActionResult Create()
        {
            return View("~/Adminboot/Admin/Views/NhaCungCap/Create.cshtml");
        }

        // Create POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NhaCungCap ncc)
        {
            if (ModelState.IsValid)
            {
                _context.NhaCungCaps.Add(ncc);
                _context.SaveChanges();
                TempData["Success"] = "Thêm nhà cung cấp thành công!";
                return RedirectToAction("Index");
            }
            return View("~/Adminboot/Admin/Views/NhaCungCap/Create.cshtml", ncc);
        }

        // Edit GET
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var ncc = _context.NhaCungCaps.FirstOrDefault(x => x.MaNCC == id);
            if (ncc == null) return NotFound();
            return View("~/Adminboot/Admin/Views/NhaCungCap/Edit.cshtml", ncc);
        }

        // Edit POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int MaNCC, string TenNCC, string SoDienThoai, string DiaChi, string Email)
        {
            var nccInDb = _context.NhaCungCaps.FirstOrDefault(x => x.MaNCC == MaNCC);
            if (nccInDb == null) return NotFound();

            nccInDb.TenNCC = TenNCC;
            nccInDb.SoDienThoai = SoDienThoai;
            nccInDb.DiaChi = DiaChi;
            nccInDb.Email = Email;

            _context.SaveChanges();

            TempData["Success"] = "Cập nhật nhà cung cấp thành công!";

            return RedirectToAction("Index", "NhaCungCap", new { area = "Admin" });
        }

        // Delete POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var ncc = _context.NhaCungCaps.Find(id);
            if (ncc == null)
            {
                return Json(new { success = false, message = "Nhà cung cấp không tồn tại" });
            }
            _context.NhaCungCaps.Remove(ncc);
            _context.SaveChanges();
            return Json(new { success = true });
        }
    }
}
