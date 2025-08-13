using Final_Project.Areas.Admin.Controllers;
using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Final_Project.Adminboot.Admin.Controllers.KhachHang
{
    [Area("Admin")]
    public class AdminSanPhamController : BaseAdminController
    {
        private readonly AppDbContext _context;

        public AdminSanPhamController(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // Sản phẩm được mua nhiều nhất
        public IActionResult SanPhamMuaNhieu()
        {
            var sanPhamMuaNhieu = _context.ChiTietDonHangs
                .Include(c => c.SanPham)
                .GroupBy(c => new { c.MaSP, c.SanPham.TenSP, c.SanPham.ImageURL, c.SanPham.DonGia })
                .Select(g => new
                {
                    MaSP = g.Key.MaSP,
                    TenSP = g.Key.TenSP,
                    ImageURL = g.Key.ImageURL,
                    DonGia = g.Key.DonGia,
                    TongSoLuong = g.Sum(x => x.SoLuong)
                })
                .OrderByDescending(x => x.TongSoLuong)
                .ToList();

            return View("~/Adminboot/Admin/Views/KhachHang/SanPhamMuaNhieu.cshtml", sanPhamMuaNhieu);
        }

        // Sản phẩm được yêu thích
        public IActionResult SanPhamYeuThich()
        {
            var sanPhamYeuThich = _context.SanPhamYeuThichs
                .Include(y => y.SanPham)
                .GroupBy(y => new { y.MaSP, y.SanPham.TenSP, y.SanPham.ImageURL, y.SanPham.DonGia })
                .Select(g => new
                {
                    MaSP = g.Key.MaSP,
                    TenSP = g.Key.TenSP,
                    ImageURL = g.Key.ImageURL,
                    DonGia = g.Key.DonGia,
                    SoLanYeuThich = g.Count(),
                      // Đếm số đánh giá từ bảng DanhGias
            SoDanhGia = _context.DanhGias
                .Count(d => d.SanPhamId == g.Key.MaSP),

                    // Đếm số bình luận (có nội dung) từ DanhGias
                    SoBinhLuan = _context.DanhGias
                .Count(d => d.SanPhamId == g.Key.MaSP && !string.IsNullOrEmpty(d.BinhLuan))
                })
                .OrderByDescending(x => x.SoLanYeuThich)
                .ToList();

            return View("~/Adminboot/Admin/Views/KhachHang/SanPhamYeuThich.cshtml", sanPhamYeuThich);
        }
    }
}
