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
    public class ThuongHieuSPController : Controller
    {
        private readonly AppDbContext _context;

        public ThuongHieuSPController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Danh sách thương hiệu
        public IActionResult Index()
        {
            var thuongHieuList = _context.ThuongHieus.ToList();
            return View("~/Adminboot/Admin/Views/ThuongHieuSp/Index.cshtml", thuongHieuList);
        }

        // GET: Hiển thị form thêm
        public IActionResult Create()
        {
            return View("~/Adminboot/Admin/Views/ThuongHieuSp/Create.cshtml");
        }

        // POST: Xử lý thêm thương hiệu
        [HttpPost]
        public async Task<IActionResult> Create(int MaThuongHieu, string TenThuongHieu, IFormFile Anh, string LinkLogo)
        {
            if (string.IsNullOrEmpty(TenThuongHieu))
            {
                ModelState.AddModelError("TenThuongHieu", "Tên thương hiệu là bắt buộc.");
                return View("~/Adminboot/Admin/Views/ThuongHieuSp/Create.cshtml");
            }

            string imagePath = null;

            if (Anh != null && Anh.Length > 0)
            {
                var fileName = Path.GetFileName(Anh.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await Anh.CopyToAsync(stream);
                }

                imagePath = "/uploads/" + fileName;
            }
            else if (!string.IsNullOrEmpty(LinkLogo))
            {
                imagePath = LinkLogo;
            }

            var thuongHieu = new ThuongHieu
            {
                MaThuongHieu = MaThuongHieu,
                TenThuongHieu = TenThuongHieu,
                Logo = imagePath // sẽ là ảnh tải lên hoặc link
            };

            _context.ThuongHieus.Add(thuongHieu);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Thêm thương hiệu thành công!";
            return RedirectToAction("Index");
        }

        // GET: Hiển thị form Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var thuongHieu = await _context.ThuongHieus.FindAsync(id);
            if (thuongHieu == null)
            {
                TempData["Error"] = "❌ Không tìm thấy thương hiệu cần sửa!";
                return RedirectToAction("Index");
            }

            return View("~/Adminboot/Admin/Views/ThuongHieuSp/Edit.cshtml", thuongHieu);
        }

        // POST: Xử lý sửa thương hiệu
        [HttpPost]
        public async Task<IActionResult> Edit(int MaThuongHieu, string TenThuongHieu, IFormFile Anh, string LinkLogo)
        {
            var thuongHieu = await _context.ThuongHieus.FindAsync(MaThuongHieu);
            if (thuongHieu == null)
            {
                TempData["Error"] = "❌ Không tìm thấy thương hiệu cần sửa!";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(TenThuongHieu))
            {
                ModelState.AddModelError("TenThuongHieu", "Tên thương hiệu là bắt buộc.");
                return View("~/Adminboot/Admin/Views/ThuongHieuSp/Edit.cshtml", thuongHieu);
            }

            string imagePath = thuongHieu.Logo; // giữ ảnh cũ nếu không đổi

            if (Anh != null && Anh.Length > 0)
            {
                var fileName = Path.GetFileName(Anh.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await Anh.CopyToAsync(stream);
                }

                imagePath = "/uploads/" + fileName;
            }
            else if (!string.IsNullOrEmpty(LinkLogo))
            {
                imagePath = LinkLogo;
            }

            // Cập nhật dữ liệu
            thuongHieu.TenThuongHieu = TenThuongHieu;
            thuongHieu.Logo = imagePath;

            _context.ThuongHieus.Update(thuongHieu);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Cập nhật thương hiệu thành công!";
            return RedirectToAction("Index");
        }
        // GET: Xóa thương hiệu
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var thuongHieu = await _context.ThuongHieus
                                           .Include(th => th.SanPhams)
                                           .FirstOrDefaultAsync(th => th.MaThuongHieu == id);
            if (thuongHieu == null)
            {   
                TempData["Error"] = "❌ Không tìm thấy thương hiệu cần xóa!";
                return RedirectToAction("Index");
            }

            // Set MaThuongHieu = null cho tất cả sản phẩm thuộc thương hiệu này
            foreach (var sp in thuongHieu.SanPhams)
            {
                sp.MaThuongHieu = null;
            }

            _context.ThuongHieus.Remove(thuongHieu);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Xóa thương hiệu thành công!";
            return RedirectToAction("Index");
        }
    }
}
