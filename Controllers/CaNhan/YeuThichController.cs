using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Authorization; // <-- Đảm bảo bạn đã using
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_Project.Controllers.CaNhan
{
    public class YeuThichController : Controller
    {
        private readonly AppDbContext _context;

        public YeuThichController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // === HÀM THÊM YÊU THÍCH (ĐÃ SỬA CHO AJAX) ===
        // =========================================================
        [Authorize] // 1. Tự động trả về lỗi 401 nếu Cookie đăng nhập không có
        [HttpPost]
        public IActionResult Them(int maSP)
        {
            // 2. Kiểm tra Session (vì [Authorize] chỉ kiểm tra cookie)
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
            {
                // 3. Trả về lỗi 401 nếu Session mất
                return Unauthorized(new { success = false, message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
            }

            bool daTonTai = _context.SanPhamYeuThichs.Any(x => x.MaTK == maTK.Value && x.MaSP == maSP);

            if (daTonTai)
            {
                // 4. SỬA LỖI: Trả về JSON "thất bại" nếu đã tồn tại
                return Json(new { success = false, message = "Sản phẩm này đã có trong danh sách yêu thích." });
            }

            // 5. Thêm mới
            var yeuThich = new SanPhamYeuThich
            {
                MaTK = maTK.Value,
                MaSP = maSP,
                NgayThem = DateTime.Now
            };
            _context.SanPhamYeuThichs.Add(yeuThich);
            _context.SaveChanges();

            // 6. SỬA LỖI: Trả về JSON "thành công"
            return Json(new { success = true, message = "Đã thêm vào danh sách yêu thích!" });
        }

        // =========================================================
        // === HÀM LẤY DANH SÁCH (GIỮ NGUYÊN - VÌ TẢI CẢ TRANG) ===
        // =========================================================
        [Authorize] // Thêm [Authorize] ở đây để bảo vệ
        public IActionResult DanhSach()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return RedirectToAction("Login", "Auth"); // Redirect ở đây là ĐÚNG

            var yeuThich = _context.SanPhamYeuThichs
        .Include(y => y.SanPham)
        .Where(y => y.MaTK == maTK)
        .Select(y => y.SanPham)
        .ToList();

            return View(yeuThich);
        }

        // =========================================================
        // === HÀM BỎ YÊU THÍCH (ĐÃ SỬA CHO AJAX) ===
        // (Dùng cho trang Profile)
        // =========================================================
        [Authorize]
        [HttpPost]
        public IActionResult BoYeuThich(int maSP)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");

            if (maTK == null)
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập hết hạn." });
            }

            var spYeuThich = _context.SanPhamYeuThichs
              .FirstOrDefault(y => y.MaSP == maSP && y.MaTK == maTK);

            if (spYeuThich != null)
            {
                _context.SanPhamYeuThichs.Remove(spYeuThich);
                _context.SaveChanges();
                return Json(new { success = true, message = "Đã bỏ yêu thích." });
            }

            return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
        }
    }
}