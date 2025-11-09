// Controllers/Menu/HomeController.cs
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic; // <-- Đảm bảo đã using
using System; // <-- Đảm bảo đã using

namespace Final_Project.Controllers.Menu
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        private void LoadCommonData()
        {
            var danhMucs = _context.DanhMucs
                .Include(d => d.SanPhams)
                .ToList();

            ViewBag.DanhMucs = danhMucs;
        }

        // ===============================
        // 🏠 Trang chủ
        // ===============================
        public async Task<IActionResult> Index()
        {
            // ===============================
            // Auto-logout Admin
            // ===============================
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Admin")
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync("MyCookie");
                await HttpContext.SignOutAsync();
                if (Request.Cookies != null)
                {
                    foreach (var cookie in Request.Cookies.Keys)
                    {
                        Response.Cookies.Delete(cookie);
                    }
                }
                return RedirectToAction("Index", "Home");
            }

            // ===============================
            // Load dữ liệu chung
            // ===============================
            LoadCommonData();

            // ========================================================
            // ✅ LOGIC MỚI: LẤY VOUCHER ĐÃ LƯU VÀ ĐÃ SỬ DỤNG
            // ========================================================

            // Thông tin tài khoản đăng nhập
            int? maTK = HttpContext.Session.GetInt32("MaTK");

            // 1. Chuẩn bị 2 HashSet
            var savedVoucherIds = new HashSet<int>(); // Danh sách đã LƯU
            var usedVoucherIds = new HashSet<int>();  // Danh sách đã SỬ DỤNG

            if (maTK != null)
            {
                var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;

                // 2. Lấy danh sách ID voucher đã LƯU
                savedVoucherIds = await _context.TaiKhoanVouchers
                    .Where(tv => tv.MaTK == maTK.Value)
                    .Select(tv => tv.MaVoucherID)
                    .ToHashSetAsync();

                // 3. Lấy danh sách ID voucher đã SỬ DỤNG (cho các đơn hàng không bị hủy)
                usedVoucherIds = await _context.DonHangs
                    .Where(d => d.MaTK == maTK.Value &&
                                d.MaVoucherID != null &&
                                d.TrangThaiDonHang != "HuyDon") // Quan trọng: Chỉ tính đơn thành công
                    .Select(d => d.MaVoucherID.Value) // .Value vì MaVoucherID là nullable
                    .Distinct()
                    .ToHashSetAsync();
            }

            // 4. Gửi cả 2 HashSet sang View
            ViewBag.SavedVoucherIds = savedVoucherIds;
            ViewBag.UsedVoucherIds = usedVoucherIds;
            // ========================================================


            // ===============================
            // Tải danh sách voucher CÓ SẴN (Không đổi)
            // ===============================
            var now = DateTime.Now;
            var availableVouchers = await _context.Vouchers
                .Where(v => v.IsActive == true &&
                            v.NgayBatDau <= now &&
                            v.NgayKetThuc >= now &&
                            (v.SoLuongToiDa == null || v.SoLuongDaDung < v.SoLuongToiDa))
                .OrderBy(v => v.NgayKetThuc)
                .ToListAsync();

            ViewBag.AvailableVouchers = availableVouchers;

            // ===============================
            // Top sản phẩm yêu thích (Không đổi)
            // ===============================
            var spYeuThichOrdered = _context.SanPhamYeuThichs
                .GroupBy(y => y.MaSP)
                .OrderByDescending(g => g.Count())
                .Select(g => new { MaSP = g.Key, SoLuongYeuThich = g.Count() })
                .Join(_context.SanPhams.Include(s => s.DanhMuc),
                    yt => yt.MaSP,
                    sp => sp.MaSP,
                    (yt, sp) => new { SanPham = sp, SoLuongYeuThich = yt.SoLuongYeuThich })
                .Take(6)
                .ToList();
            ViewBag.SanPhamYeuThich = spYeuThichOrdered;

            // ===============================
            // Top sản phẩm bán chạy (Không đổi)
            // ===============================
            var spBanChayOrdered = _context.ChiTietDonHangs
                .GroupBy(c => c.MaSP)
                .OrderByDescending(g => g.Sum(x => x.SoLuong))
                .Select(g => new { MaSP = g.Key, SoLuongBan = g.Sum(x => x.SoLuong) })
                .Join(_context.SanPhams.Include(s => s.DanhMuc),
                    bc => bc.MaSP,
                    sp => sp.MaSP,
                    (bc, sp) => new { SanPham = sp, SoLuongDaBan = bc.SoLuongBan })
                .Take(6)
                .ToList();
            ViewBag.SanPhamBanChay = spBanChayOrdered;

            // ===============================
            // Sản phẩm mới nhất (Không đổi)
            // ===============================
            var sanPhamsMoi = _context.SanPhams
                .Include(sp => sp.DanhMuc)
                .Include(sp => sp.ThuongHieu)
                .OrderByDescending(sp => sp.MaSP)
                .Take(6)
                .ToList();

            return View(sanPhamsMoi);
        }

        // ===============================
        // 📦 Chi tiết sản phẩm
        // ===============================
        public IActionResult Details(int id)
        {
            LoadCommonData();

            var product = _context.SanPhams
                .Include(sp => sp.DanhMuc)
                .Include(sp => sp.ThuongHieu)
                .FirstOrDefault(p => p.MaSP == id);

            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}