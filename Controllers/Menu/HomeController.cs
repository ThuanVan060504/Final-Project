using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
            // Gọi hàm load danh mục trước khi render view
            LoadCommonData();

            // Lấy thông tin tài khoản đang đăng nhập
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }

            // Sản phẩm được yêu thích nhiều nhất
            var topYeuThich = _context.SanPhamYeuThichs
                .GroupBy(y => y.MaSP)
                .OrderByDescending(g => g.Count())
                .Select(g => new { MaSP = g.Key, SoLuongYeuThich = g.Count() })
                .Take(6)
                .ToList();

            var spYeuThichOrdered = topYeuThich
                .Join(_context.SanPhams.Include(s => s.DanhMuc),
                    yt => yt.MaSP,
                    sp => sp.MaSP,
                    (yt, sp) => new { SanPham = sp, SoLuongYeuThich = yt.SoLuongYeuThich })
                .Take(6)
                .ToList();

            ViewBag.SanPhamYeuThich = spYeuThichOrdered;

            // Sản phẩm bán chạy nhất
            var topBanChay = _context.ChiTietDonHangs
                .GroupBy(c => c.MaSP)
                .OrderByDescending(g => g.Sum(x => x.SoLuong))
                .Select(g => new { MaSP = g.Key, SoLuongBan = g.Sum(x => x.SoLuong) })
                .Take(6)
                .ToList();

            var spBanChayOrdered = topBanChay
                .Join(_context.SanPhams.Include(s => s.DanhMuc),
                    bc => bc.MaSP,
                    sp => sp.MaSP,
                    (bc, sp) => new { SanPham = sp, SoLuongDaBan = bc.SoLuongBan })
                .Take(6)
                .ToList();

            ViewBag.SanPhamBanChay = spBanChayOrdered;

            // Lấy danh sách sản phẩm mới nhất
            var sanPhams = _context.SanPhams
                .Include(sp => sp.DanhMuc)
                .Include(p => p.ThuongHieu)
                .OrderByDescending(sp => sp.MaSP)
                .Take(6)
                .ToList();

            return View(sanPhams);
        }

        public IActionResult Details(int id)
        {
            LoadCommonData();

            var product = _context.SanPhams
                .FirstOrDefault(p => p.MaSP == id);

            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}
