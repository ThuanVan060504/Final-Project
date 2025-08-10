using Microsoft.AspNetCore.Mvc;
using Final_Project.Models.Shop;

namespace Final_Project.Adminboot.Admin.Controllers
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
        public IActionResult Create(string TenNCC, string SoDienThoai, string DiaChi, string Email)
        {
            if (!string.IsNullOrWhiteSpace(TenNCC))
            {
                var ncc = new NhaCungCap
                {
                    TenNCC = TenNCC,
                    SoDienThoai = SoDienThoai,
                    DiaChi = DiaChi,
                    Email = Email
                };
                _context.NhaCungCaps.Add(ncc);
                _context.SaveChanges();

                TempData["Success"] = "Thêm nhà cung cấp thành công!";
                return RedirectToAction("Index");
            }

            // Trả lại dữ liệu nếu nhập lỗi
            ViewBag.TenNCC = TenNCC;
            ViewBag.SoDienThoai = SoDienThoai;
            ViewBag.DiaChi = DiaChi;
            ViewBag.Email = Email;
            return View("~/Adminboot/Admin/Views/NhaCungCap/Create.cshtml");
        }


        // Edit GET
        public IActionResult Edit(int id)
        {
            var ncc = _context.NhaCungCaps.Find(id);
            if (ncc == null) return NotFound();
            return View("~/Adminboot/Admin/Views/NhaCungCap/Edit.cshtml", ncc);
        }

        // Edit POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NhaCungCap ncc)
        {
            if (ModelState.IsValid)
            {
                _context.NhaCungCaps.Update(ncc);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("~/Adminboot/Admin/Views/NhaCungCap/Edit.cshtml", ncc);
        }

        // Delete GET (Confirm)
        public IActionResult Delete(int id)
        {
            var ncc = _context.NhaCungCaps.Find(id);
            if (ncc == null) return NotFound();
            return View("~/Adminboot/Admin/Views/NhaCungCap/Delete.cshtml", ncc);
        }

        // Delete POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var ncc = _context.NhaCungCaps.Find(id);
            if (ncc != null)
            {
                _context.NhaCungCaps.Remove(ncc);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
