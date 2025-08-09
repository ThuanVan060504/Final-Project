using Final_Project.Models; // thay bằng namespace thật của bạn
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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

        // API thống kê doanh thu theo ngày
        [HttpGet]
        public IActionResult DoanhThu(DateTime? tuNgay, DateTime? denNgay)
        {
            var query = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .Where(d => d.TrangThaiDonHang == "DaGiao");

            if (tuNgay.HasValue)
                query = query.Where(d => d.NgayDat >= tuNgay.Value);
            if (denNgay.HasValue)
                query = query.Where(d => d.NgayDat <= denNgay.Value);

            var data = query
      .GroupBy(d => d.NgayDat.Date)
      .Select(g => new
      {
          Ngay = g.Key,
          TongSoLuong = g.Sum(d => d.ChiTietDonHangs.Sum(ct => ct.SoLuong)),
          TongDoanhThu = g.Sum(d => d.ChiTietDonHangs.Sum(ct => ct.SoLuong * ct.DonGia))
      })
      .OrderBy(x => x.Ngay)
      .ToList();


            return Json(data);
        }

        // API lấy top 10 sản phẩm bán chạy
        [HttpGet]
        public IActionResult TopSanPham()
        {
            var data = _context.ChiTietDonHangs
                .Include(ct => ct.SanPham)
                .Include(ct => ct.DonHang)
                .Where(ct => ct.DonHang.TrangThaiDonHang == "DaGiao")
                .GroupBy(ct => ct.SanPham.TenSP)
                .Select(g => new
                {
                    TenSP = g.Key,
                    TongSoLuong = g.Sum(ct => ct.SoLuong)
                })
                .OrderByDescending(x => x.TongSoLuong)
                .Take(10)
                .ToList();

            return Json(data);
        }
        // API lấy danh sách chi tiết sản phẩm đã bán trong khoảng thời gian
        [HttpGet]
        public IActionResult ChiTietSanPham(DateTime? tuNgay, DateTime? denNgay)
        {
            var query = _context.ChiTietDonHangs
                .Include(ct => ct.SanPham)
                .Include(ct => ct.DonHang)
                .Where(ct => ct.DonHang.TrangThaiDonHang == "DaGiao");

            if (tuNgay.HasValue)
                query = query.Where(ct => ct.DonHang.NgayDat >= tuNgay.Value);
            if (denNgay.HasValue)
                query = query.Where(ct => ct.DonHang.NgayDat <= denNgay.Value);

            var data = query
                .Select(ct => new
                {
                    TenSP = ct.SanPham.TenSP,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.SoLuong * ct.DonGia,
                    NgayDat = ct.DonHang.NgayDat
                })
                .OrderByDescending(x => x.NgayDat)
                .ToList();

            return Json(data);
        }
        [HttpGet]
        public IActionResult ThongKeTongQuat(DateTime? tuNgay, DateTime? denNgay)
        {
            var donHangs = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .Where(d => d.TrangThaiDonHang == "DaGiao");

            if (tuNgay.HasValue)
                donHangs = donHangs.Where(d => d.NgayDat >= tuNgay.Value);
            if (denNgay.HasValue)
                donHangs = donHangs.Where(d => d.NgayDat <= denNgay.Value);

            var tongDoanhThu = donHangs.Sum(d => d.ChiTietDonHangs.Sum(ct => ct.SoLuong * ct.DonGia));
            var tongDonHang = donHangs.Count();
            var tongSanPham = donHangs.Sum(d => d.ChiTietDonHangs.Sum(ct => ct.SoLuong));
            var tongKhachHang = donHangs.Select(d => d.MaTK).Distinct().Count(); // nếu có field TaiKhoanId

            return Json(new
            {
                tongDoanhThu,
                tongDonHang,
                tongSanPham,
                tongKhachHang
            });
        }

        // View chính
        public IActionResult Index()
        {
            return View("~/Adminboot/Admin/Views/ThongKe/Index.cshtml");
        }
    }
}
