using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

namespace Final_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SanPhamController : Controller
    {
        private readonly AppDbContext _context;

        public SanPhamController(AppDbContext context)
        {
            _context = context;
        }

        // Danh sách sản phẩm
        public IActionResult Index()
        {
            var sanPhamList = _context.SanPhams
                .Include(s => s.DanhMuc)
                .Include(s => s.ThuongHieu)
                .ToList();

            return View("~/Adminboot/Admin/Views/Sanpham/Index.cshtml", sanPhamList);
        }

        // Form thêm sản phẩm
        public IActionResult Create()
        {
            ViewBag.DanhMucList = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc");
            ViewBag.ThuongHieuList = new SelectList(_context.ThuongHieus, "MaThuongHieu", "TenThuongHieu");
            return View("~/Adminboot/Admin/Views/Sanpham/Create.cshtml");
        }

        // Xử lý thêm sản phẩm
        [HttpPost]
        public async Task<IActionResult> Create(
            string TenSP,
            string MoTa,
            decimal DonGia,
            decimal GiaGoc,
            int SoLuong,
            double ChieuRong,
            double ChieuCao,
            double ChieuSau,
            int MaDanhMuc,
            int MaThuongHieu,
            IFormFile Anh,
            string ImageURL)
        {
            if (string.IsNullOrEmpty(TenSP))
            {
                ModelState.AddModelError("TenSP", "Tên sản phẩm là bắt buộc.");
                ViewBag.DanhMucList = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc", MaDanhMuc);
                ViewBag.ThuongHieuList = new SelectList(_context.ThuongHieus, "MaThuongHieu", "TenThuongHieu", MaThuongHieu);
                return View("~/Adminboot/Admin/Views/Sanpham/Create.cshtml");
            }

            string imagePath = null;

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

                imagePath = "/uploads/" + fileName;
            }
            else if (!string.IsNullOrEmpty(ImageURL))
            {
                imagePath = ImageURL;
            }
            else
            {
                imagePath = "/images/no-image.png";
            }

            var sanPham = new SanPham
            {
                TenSP = TenSP,
                MoTa = MoTa,
                DonGia = DonGia,
                GiaGoc = GiaGoc,
                SoLuong = SoLuong,
                ChieuRong = (int?)ChieuRong,
                ChieuCao = (int?)ChieuCao,
                ChieuSau = (int?)ChieuSau,
                MaDanhMuc = MaDanhMuc,
                MaThuongHieu = MaThuongHieu,
                ImageURL = imagePath,
                NgayTao = DateTime.Now
            };

            _context.SanPhams.Add(sanPham);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Thêm sản phẩm thành công!";
            return RedirectToAction("Index");
        }
    }
}
