using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Final_Project.Controllers.CaNhan
{
    public class YeuThichController : Controller
    {
        private readonly AppDbContext _context;

        public YeuThichController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Them(int maSP)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return RedirectToAction("Login", "Auth");

            bool daTonTai = _context.SanPhamYeuThichs.Any(x => x.MaTK == maTK && x.MaSP == maSP);
            if (!daTonTai)
            {
                var yeuThich = new SanPhamYeuThich
                {
                    MaTK = maTK.Value,
                    MaSP = maSP,
                    NgayThem = DateTime.Now
                };
                _context.SanPhamYeuThichs.Add(yeuThich);
                _context.SaveChanges();
            }

            return RedirectToAction("Index", "SanPham");
        }

        public IActionResult DanhSach()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return RedirectToAction("Login", "Auth");

            var yeuThich = _context.SanPhamYeuThichs
                .Include(y => y.SanPham)
                .Where(y => y.MaTK == maTK)
                .Select(y => y.SanPham)
                .ToList();

            return View(yeuThich); // View này dùng lại view list sản phẩm cũng được
        }

        [HttpPost]
        public IActionResult BoYeuThich(int maSP)
        {
            var maTK = HttpContext.Session.GetInt32("MaTK");

            if (maTK == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var spYeuThich = _context.SanPhamYeuThichs
                .FirstOrDefault(y => y.MaSP == maSP && y.MaTK == maTK);

            if (spYeuThich != null)
            {
                _context.SanPhamYeuThichs.Remove(spYeuThich);
                _context.SaveChanges();
            }

            // Quay về trang hồ sơ người dùng
            return RedirectToAction("Profile", "User");
        }

    }
}
