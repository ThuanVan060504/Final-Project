using Final_Project.Models; // thay bằng namespace thật của bạn
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using SelectPdf;
using Microsoft.AspNetCore.Http;
using System.IO;
namespace Final_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongKeController : BaseAdminController
    {
        private readonly AppDbContext _context;

        public ThongKeController(AppDbContext context) : base(context)
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

        [HttpPost]
        public IActionResult UploadImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("Không có file được chọn.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            var relativePath = $"/uploads/{fileName}";
            return Json(new { filePath = relativePath });
        }

        [HttpGet]
        public IActionResult ExportPdf(DateTime? tuNgay, DateTime? denNgay)
        {
            var donHangs = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .Where(d => d.TrangThaiDonHang == "DaGiao");

            if (tuNgay.HasValue)
                donHangs = donHangs.Where(d => d.NgayDat >= tuNgay.Value);
            if (denNgay.HasValue)
                donHangs = donHangs.Where(d => d.NgayDat <= denNgay.Value);

            var chiTietSP = donHangs
                .SelectMany(d => d.ChiTietDonHangs)
                .Select(ct => new
                {
                    NgayDat = ct.DonHang.NgayDat,
                    TenSP = ct.SanPham.TenSP,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.SoLuong * ct.DonGia
                })
                .OrderByDescending(x => x.NgayDat)
                .ToList();

            var tongDoanhThu = chiTietSP.Sum(x => x.ThanhTien);

            // Tạo HTML giống CGV
            string html = $@"
<html>
<head>
<meta charset='UTF-8'>
<style>
    body {{ font-family: DejaVu Sans, sans-serif; }}
    h2 {{ text-align:center; color:#0000A0; }}
    table {{ border-collapse: collapse; width: 100%; margin-top: 15px; }}
    table, th, td {{ border: 1px solid black; }}
    th, td {{ padding: 8px; text-align: center; }}
    .header {{ width:100%; margin-bottom:20px; }}
    .header div {{ display:inline-block; width:49%; }}
    .right {{ text-align:right; }}
    .bold {{ font-weight:bold; }}
</style>
</head>
<body>
    <div class='header'>
        <div><img src='https://imgbb.com/][img]https://i.ibb.co/MDYmPT8L/Logo.jpg[/img][/' height='30'/></div>
        <div class='right'>{DateTime.Now:dd/MM/yyyy hh:mm:ss tt}</div>
    </div>

    <h2>DOANH THU</h2>

    <div class='header'>
        <div>Từ Ngày: {(tuNgay.HasValue ? tuNgay.Value.ToString("dd/MM/yyyy") : "")}</div>
        <div class='right'>Đến Ngày: {(denNgay.HasValue ? denNgay.Value.ToString("dd/MM/yyyy") : "")}</div>
    </div>

    <table>
        <thead>
            <tr>
                <th>STT</th>
                <th>Tên Sản Phẩm</th>
                <th>Ngày Đặt</th>
                <th>Giờ Đặt</th>
                <th>Số Lượng</th>
                <th>Thành Tiền (VND)</th>
            </tr>
        </thead>
        <tbody>";

            int stt = 1;
            foreach (var item in chiTietSP)
            {
                html += $@"
            <tr>
                <td>{stt++}</td>
                <td>{item.TenSP}</td>
                <td>{item.NgayDat:dd/MM/yyyy}</td>
                <td>{item.NgayDat:HH:mm:ss}</td>
                <td>{item.SoLuong}</td>
                <td>{item.ThanhTien.ToString("N0")}</td>
            </tr>";
            }

            html += $@"
        </tbody>
    </table>

    <h4 style='text-align:right; margin-top:10px;'>Tổng Doanh Thu: {tongDoanhThu.ToString("N0")} đ</h4>
</body>
</html>";

            HtmlToPdf converter = new HtmlToPdf();
            PdfDocument doc = converter.ConvertHtmlString(html);

            byte[] pdf = doc.Save();
            doc.Close();

            return File(pdf, "application/pdf", $"ThongKe_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        // View chính
        public IActionResult Index()
        {
            return View("~/Adminboot/Admin/Views/ThongKe/Index.cshtml");
        }
    }
}
