using Final_Project.Models.Shop;
using Final_Project.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Final_Project.Adminboot.Admin.Controllers.Sanpham
{
    [Area("Admin")]
    public class SanphamController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public SanphamController(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            var products = _context.SanPhams
                .Include(p => p.DanhMuc)
                .Include(p => p.ThuongHieu)
                .ToList();

            return View("~/Adminboot/Admin/Views/Sanpham/Index.cshtml", products);
        }
        // GET: Admin/Sanpham/Create
        public IActionResult Create()
        {
            ViewBag.DanhMucList = new SelectList(_context.DanhMucs.ToList(), "MaDanhMuc", "TenDanhMuc");
            ViewBag.ThuongHieuList = new SelectList(_context.ThuongHieus.ToList(), "MaThuongHieu", "TenThuongHieu");

            return View("~/Adminboot/Admin/Views/Sanpham/Create.cshtml");
        }

        // POST: Admin/Sanpham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPhamViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (model.ImageUpload != null && model.ImageUpload.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageUpload.CopyToAsync(fileStream);
                    }
                }

                // Chuyển từ ViewModel sang Entity
                var sanPham = new SanPham
                {
                    TenSP = model.TenSP,
                    MoTa = model.MoTa,
                    DonGia = model.DonGia,
                    GiaGoc = model.GiaGoc,
                    SoLuong = model.SoLuong,
                    ImageURL = "/images/" + uniqueFileName,
                    MaDanhMuc = model.MaDanhMuc,
                    MaThuongHieu = model.MaThuongHieu,
                    ChieuRong = model.ChieuRong,
                    ChieuCao = model.ChieuCao,
                    ChieuSau = model.ChieuSau,
                    NgayTao = DateTime.Now
                };

                _context.SanPhams.Add(sanPham);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // Load lại dropdown nếu ModelState không hợp lệ
            ViewBag.DanhMucList = new SelectList(_context.DanhMucs.ToList(), "MaDanhMuc", "TenDanhMuc", model.MaDanhMuc);
            ViewBag.ThuongHieuList = new SelectList(_context.ThuongHieus.ToList(), "MaThuongHieu", "TenThuongHieu", model.MaThuongHieu);

            return View(model);
        }

    }
}
