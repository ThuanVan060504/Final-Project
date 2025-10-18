using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Final_Project.Controllers.Menu
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Nạp dữ liệu danh mục dùng chung cho menu trong Layout
        /// </summary>
        private void LoadCommonData()
        {
            var danhMucs = _context.DanhMucs
                .Include(d => d.SanPhams)
                .ToList();

            ViewBag.DanhMucs = danhMucs;
        }

        public IActionResult Index()
        {
            LoadCommonData();

            // Thông tin tài khoản đăng nhập
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }

            // ==============================
            // Top sản phẩm yêu thích
            // ==============================
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

            // ==============================
            // Top sản phẩm bán chạy
            // ==============================
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



            // ==============================
            // Sản phẩm mới nhất
            // ==============================
            var sanPhamsMoi = _context.SanPhams
                .Include(sp => sp.DanhMuc)
                .Include(sp => sp.ThuongHieu)
                .OrderByDescending(sp => sp.MaSP)
                .Take(6)
                .ToList();

            return View(sanPhamsMoi);
        }

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
