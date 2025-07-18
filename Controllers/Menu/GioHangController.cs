// Controllers/GioHangController.cs
using Microsoft.AspNetCore.Mvc;
using Final_Project.Models.Shop;
using System.Linq;

namespace Final_Project.Controllers
{
    public class GioHangController : Controller
    {
        private readonly AppDbContext _context;

        public GioHangController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {

            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
            {
                return RedirectToAction("DangNhap", "Auth");
            }

            var gioHang = from gh in _context.GioHangs
                          join sp in _context.SanPhams on gh.MaSP equals sp.MaSP
                          where gh.MaTK == maTK
                          select new GioHangViewModel
                          {
                              MaSP = sp.MaSP,
                              TenSP = sp.TenSP,
                              SoLuong = gh.SoLuong,
                              DonGia = sp.DonGia,
                              ImageURL = sp.ImageURL
                          };

            return View(gioHang.ToList());
        }
        [HttpPost]
        public IActionResult ThemGioHang(int maSP, int soLuong = 1)
        {
            // Kiểm tra đăng nhập
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return RedirectToAction("Login", "Auth");


            // Kiểm tra sản phẩm đã tồn tại trong giỏ chưa
            var gioHangItem = _context.GioHangs
                .FirstOrDefault(g => g.MaTK == maTK && g.MaSP == maSP);

            if (gioHangItem != null)
            {
                // Đã có → tăng số lượng
                gioHangItem.SoLuong += soLuong;
                gioHangItem.NgayThem = DateTime.Now;
                _context.Update(gioHangItem);
            }
            else
            {
                // Chưa có → tạo mới
                var newItem = new GioHang
                {
                    MaTK = maTK.Value, // ✅ ép kiểu từ int? → int
                    MaSP = maSP,
                    SoLuong = soLuong,
                    NgayThem = DateTime.Now
                };
                _context.GioHangs.Add(newItem);
            }

            _context.SaveChanges();

            TempData["Success"] = "Đã thêm vào giỏ hàng!";
            return RedirectToAction("Index", "GioHang");

        }

    }
}
