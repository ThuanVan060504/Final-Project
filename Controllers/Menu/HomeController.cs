using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
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
            // Nếu đang là Admin mà quay về /Home → auto logout + clear cookie
            // ===============================
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Admin")
            {
                // Xóa toàn bộ session
                HttpContext.Session.Clear();

                // Đăng xuất khỏi cookie authentication (xoá MyCookie & Google cookie)
                await HttpContext.SignOutAsync("MyCookie");
                await HttpContext.SignOutAsync();

                // Xóa luôn cookie cũ (phòng trường hợp còn cookie trình duyệt)
                if (Request.Cookies != null)
                {
                    foreach (var cookie in Request.Cookies.Keys)
                    {
                        Response.Cookies.Delete(cookie);
                    }
                }

                // Redirect về Home gốc
                return RedirectToAction("Index", "Home");
            }

            // ===============================
            // Load dữ liệu chung cho trang Home
            // ===============================
            LoadCommonData();

            // Thông tin tài khoản đăng nhập
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }
            var now = DateTime.Now;
            var availableVouchers = await _context.Vouchers
                .Where(v => v.IsActive == true &&
                            v.NgayBatDau <= now &&
                            v.NgayKetThuc >= now &&
                            (v.SoLuongToiDa == null || v.SoLuongDaDung < v.SoLuongToiDa))
                .OrderBy(v => v.NgayKetThuc) // Ưu tiên cái sắp hết hạn
                .ToListAsync();

            ViewBag.AvailableVouchers = availableVouchers;
            // ===============================
            // Top sản phẩm yêu thích
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
            // Top sản phẩm bán chạy
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
            // Sản phẩm mới nhất
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
