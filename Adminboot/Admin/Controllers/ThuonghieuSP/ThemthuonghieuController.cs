using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace Final_Project.Adminboot.Admin.Controllers.ThuonghieuSP
{
    [Area("Admin")]
    public class ThemthuonghieuController : Controller
    {
        private readonly AppDbContext _context;

        public ThemthuonghieuController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View("/Adminboot/Admin/Views/ThemThuongHieu/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Index(int MaThuongHieu, string TenThuongHieu, IFormFile Anh, string LinkLogo)
        {
            if (string.IsNullOrEmpty(TenThuongHieu))
            {
                ModelState.AddModelError("TenThuongHieu", "Tên thương hiệu là bắt buộc.");
                return View("/Adminboot/Admin/Views/ThemThuongHieu/Index.cshtml");
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
            return View("/Adminboot/Admin/Views/ThemThuongHieu/Index.cshtml");
        }

    }
}