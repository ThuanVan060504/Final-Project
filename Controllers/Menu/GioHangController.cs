// Controllers/GioHangController.cs
using Microsoft.AspNetCore.Mvc;
using Final_Project.Models.Shop;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Final_Project.Controllers
{
    public class GioHangController : Controller
    {
        private readonly AppDbContext _context;

        public GioHangController(AppDbContext context)
        {
            _context = context;
        }

        private void CapNhatSessionSoLuong(int maTK)
        {
            int tongSoLuong = _context.GioHangs
                .Where(g => g.MaTK == maTK)
                .Sum(g => g.SoLuong);

            HttpContext.Session.SetInt32("SoLuongGioHang", tongSoLuong);
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
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return RedirectToAction("Login", "Auth");

            var gioHangItem = _context.GioHangs
                .FirstOrDefault(g => g.MaTK == maTK && g.MaSP == maSP);

            if (gioHangItem != null)
            {
                gioHangItem.SoLuong += soLuong;
                gioHangItem.NgayThem = DateTime.Now;
                _context.Update(gioHangItem);
            }
            else
            {
                var newItem = new GioHang
                {
                    MaTK = maTK.Value,
                    MaSP = maSP,
                    SoLuong = soLuong,
                    NgayThem = DateTime.Now
                };
                _context.GioHangs.Add(newItem);
            }

            _context.SaveChanges();
            CapNhatSessionSoLuong(maTK.Value);

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
                CapNhatSessionSoLuong(maTK.Value);
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
                CapNhatSessionSoLuong(maTK.Value);
                TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CapNhatNhieu(List<int> chonSP, Dictionary<int, int> soLuong)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            foreach (var maSP in chonSP)
            {
                if (soLuong.ContainsKey(maSP))
                {
                    var item = _context.GioHangs.FirstOrDefault(g => g.MaTK == maTK && g.MaSP == maSP);
                    if (item != null)
                    {
                        item.SoLuong = soLuong[maSP];
                        item.NgayThem = DateTime.Now;
                    }
                }
            }

            _context.SaveChanges();
            CapNhatSessionSoLuong(maTK.Value);
            TempData["Success"] = "Đã cập nhật số lượng thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult XoaNhieu(List<int> chonSP)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            foreach (var maSP in chonSP)
            {
                var item = _context.GioHangs.FirstOrDefault(g => g.MaTK == maTK && g.MaSP == maSP);
                if (item != null)
                {
                    _context.GioHangs.Remove(item);
                }
            }

            _context.SaveChanges();
            CapNhatSessionSoLuong(maTK.Value);
            TempData["Success"] = "Đã xóa sản phẩm đã chọn!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult LayDiaChiMacDinh()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return Json(new { success = false });

            var diaChi = _context.DiaChiNguoiDungs
                .FirstOrDefault(d => d.MaTK == maTK && d.MacDinh);

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

        [HttpGet]
        public IActionResult LayTatCaDiaChi()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return Json(new { success = false, message = "Chưa đăng nhập" });

            var danhSachDiaChi = _context.DiaChiNguoiDungs
                .Where(d => d.MaTK == maTK)
                .Select(d => new
                {
                    id = d.MaDiaChi,
                    tenNguoiNhan = d.TenNguoiNhan,
                    soDienThoai = d.SoDienThoai,
                    diaChiChiTiet = d.DiaChiChiTiet,
                    phuongXa = d.PhuongXa,
                    quanHuyen = d.QuanHuyen,
                    tinhTP = d.TinhTP,
                    macDinh = d.MacDinh
                })
                .ToList();

            return Json(new { success = true, data = danhSachDiaChi });
        }
    }
}
