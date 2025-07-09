using Microsoft.AspNetCore.Mvc;
using Final_Project.Models;
using System.Linq;

namespace Final_Project.Controllers
{
    public class ProductController : Controller
    {
        public static List<SanPham> Products = new List<SanPham>
        {
            new SanPham { MaSP = 1, TenSP = "Gạch lát nền Viglacera 60x60", DanhMuc = "Gạch ốp lát", DonGia = 175000, MoTa = "Bề mặt nhám nhẹ chống trơn, tông xám hiện đại.", ChiTiet = "Gạch lát nền Viglacera 60x60 là lựa chọn lý tưởng...", ImageURL = "/images/product1.jpg" },
            // 🔻 Các sản phẩm còn lại thì thêm tiếp như em đã viết
        };

        public IActionResult Index(string? danhmuc, string? timkiem, string? sapxep)
        {
            var sanpham = Products.AsQueryable();

            if (!string.IsNullOrEmpty(danhmuc))
            {
                sanpham = sanpham.Where(p => p.DanhMuc != null && p.DanhMuc.Equals(danhmuc, StringComparison.OrdinalIgnoreCase));
                ViewBag.DanhMuc = danhmuc;
            }

            if (!string.IsNullOrEmpty(timkiem))
            {
                sanpham = sanpham.Where(p => p.TenSP != null && p.TenSP.Contains(timkiem, StringComparison.OrdinalIgnoreCase));
                ViewBag.TimKiem = timkiem;
            }

            if (!string.IsNullOrEmpty(sapxep))
            {
                if (sapxep == "asc")
                    sanpham = sanpham.OrderBy(p => p.DonGia);
                else if (sapxep == "desc")
                    sanpham = sanpham.OrderByDescending(p => p.DonGia);

                ViewBag.SapXep = sapxep;
            }

            return View(sanpham.ToList());
        }

        public IActionResult Details(int id)
        {
            var sp = Products.FirstOrDefault(p => p.MaSP == id);
            if (sp == null)
                return NotFound();

            return View(sp);
        }

        public IActionResult DanhMuc(string name)
        {
            var danhSach = Products.Where(p => p.DanhMuc == name).ToList();
            ViewBag.DanhMuc = name;
            return View("DanhMuc", danhSach);
        }
    }
}
