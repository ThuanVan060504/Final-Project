// Controllers/GioHangController.cs
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        private void CapNhatSessionSoLuong(int maTK)
        {
            int tongSoLuong = _context.GioHangs
                .Where(g => g.MaTK == maTK)
                .Sum(g => g.SoLuong);

            HttpContext.Session.SetInt32("SoLuongGioHang", tongSoLuong);
        }
        private int? LayMaTK()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
                return maTK;

            var email = User?.Identity?.IsAuthenticated == true
                ? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value
                : null;

            if (email != null)
            {
                var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("MaTK", user.MaTK);
                    CapNhatSessionSoLuong(user.MaTK);
                    return user.MaTK;
                }
            }

            return null;
        }

        public IActionResult Index()
        {
            int? maTK = LayMaTK();
            if (maTK == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
            ViewBag.Avatar = taiKhoan?.Avatar;
            ViewBag.HoTen = taiKhoan?.HoTen;

            var now = DateTime.Now;

            var gioHang = _context.GioHangs
                .Where(g => g.MaTK == maTK)
                .Join(_context.SanPhams,
                    gh => gh.MaSP,
                    sp => sp.MaSP,
                    (gh, sp) => new { gh, sp })
                .GroupJoin(
                    _context.FlashSales.Where(fs => fs.ThoiGianKetThuc > now),
                    x => x.sp.MaSP,
                    fs => fs.MaSP,
                    (x, flashSales) => new { x.gh, x.sp, flashSale = flashSales.FirstOrDefault() })
                .Select(result => new GioHangViewModel
                {
                    MaSP = result.sp.MaSP,
                    TenSP = result.sp.TenSP,
                    SoLuong = result.gh.SoLuong,
                    DonGia = result.sp.DonGia,
                    ImageURL = result.sp.ImageURL
                }).ToList();


            return View(gioHang.ToList());
        }
        [Authorize] // <--- BẮT BUỘC: Tự động trả về lỗi 401 nếu chưa đăng nhập
        [HttpPost]
        public IActionResult ThemGioHang(int maSP, int soLuong = 1)
        {
            // 1. Lấy MaTK (Mã Tài Khoản)
            // [Authorize] đã đảm bảo người dùng đã đăng nhập.
            // Dùng hàm LayMaTK() của bạn để lấy ID (từ session hoặc từ User.Claims)
            int? maTK = LayMaTK();

            if (maTK == null)
            {
                // Trường hợp hi hữu nếu LayMaTK() vì lý do nào đó không lấy được
                return Unauthorized(new { success = false, message = "Lỗi xác thực người dùng." });
            }

            // 2. Kiểm tra tồn kho sản phẩm
            var sanPham = _context.SanPhams.Find(maSP);
            if (sanPham == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            var gioHangItem = _context.GioHangs
                .FirstOrDefault(g => g.MaTK == maTK.Value && g.MaSP == maSP);

            int soLuongHienTai = gioHangItem?.SoLuong ?? 0;
            int soLuongMoi = soLuongHienTai + soLuong;

            // Kiểm tra xem số lượng mới có vượt quá tồn kho không
            if (sanPham.SoLuong < soLuongMoi)
            {
                return Json(new
                {
                    success = false,
                    message = $"Số lượng tồn kho không đủ (Chỉ còn {sanPham.SoLuong} sản phẩm)."
                });
            }

            // 3. Logic thêm/cập nhật giỏ hàng (giống code cũ của bạn)
            if (gioHangItem != null)
            {
                gioHangItem.SoLuong = soLuongMoi; // Cập nhật số lượng mới
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

            // 4. Cập nhật Session và TRẢ VỀ JSON (thay vì Redirect)
            CapNhatSessionSoLuong(maTK.Value);

            return RedirectToAction("Index");
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
