using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System; // <-- Thêm using này
using System.Collections.Generic; // <-- Thêm using này

namespace Final_Project.Controllers
{
    public class GioHangController : Controller
    {
        private readonly AppDbContext _context;

        public GioHangController(AppDbContext context)
        {
            _context = context;
        }

        // --- CÁC HÀM HELPER (Giữ nguyên) ---

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

        // ✅ HÀM LOAD DỮ LIỆU CHUNG (MỚI)
        private void LoadCommonData()
        {
            // Lấy thông tin User
            int? maTK = LayMaTK(); // Dùng lại hàm helper của bạn
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }

            // Lấy danh mục (cho _Layout)
            var danhMucs = _context.DanhMucs
                .Include(d => d.SanPhams)
                .ToList();

            ViewBag.DanhMucs = danhMucs;
        }


        // --- CÁC ACTION ---

        public IActionResult Index()
        {
            // ✅ BƯỚC 1: Gọi hàm LoadCommonData()
            LoadCommonData();
            
            // Lấy MaTK cho logic của riêng trang giỏ hàng
            int? maTK = LayMaTK();
            if (maTK == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // (Code lấy Avatar/HoTen đã được chuyển vào LoadCommonData())

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
                .Select(result => new GioHangViewModel // Giả sử bạn có ViewModel này
                {
                    MaSP = result.sp.MaSP,
                    TenSP = result.sp.TenSP,
                    SoLuong = result.gh.SoLuong,
                    DonGia = result.sp.DonGia, 
                    ImageURL = result.sp.ImageURL
                }).ToList();


            return View(gioHang); // Bỏ .ToList() thừa
        }
        
        // --- CÁC ACTION KHÁC (Không cần LoadCommonData vì trả về JSON/Redirect) ---

        [Authorize]
        [HttpPost]
        public IActionResult ThemGioHang(int maSP, int soLuong = 1)
        {
            // ... (code của bạn giữ nguyên) ...
            int? maTK = LayMaTK();

            if (maTK == null)
            {
                return Unauthorized(new { success = false, message = "Lỗi xác thực người dùng." });
            }

            var sanPham = _context.SanPhams.Find(maSP);
            if (sanPham == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            var gioHangItem = _context.GioHangs
                .FirstOrDefault(g => g.MaTK == maTK.Value && g.MaSP == maSP);

            int soLuongHienTai = gioHangItem?.SoLuong ?? 0;
            int soLuongMoi = soLuongHienTai + soLuong;

            if (sanPham.SoLuong < soLuongMoi)
            {
                return Json(new
                {
                    success = false,
                    message = $"Số lượng tồn kho không đủ (Chỉ còn {sanPham.SoLuong} sản phẩm)."
                });
            }

            if (gioHangItem != null)
            {
                gioHangItem.SoLuong = soLuongMoi;
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

            int newCartCount = HttpContext.Session.GetInt32("SoLuongGioHang") ?? 0;

            return Json(new
            {
                success = true,
                message = "Đã thêm sản phẩm vào giỏ hàng!",
                cartCount = newCartCount
            });
        }

        [HttpPost]
        public IActionResult CapNhatSoLuong(int maSP, int soLuong)
        {
            // ... (code của bạn giữ nguyên) ...
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
            // ... (code của bạn giữ nguyên) ...
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
            // ... (code của bạn giữ nguyên) ...
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
            // ... (code của bạn giữ nguyên) ...
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
            // ... (code của bạn giữ nguyên) ...
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
            // ... (code của bạn giữ nguyên) ...
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