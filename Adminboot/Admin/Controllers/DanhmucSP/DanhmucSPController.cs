using Final_Project.Models;
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Final_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DanhmucSPController : BaseAdminController
    {
        private readonly AppDbContext _context;

        public DanhmucSPController(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // Hiển thị danh sách danh mục
        public IActionResult Index()
        {
            var danhMucList = _context.DanhMucs
                                      .Include(dm => dm.SanPhams)
                                      .ToList();
            return View("~/Adminboot/Admin/Views/DanhmucSP/Index.cshtml", danhMucList);
        }

        // GET: Form thêm danh mục
        public IActionResult Create()
        {
            return View("~/Adminboot/Admin/Views/DanhmucSP/Create.cshtml");
        }

        // POST: Xử lý thêm danh mục
        [HttpPost]
        public async Task<IActionResult> Create(string TenDanhMuc, string MoTa, IFormFile Anh, string LinkLogo)
        {
            if (string.IsNullOrEmpty(TenDanhMuc))
            {
                ModelState.AddModelError("TenDanhMuc", "Tên danh mục là bắt buộc.");
                return View("~/Adminboot/Admin/Views/DanhmucSP/Create.cshtml");
            }

            string logoPath = null;

            if (Anh != null && Anh.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                var fileName = Path.GetFileName(Anh.FileName);
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Anh.CopyToAsync(stream);
                }

                logoPath = "/uploads/" + fileName;
            }
            else if (!string.IsNullOrEmpty(LinkLogo))
            {
                logoPath = LinkLogo;
            }
            else
            {
                logoPath = "/images/no-image.png"; // ảnh mặc định
            }

            var danhMuc = new DanhMuc
            {
                TenDanhMuc = TenDanhMuc,
                MoTa = MoTa,
                Logo = logoPath
            };

            _context.DanhMucs.Add(danhMuc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Thêm danh mục thành công!";
            return RedirectToAction("Index");
        }

        // GET: Form chỉnh sửa danh mục
        public async Task<IActionResult> Edit(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null)
                return NotFound();

            return View("~/Adminboot/Admin/Views/DanhmucSP/Edit.cshtml", danhMuc);
        }

        // POST: Xử lý chỉnh sửa danh mục
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string TenDanhMuc, string MoTa, IFormFile Anh, string LinkLogo)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null)
                return NotFound();

            if (string.IsNullOrEmpty(TenDanhMuc))
            {
                ModelState.AddModelError("TenDanhMuc", "Tên danh mục là bắt buộc.");
                return View("~/Adminboot/Admin/Views/DanhmucSP/Edit.cshtml", danhMuc);
            }

            danhMuc.TenDanhMuc = TenDanhMuc;
            danhMuc.MoTa = MoTa;

            if (Anh != null && Anh.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                var fileName = Path.GetFileName(Anh.FileName);
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Anh.CopyToAsync(stream);
                }

                danhMuc.Logo = "/uploads/" + fileName;
            }
            else if (!string.IsNullOrEmpty(LinkLogo))
            {
                danhMuc.Logo = LinkLogo;
            }
            // else giữ nguyên logo cũ

            _context.DanhMucs.Update(danhMuc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Cập nhật danh mục thành công!";
            return RedirectToAction("Index");
        }

        // GET: Xác nhận xóa danh mục
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null)
                return NotFound();

            _context.DanhMucs.Remove(danhMuc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Xóa danh mục thành công!";
            return RedirectToAction("Index");
        }

        // POST: Xóa danh mục
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null)
            {
                TempData["Error"] = "❌ Danh mục không tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            var hasSP = await _context.SanPhams.AnyAsync(sp => sp.MaDanhMuc == id);
            if (hasSP)
            {
                TempData["Error"] = "❌ Danh mục đang có sản phẩm, không thể xóa!";
                return RedirectToAction(nameof(Index));
            }

            _context.DanhMucs.Remove(danhMuc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Xóa danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
