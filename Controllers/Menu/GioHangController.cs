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
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
            ViewBag.Avatar = taiKhoan?.Avatar;
            ViewBag.HoTen = taiKhoan?.HoTen;
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

        [HttpPost]
        public IActionResult CapNhatSoLuong(int maSP, int soLuong)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            var item = _context.GioHangs.FirstOrDefault(g => g.MaTK == maTK && g.MaSP == maSP);
            if (item != null)
            {
                item.SoLuong = soLuong;
                item.NgayThem = DateTime.Now;
                _context.SaveChanges();
                TempData["Success"] = "Đã cập nhật số lượng.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult XoaKhoiGio(int maSP)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            var item = _context.GioHangs.FirstOrDefault(g => g.MaTK == maTK && g.MaSP == maSP);
            if (item != null)
            {
                _context.GioHangs.Remove(item);
                _context.SaveChanges();
                TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ.";
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult ThanhToan()
        {
            // 1. Lấy tài khoản đang đăng nhập
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return RedirectToAction("Login", "Auth");

            // 2. Lấy địa chỉ mặc định của người dùng
            var diaChiMacDinh = _context.DiaChiNguoiDungs
                .FirstOrDefault(d => d.MaTK == maTK && d.MacDinh);

            if (diaChiMacDinh == null)
            {
                TempData["Success"] = "Bạn chưa có địa chỉ mặc định. Vui lòng thêm địa chỉ trước khi thanh toán.";
                return RedirectToAction("Index");
            }

            // 3. Lấy danh sách sản phẩm trong giỏ hàng
            var gioHang = _context.GioHangs
                .Where(g => g.MaTK == maTK)
                .ToList();

            if (!gioHang.Any())
            {
                TempData["Success"] = "Không có sản phẩm trong giỏ hàng.";
                return RedirectToAction("Index");
            }

            // 4. Tính tổng tiền
            decimal tongTien = (from gh in gioHang
                                join sp in _context.SanPhams on gh.MaSP equals sp.MaSP
                                select gh.SoLuong * sp.DonGia).Sum();

            // 5. Tạo đơn hàng mới
            var donHang = new DonHang
            {
                MaTK = maTK.Value,
                MaDiaChi = diaChiMacDinh.MaDiaChi, // ✅ đúng địa chỉ mặc định
                NgayDat = DateTime.Now,
                NgayYeuCau = DateTime.Now.AddDays(3),
                PhiVanChuyen = 0,
                TongTien = tongTien,
                GiamGia = 0,
                PhuongThucThanhToan = "COD",
                TrangThaiThanhToan = "DaThanhToan",
                TrangThaiDonHang = "DangXuLy",
                GhiChu = null
            };

            _context.DonHangs.Add(donHang);
            _context.SaveChanges(); // lấy được MaDonHang

            // 6. Thêm chi tiết đơn hàng
            foreach (var item in gioHang)
            {
                var sanPham = _context.SanPhams.First(sp => sp.MaSP == item.MaSP);
                var chiTiet = new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSP = item.MaSP,
                    SoLuong = item.SoLuong,
                    DonGia = sanPham.DonGia
                };
                _context.ChiTietDonHangs.Add(chiTiet);
            }

            // 7. Xóa giỏ hàng
            _context.GioHangs.RemoveRange(gioHang);

            // 8. Lưu thay đổi
            _context.SaveChanges();

            TempData["Success"] = "Thanh toán thành công! Đơn hàng của bạn đang được xử lý.";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult LayDiaChiMacDinh()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return Json(new { success = false });

            var diaChi = _context.DiaChiNguoiDungs
                .Where(d => d.MaTK == maTK && d.MacDinh)
                .FirstOrDefault();

            if (diaChi == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                diaChiChiTiet = diaChi.DiaChiChiTiet,
                phuongXa = diaChi.PhuongXa,
                quanHuyen = diaChi.QuanHuyen,
                tinhTP = diaChi.TinhTP
            });
        }
        [HttpPost]
        public IActionResult CapNhatNhieu(List<int> chonSP, Dictionary<int, int> soLuong)
        {
            foreach (var maSP in chonSP)
            {
                if (soLuong.ContainsKey(maSP))
                {
                    CapNhatSoLuong(maSP, soLuong[maSP]); // xử lý cập nhật giỏ hàng
                }
            }
            TempData["Success"] = "Đã cập nhật số lượng thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult XoaNhieu(List<int> chonSP)
        {
            foreach (var maSP in chonSP)
            {
                XoaKhoiGio(maSP); // xử lý xóa
            }
            TempData["Success"] = "Đã xóa sản phẩm đã chọn!";
            return RedirectToAction("Index");
        }

    }
}