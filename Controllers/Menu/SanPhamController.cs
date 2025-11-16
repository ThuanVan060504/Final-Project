using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Final_Project.Controllers.Menu
{
    public class SanPhamController : Controller
    {
        private readonly AppDbContext _context;

        public SanPhamController(AppDbContext context)
        {
            _context = context;
        }

        // 🟢 Lấy thông tin user + danh mục chung
        private void LoadCommonData()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }

            // ✅ Lấy tất cả danh mục kèm số sản phẩm
            ViewBag.DanhMucs = _context.DanhMucs
                .Include(d => d.SanPhams)
                .ToList();
        }

        public async Task<IActionResult> Index(string category, string search, string sort, int page = 1)
        {
            LoadCommonData(); // gọi hàm chung

            int pageSize = 6;

            var products = _context.SanPhams
                .Include(p => p.DanhMuc)
                .AsQueryable();

            // (Code lọc, tìm kiếm, sắp xếp... giữ nguyên)
            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.DanhMuc.TenDanhMuc == category);
            }
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.TenSP.Contains(search));
            }
            switch (sort)
            {
                case "asc":
                    products = products.OrderBy(p => p.DonGia);
                    break;
                case "desc":
                    products = products.OrderByDescending(p => p.DonGia);
                    break;
                default:
                    products = products.OrderBy(p => p.MaSP);
                    break;
            }

            int totalItems = await products.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedProducts = await products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ======================================================
            // ✅ SỬA LỖI Ở ĐÂY: Lọc MaSP != null và dùng g.Key.Value
            // ======================================================
            var soldQuantities = await _context.ChiTietDonHangs
                .Where(ct => ct.MaSP.HasValue) // Lọc bỏ các chi tiết đơn hàng không có MaSP
                .GroupBy(ct => ct.MaSP.Value) // Group theo MaSP (int)
                .Select(g => new { MaSP = g.Key, SoLuongDaBan = g.Sum(ct => ct.SoLuong) })
                .ToDictionaryAsync(x => x.MaSP, x => x.SoLuongDaBan);

            // ======================================================

            // ✅ SỬA LỖI: Thêm code truy vấn Flash Sale
            var now = DateTime.Now;
            var activeFlashSales = await _context.ChiTietFlashSale
                .Include(ct => ct.DotFlashSale)
                .Where(ct =>
                    ct.DotFlashSale.IsActive == true &&
                    now >= ct.DotFlashSale.ThoiGianBatDau &&
                    now <= ct.DotFlashSale.ThoiGianKetThuc
                )
                .ToDictionaryAsync(ct => ct.MaSP, ct => ct);

            ViewBag.ActiveFlashSales = activeFlashSales;

            ViewBag.SoLuongDaBan = soldQuantities;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedProducts);
        }

        public async Task<IActionResult> Details(int id)
        {
            LoadCommonData();

            var sp = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .FirstOrDefaultAsync(s => s.MaSP == id);

            if (sp == null) return NotFound();

            // FlashSale (Code này đã đúng từ trước)
            var now = DateTime.Now;
            var chiTietSale = await _context.ChiTietFlashSale
                .Include(ct => ct.DotFlashSale)
                .FirstOrDefaultAsync(ct =>
                    ct.MaSP == id &&
                    ct.DotFlashSale.IsActive == true &&
                    now >= ct.DotFlashSale.ThoiGianBatDau &&
                    now <= ct.DotFlashSale.ThoiGianKetThuc
                );

            if (chiTietSale != null)
            {
                ViewBag.FlashSale = chiTietSale;
                ViewBag.GiaSauGiam = chiTietSale.GiaSauGiam;
            }
            else
            {
                ViewBag.FlashSale = null;
            }

            // Sản phẩm tương tự
            var tuongTu = await _context.SanPhams
                .Where(s => s.MaDanhMuc == sp.MaDanhMuc && s.MaSP != sp.MaSP)
                .Take(4)
                .ToListAsync();

            // Đánh giá
            var danhGiaList = await _context.DanhGias
                .Where(d => d.SanPhamId == id)
                .OrderByDescending(d => d.ThoiGian)
                .ToListAsync();

            double diemTrungBinh = danhGiaList.Any()
                ? Math.Round(danhGiaList.Average(d => d.Diem), 1)
                : 0;

            // ======================================================
            // ✅ SỬA LỖI Ở ĐÂY: Lọc MaSP != null và dùng g.Key.Value
            // ======================================================
            var soldQuantities = await _context.ChiTietDonHangs
                .Where(ct => ct.MaSP.HasValue) // Lọc bỏ các chi tiết đơn hàng không có MaSP
                .GroupBy(ct => ct.MaSP.Value) // Group theo MaSP (int)
                .Select(g => new { MaSP = g.Key, SoLuongDaBan = g.Sum(ct => ct.SoLuong) })
                .ToDictionaryAsync(x => x.MaSP, x => x.SoLuongDaBan);

            ViewBag.SoLuongDaBan = soldQuantities;
            // ======================================================


            ViewBag.DiemTrungBinh = diemTrungBinh;
            ViewBag.SanPhamTuongTu = tuongTu;
            ViewBag.DanhGiaList = danhGiaList;

            return View(sp);
        }

        [HttpPost]
        public async Task<IActionResult> GuiDanhGia(int SanPhamId, int Diem, string BinhLuan)
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Sửa: Dùng Auth Controller (theo file GioHangController)
                return RedirectToAction("Login", "Auth");
            }

            var tenNguoiDung = User.Claims.FirstOrDefault(c => c.Type == "HoTen")?.Value ?? User.Identity.Name ?? "Ẩn danh";

            var danhGia = new DanhGia
            {
                SanPhamId = SanPhamId,
                TenNguoiDung = tenNguoiDung,
                Diem = Diem,
                BinhLuan = BinhLuan,
                ThoiGian = DateTime.Now
            };

            _context.DanhGias.Add(danhGia);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = SanPhamId });
        }

        public async Task<IActionResult> Search(string keyword, int page = 1)
        {
            LoadCommonData();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return RedirectToAction("Index");
            }

            int pageSize = 6;

            var products = _context.SanPhams
                .Include(p => p.DanhMuc)
                .Where(p => p.TenSP.Contains(keyword))
                .OrderBy(p => p.MaSP);

            int totalItems = await products.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedProducts = await products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ======================================================
            // ✅ SỬA LỖI Ở ĐÂY: Lọc MaSP != null và dùng g.Key.Value
            // ======================================================
            var soldQuantities = await _context.ChiTietDonHangs
                .Where(ct => ct.MaSP.HasValue) // Lọc bỏ các chi tiết đơn hàng không có MaSP
                .GroupBy(ct => ct.MaSP.Value) // Group theo MaSP (int)
                .Select(g => new { MaSP = g.Key, SoLuongDaBan = g.Sum(ct => ct.SoLuong) })
                .ToDictionaryAsync(x => x.MaSP, x => x.SoLuongDaBan);

            // ======================================================

            // ✅ SỬA LỖI: Thêm code truy vấn Flash Sale cho trang Search
            var now = DateTime.Now;
            var activeFlashSales = await _context.ChiTietFlashSale
                .Include(ct => ct.DotFlashSale)
                .Where(ct =>
                    ct.DotFlashSale.IsActive == true &&
                    now >= ct.DotFlashSale.ThoiGianBatDau &&
                    now <= ct.DotFlashSale.ThoiGianKetThuc
                )
                .ToDictionaryAsync(ct => ct.MaSP, ct => ct);

            ViewBag.ActiveFlashSales = activeFlashSales;
            // ======================================================

            ViewBag.SoLuongDaBan = soldQuantities;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword;

            return View("Index", pagedProducts);
        }

        [HttpGet]
        public async Task<IActionResult> QuickView(int id)
        {
            var sanPham = await _context.SanPhams
                                  .Include(sp => sp.DanhMuc)
                                  .FirstOrDefaultAsync(sp => sp.MaSP == id);

            if (sanPham == null)
            {
                return NotFound("<p class='text-danger text-center'>Không tìm thấy sản phẩm.</p>");
            }

            // ✅ SỬA LỖI: Thêm code kiểm tra Flash Sale cho QuickView
            var now = DateTime.Now;
            var chiTietSale = await _context.ChiTietFlashSale
                .Include(ct => ct.DotFlashSale)
                .FirstOrDefaultAsync(ct =>
                    ct.MaSP == id &&
                    ct.DotFlashSale.IsActive == true &&
                    now >= ct.DotFlashSale.ThoiGianBatDau &&
                    now <= ct.DotFlashSale.ThoiGianKetThuc
                );

            if (chiTietSale != null)
            {
                ViewBag.FlashSale = chiTietSale;
                ViewBag.GiaSauGiam = chiTietSale.GiaSauGiam;
            }

            return PartialView("_QuickViewPartial", sanPham);
        }
    }
}