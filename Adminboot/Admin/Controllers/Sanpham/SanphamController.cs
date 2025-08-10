using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

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

        // GET: Form thêm sản phẩm
        public IActionResult Create()
        {
            ViewBag.DanhMucList = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc");
            ViewBag.ThuongHieuList = new SelectList(_context.ThuongHieus, "MaThuongHieu", "TenThuongHieu");
            return View("~/Adminboot/Admin/Views/Sanpham/Create.cshtml");
        }

        // POST: Thêm sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: Form sửa sản phẩm
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null) return NotFound();

            ViewBag.DanhMucList = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc", sanPham.MaDanhMuc);
            ViewBag.ThuongHieuList = new SelectList(_context.ThuongHieus, "MaThuongHieu", "TenThuongHieu", sanPham.MaThuongHieu);

            return View("~/Adminboot/Admin/Views/Sanpham/Edit.cshtml", sanPham);
        }

        // POST: Xử lý sửa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int MaSP, string TenSP, string MoTa, decimal DonGia, decimal GiaGoc, int SoLuong, double ChieuRong, double ChieuCao, double ChieuSau, int MaDanhMuc, int MaThuongHieu, IFormFile Anh, string ImageURL)
        {
            var sanPham = await _context.SanPhams.FindAsync(MaSP);
            if (sanPham == null) return NotFound();

            if (string.IsNullOrEmpty(TenSP))
            {
                ModelState.AddModelError("TenSP", "Tên sản phẩm là bắt buộc.");
                ViewBag.DanhMucList = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc", MaDanhMuc);
                ViewBag.ThuongHieuList = new SelectList(_context.ThuongHieus, "MaThuongHieu", "TenThuongHieu", MaThuongHieu);
                return View("~/Adminboot/Admin/Views/SanPham/Edit.cshtml", sanPham);
            }

            sanPham.TenSP = TenSP;
            sanPham.MoTa = MoTa;
            sanPham.DonGia = DonGia;
            sanPham.GiaGoc = GiaGoc;
            sanPham.SoLuong = SoLuong;
            sanPham.ChieuRong = (int?)Convert.ToInt32(ChieuRong);
            sanPham.ChieuCao = (int?)Convert.ToInt32(ChieuCao);
            sanPham.ChieuSau = (int?)Convert.ToInt32(ChieuSau);
            sanPham.MaDanhMuc = MaDanhMuc;
            sanPham.MaThuongHieu = MaThuongHieu;

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

                sanPham.ImageURL = "/uploads/" + fileName;
            }
            else if (!string.IsNullOrEmpty(ImageURL))
            {
                sanPham.ImageURL = ImageURL;
            }
            // else giữ nguyên ImageURL cũ

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                TempData["Error"] = "Lỗi khi cập nhật sản phẩm: " + innerMsg;
                ViewBag.DanhMucList = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc", MaDanhMuc);
                ViewBag.ThuongHieuList = new SelectList(_context.ThuongHieus, "MaThuongHieu", "TenThuongHieu", MaThuongHieu);
                return View("~/Adminboot/Admin/Views/SanPham/Edit.cshtml", sanPham);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khác: " + ex.Message;
                ViewBag.DanhMucList = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc", MaDanhMuc);
                ViewBag.ThuongHieuList = new SelectList(_context.ThuongHieus, "MaThuongHieu", "TenThuongHieu", MaThuongHieu);
                return View("~/Adminboot/Admin/Views/SanPham/Edit.cshtml", sanPham);
            }

            TempData["Success"] = "✅ Cập nhật sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        // POST: Xóa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            try
            {
                _context.SanPhams.Remove(sp);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa sản phẩm: " + ex.Message });
            }
        }
    }
}
