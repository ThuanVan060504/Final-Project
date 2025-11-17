using Final_Project.Models.Shop;
using Final_Project.Models.ViewModels;
using Microsoft.AspNetCore.Authorization; // Cần cho Authorize
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Final_Project.Controllers.Admin // (Kiểm tra lại namespace Controller của bạn)
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Giả sử bạn dùng Role "Admin"
    public class FlashSaleAdminController : Controller
    {
        private readonly AppDbContext _context;

        public FlashSaleAdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/FlashSaleAdmin
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var dotSales = await _context.DotFlashSale
                .Include(d => d.ChiTietFlashSales) // Nối bảng con để đếm SL
                .OrderByDescending(d => d.ThoiGianBatDau)
                .ToListAsync();

            // Map sang ViewModel
            var viewModel = dotSales.Select(d => new FlashSaleEventViewModel
            {
                MaDot = d.MaDot,
                TenDot = d.TenDot,
                ThoiGianBatDau = d.ThoiGianBatDau,
                ThoiGianKetThuc = d.ThoiGianKetThuc,
                IsActive = d.IsActive,
                SoLuongSanPham = d.ChiTietFlashSales.Count,
                TrangThai = d.IsActive == false ? "Đã tắt" :
                            (now < d.ThoiGianBatDau) ? "Sắp diễn ra" :
                            (now > d.ThoiGianKetThuc) ? "Đã kết thúc" : "Đang diễn ra"
            }).ToList();
            return View("~/Adminboot/Admin/Views/FlashSaleAdmin/Index.cshtml", viewModel);
        }

        // GET: Admin/FlashSaleAdmin/Create
        public IActionResult Create()
        {
            var model = new DotFlashSale
            {
                ThoiGianBatDau = DateTime.Now.AddHours(1),
                ThoiGianKetThuc = DateTime.Now.AddDays(1)
            };
            return View("~/Adminboot/Admin/Views/FlashSaleAdmin/Create.cshtml", model);
        }

        // POST: Admin/FlashSaleAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDot,MoTa,ThoiGianBatDau,ThoiGianKetThuc,IsActive")] DotFlashSale dotFlashSale)
        {
            if (dotFlashSale.ThoiGianKetThuc <= dotFlashSale.ThoiGianBatDau)
            {
                ModelState.AddModelError("ThoiGianKetThuc", "Thời gian kết thúc phải sau thời gian bắt đầu.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(dotFlashSale);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo đợt sale mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Adminboot/Admin/Views/FlashSaleAdmin/Create.cshtml", dotFlashSale);
        }

        // GET: Admin/FlashSaleAdmin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var dotFlashSale = await _context.DotFlashSale.FindAsync(id);
            if (dotFlashSale == null) return NotFound();

            return View("~/Adminboot/Admin/Views/FlashSaleAdmin/Edit.cshtml", dotFlashSale);
        }

        // POST: Admin/FlashSaleAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDot,TenDot,MoTa,ThoiGianBatDau,ThoiGianKetThuc,IsActive")] DotFlashSale dotFlashSale)
        {
            if (id != dotFlashSale.MaDot) return NotFound();

            if (dotFlashSale.ThoiGianKetThuc <= dotFlashSale.ThoiGianBatDau)
            {
                ModelState.AddModelError("ThoiGianKetThuc", "Thời gian kết thúc phải sau thời gian bắt đầu.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dotFlashSale);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DotFlashSale.Any(e => e.MaDot == dotFlashSale.MaDot)) return NotFound();
                    else throw;
                }
                TempData["SuccessMessage"] = "Cập nhật đợt sale thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Adminboot/Admin/Views/FlashSaleAdmin/Edit.cshtml", dotFlashSale);
        }

        // POST: Admin/FlashSaleAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dotFlashSale = await _context.DotFlashSale.FindAsync(id);
            if (dotFlashSale != null)
            {
                _context.DotFlashSale.Remove(dotFlashSale);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa đợt sale.";
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var dotSale = await _context.DotFlashSale.FindAsync(id);
            if (dotSale == null) return NotFound();

            var productsInSale = await _context.ChiTietFlashSale
                .Include(ct => ct.SanPham)
                .Where(ct => ct.MaDot == id)
                .Select(ct => new ChiTietFlashSaleViewModel
                {
                    MaChiTiet = ct.MaChiTiet,
                    MaSP = ct.MaSP,
                    TenSP = ct.SanPham.TenSP,
                    ImageURL = ct.SanPham.ImageURL,
                    GiaGoc = ct.SanPham.DonGia,
                    PhanTramGiam = ct.PhanTramGiam,
                    GiaSauGiam = ct.GiaSauGiam,
                    SoLuongGiamGia = ct.SoLuongGiamGia,
                    SoLuongDaBan = ct.SoLuongDaBan
                })
                .ToListAsync();

            var spDaCoTrongSale = productsInSale.Select(p => p.MaSP).ToList();
            var sanPhamList = await _context.SanPhams
                .Where(sp => !spDaCoTrongSale.Contains(sp.MaSP))
                .OrderBy(sp => sp.TenSP)
                .ToListAsync();

            var viewModel = new FlashSaleManageViewModel
            {
                EventDetails = dotSale,
                ProductsInSale = productsInSale,
                SanPhamList = new SelectList(sanPhamList, "MaSP", "TenSP"),
                AddProductForm = new AddProductToSaleViewModel { MaDot = id.Value }
            };

            return View("~/Adminboot/Admin/Views/FlashSaleAdmin/Details.cshtml", viewModel);
        }

        // POST: Admin/FlashSaleAdmin/AddProductToSale (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductToSale([Bind(Prefix = "AddProductForm")] AddProductToSaleViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool isExist = await _context.ChiTietFlashSale
                    .AnyAsync(ct => ct.MaDot == model.MaDot && ct.MaSP == model.MaSP);

                if (isExist)
                {
                    TempData["ErrorMessage"] = "Sản phẩm này đã có trong đợt sale.";
                    return RedirectToAction(nameof(Details), new { id = model.MaDot });
                }

                var sanPham = await _context.SanPhams.FindAsync(model.MaSP);
                if (sanPham == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction(nameof(Details), new { id = model.MaDot });
                }

                var chiTiet = new ChiTietFlashSale
                {
                    MaDot = model.MaDot,
                    MaSP = model.MaSP,
                    PhanTramGiam = model.PhanTramGiam,
                    SoLuongGiamGia = model.SoLuongGiamGia,
                    SoLuongDaBan = 0,
                    GiaSauGiam = sanPham.DonGia * (1 - (model.PhanTramGiam / 100.0m))
                };

                _context.ChiTietFlashSale.Add(chiTiet);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã thêm sản phẩm '{sanPham.TenSP}' vào đợt sale.";
                return RedirectToAction(nameof(Details), new { id = model.MaDot });
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            return RedirectToAction(nameof(Details), new { id = model.MaDot });
        }

        // POST: Admin/FlashSaleAdmin/RemoveProductFromSale/123
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProductFromSale(int id) // id ở đây là MaChiTiet
        {
            var chiTiet = await _context.ChiTietFlashSale.FindAsync(id);
            if (chiTiet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm để xóa.";
                return RedirectToAction(nameof(Index));
            }

            int maDot = chiTiet.MaDot;
            _context.ChiTietFlashSale.Remove(chiTiet);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi đợt sale.";
            return RedirectToAction(nameof(Details), new { id = maDot });
        }

        // GET: /Admin/FlashSaleAdmin/GetProductPrice?maSP=5
        [HttpGet]
        public async Task<IActionResult> GetProductPrice(int maSP)
        {
            var sanPham = await _context.SanPhams.FindAsync(maSP);
            if (sanPham == null)
            {
                return NotFound();
            }
            return Ok(new { donGia = sanPham.DonGia });
        }
    }
}