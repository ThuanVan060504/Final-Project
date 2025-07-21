using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Final_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThemdanhmucSPController : Controller
    {
        private readonly AppDbContext _context;

        public ThemdanhmucSPController(AppDbContext context)
        {
            _context = context;
        }

        // Hiển thị form thêm danh mục
        public IActionResult Index()
        {
            return View("~/Adminboot/Admin/Views/ThemDanhmucSP/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Index(string TenDanhMuc, string MoTa, IFormFile Anh, string LinkLogo)
        {
            if (string.IsNullOrEmpty(TenDanhMuc))
            {
                ModelState.AddModelError("TenDanhMuc", "Tên danh mục là bắt buộc.");
                return View("~/Adminboot/Admin/Views/ThemDanhmucSP/Index.cshtml");
            }

            string logoPath = null;

            // Ưu tiên ảnh upload
            if (Anh != null && Anh.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

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
            return View("~/Adminboot/Admin/Views/ThemDanhmucSP/Index.cshtml");
        }

    }
}
