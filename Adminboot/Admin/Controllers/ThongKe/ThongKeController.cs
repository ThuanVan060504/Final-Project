using Microsoft.AspNetCore.Mvc;
using Final_Project.Models.Shop;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Final_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongKeController : Controller
    {
        private readonly AppDbContext _context;

        public ThongKeController(AppDbContext context)
        {
            _context = context;
        }

        // API: Top sản phẩm đã bán (đơn hàng đã được tạo, không phân biệt trạng thái)
        public IActionResult SanPhamDaBan()
        {
            var data = _context.ChiTietDonHangs
                .AsNoTracking()
                .Include(ct => ct.DonHang)
                .Include(ct => ct.SanPham)
                .Where(ct => ct.DonHang != null) // chỉ lấy chi tiết đơn hàng có đơn hàng tồn tại
                .GroupBy(ct => ct.SanPham.TenSP)
                .Select(g => new
                {
                    label = g.Key,
                    value = g.Sum(x => x.SoLuong)
                })
                .OrderByDescending(x => x.value)
                .Take(10)
                .ToList();

            return Json(data);
        }

        // API: Top sản phẩm yêu thích
        public IActionResult SanPhamYeuThich()
        {
            var data = _context.SanPhamYeuThichs
                .AsNoTracking()
                .Include(sp => sp.SanPham)
                .Where(sp => sp.SanPham != null)
                .GroupBy(sp => sp.SanPham.TenSP)
                .Select(g => new
                {
                    label = g.Key,
                    value = g.Count()
                })
                .OrderByDescending(x => x.value)
                .Take(10)
                .ToList();

            return Json(data);
        }

        // Trang thống kê
        public IActionResult Index()
        {
            return View("~/Adminboot/Admin/Views/ThongKe/Index.cshtml");
        }
    }
}
