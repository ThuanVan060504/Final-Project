// Controllers/ThanhToanController.cs
using Microsoft.AspNetCore.Mvc;
using Final_Project.Models.Shop;
using System.Linq;

namespace Final_Project.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly AppDbContext _context;

        public ThanhToanController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult ThanhToan(List<int> chonSP, string paymentMethod)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return RedirectToAction("Login", "Auth");

            if (chonSP == null || !chonSP.Any())
            {
                TempData["Success"] = "Vui lòng chọn sản phẩm để thanh toán.";
                return RedirectToAction("Index", "GioHang");
            }

            var diaChiMacDinh = _context.DiaChiNguoiDungs
                .FirstOrDefault(d => d.MaTK == maTK && d.MacDinh);

            if (diaChiMacDinh == null)
            {
                TempData["Success"] = "Bạn chưa có địa chỉ mặc định.";
                return RedirectToAction("Index", "GioHang");
            }

            var gioHang = _context.GioHangs
                .Where(g => g.MaTK == maTK && chonSP.Contains(g.MaSP))
                .ToList();

            if (!gioHang.Any())
            {
                TempData["Success"] = "Không có sản phẩm nào được chọn.";
                return RedirectToAction("Index", "GioHang");
            }

            decimal tongTien = gioHang.Sum(gh =>
            {
                var sanPham = _context.SanPhams.First(sp => sp.MaSP == gh.MaSP);
                return gh.SoLuong * sanPham.DonGia;
            });

            string trangThaiThanhToan;
            string phuongThuc;

            switch (paymentMethod)
            {
                case "COD":
                    trangThaiThanhToan = "ChuaThanhToan";
                    phuongThuc = "Thanh toán khi nhận hàng";
                    break;
                case "ChuyenKhoan":
                    trangThaiThanhToan = "DaThanhToan";
                    phuongThuc = "Chuyển khoản";
                    break;
                default:
                    TempData["Success"] = "Phương thức thanh toán không hợp lệ.";
                    return RedirectToAction("XacNhanThanhToan", new { chonSP = string.Join(",", chonSP) });
            }

            var donHang = new DonHang
            {
                MaTK = maTK.Value,
                MaDiaChi = diaChiMacDinh.MaDiaChi,
                NgayDat = DateTime.Now,
                NgayYeuCau = DateTime.Now.AddDays(3),
                PhiVanChuyen = 17000,
                TongTien = tongTien + 17000,
                GiamGia = 0,
                PhuongThucThanhToan = phuongThuc,
                TrangThaiThanhToan = trangThaiThanhToan,
                TrangThaiDonHang = "DangXuLy",
                GhiChu = null
            };

            _context.DonHangs.Add(donHang);
            _context.SaveChanges();

            foreach (var item in gioHang)
            {
                var sanPham = _context.SanPhams.First(sp => sp.MaSP == item.MaSP);

                if (item.SoLuong > sanPham.SoLuong)
                {
                    TempData["Success"] = $"Sản phẩm {sanPham.TenSP} không đủ hàng.";
                    return RedirectToAction("Index", "GioHang");
                }

                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSP = item.MaSP,
                    SoLuong = item.SoLuong,
                    DonGia = sanPham.DonGia
                });

                sanPham.SoLuong -= item.SoLuong;
            }

            _context.GioHangs.RemoveRange(gioHang);
            _context.SaveChanges();

            TempData["Success"] = "Thanh toán thành công!";
            return RedirectToAction("Index", "GioHang");
        }

       [HttpPost]
public IActionResult XacNhanThanhToan(List<int> chonSP)
{
    int? maTK = HttpContext.Session.GetInt32("MaTK");
    if (maTK == null || chonSP == null || !chonSP.Any())
        return RedirectToAction("Index", "GioHang");

    var diaChiMacDinh = _context.DiaChiNguoiDungs
        .FirstOrDefault(d => d.MaTK == maTK && d.MacDinh);

    if (diaChiMacDinh == null)
    {
        TempData["Error"] = "⚠ Bạn chưa thiết lập địa chỉ mặc định. Vui lòng cập nhật trước khi thanh toán.";
        return RedirectToAction("DanhSachDiaChi", "User");
    }

    var gioHang = (from gh in _context.GioHangs
                   join sp in _context.SanPhams on gh.MaSP equals sp.MaSP
                   where gh.MaTK == maTK && chonSP.Contains(sp.MaSP)
                   select new GioHangViewModel
                   {
                       MaSP = sp.MaSP,
                       TenSP = sp.TenSP,
                       SoLuong = gh.SoLuong,
                       DonGia = sp.DonGia,
                       ImageURL = sp.ImageURL
                   }).ToList();

    ViewBag.DiaChi = diaChiMacDinh;
    ViewBag.TongTien = gioHang.Sum(g => g.ThanhTien);

    return View("Index", gioHang); // ✅ dùng Index.cshtml
}

    }
}
