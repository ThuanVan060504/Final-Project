using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // Đảm bảo bạn đã using
using System.IO; // Đảm bảo bạn đã using
using System.Threading.Tasks; // Đảm bảo bạn đã using

namespace Final_Project.Models.Helpers;

public class UserController : Controller
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Profile()
    {
        int? maTK = HttpContext.Session.GetInt32("MaTK");
        if (maTK == null)
        {
            return RedirectToAction("DangNhap", "Auth");
        }

        // Tải tài khoản và địa chỉ liên quan
        var taiKhoan = _context.TaiKhoans
            .Include(t => t.DiaChiNguoiDungs) // Dùng Include để tải địa chỉ
            .FirstOrDefault(t => t.MaTK == maTK);

        // Không cần .Load() riêng nếu đã dùng Include
        // _context.Entry(taiKhoan).Collection(t => t.DiaChiNguoiDungs).Load(); 

        ViewBag.Avatar = taiKhoan?.Avatar;
        ViewBag.HoTen = taiKhoan?.HoTen;

        if (taiKhoan == null)
        {
            return NotFound("Không tìm thấy tài khoản.");
        }

        // Tải đơn hàng
        var donHangs = _context.DonHangs
            .Where(d => d.MaTK == maTK)
            .Include(d => d.ChiTietDonHangs)
                .ThenInclude(ct => ct.SanPham)
            .Include(d => d.DiaChiNguoiDung)
            .OrderByDescending(d => d.NgayDat)
            .ToList();

        // Tải sản phẩm yêu thích
        var yeuThich = _context.SanPhamYeuThichs
            .Where(y => y.MaTK == maTK)
            .Include(y => y.SanPham)
            .Select(y => y.SanPham)
            .ToList();

        ViewBag.SanPhamYeuThich = yeuThich;
        ViewBag.TaiKhoan = taiKhoan; // Gửi toàn bộ tài khoản (bao gồm danh sách địa chỉ)

        return View(donHangs); // Model chính là danh sách đơn hàng
    }

    [HttpPost]
    public IActionResult UpdateProfile(TaiKhoan model)
    {
        var user = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == model.MaTK);
        if (user != null)
        {
            user.HoTen = model.HoTen;
            user.Email = model.Email;
            user.SDT = model.SDT;

            _context.SaveChanges();
            TempData["Success"] = "Cập nhật thông tin thành công!";
        }
        // Chuyển hướng về tab thông tin cá nhân (mặc định)
        return RedirectToAction("Profile", new { fragment = "profileSection" });
    }

    // === ĐÃ SỬA ĐỂ ĐỒNG BỘ VỚI FORM VÀ MODEL DiaChiNguoiDung ===
    [HttpPost]
    public IActionResult ThemDiaChi(DiaChiNguoiDung model) // Đổi từ ViewModel sang Model chính
    {
        int? maTK = HttpContext.Session.GetInt32("MaTK");
        if (maTK == null) return RedirectToAction("DangNhap", "Auth");

        // Gán MaTK cho địa chỉ mới
        model.MaTK = maTK.Value;

        // Bỏ qua kiểm tra ModelState cho TaiKhoan (nếu nó là [Required])
        ModelState.Remove("TaiKhoan");

        if (ModelState.IsValid)
        {
            // Nếu người dùng chọn "Đặt làm mặc định"
            if (model.MacDinh)
            {
                // Bỏ tất cả mặc định cũ của tài khoản này
                var diaChiCu = _context.DiaChiNguoiDungs.Where(d => d.MaTK == maTK && d.MacDinh).ToList();
                foreach (var d in diaChiCu)
                {
                    d.MacDinh = false;
                }
            }

            _context.DiaChiNguoiDungs.Add(model); // Thêm model đã được binding
            _context.SaveChanges();

            TempData["Success"] = "Thêm địa chỉ thành công!";
        }
        else
        {
            // Lấy lỗi
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join(", ", errors);
        }

        // Chuyển hướng về tab địa chỉ
        return RedirectToAction("Profile", new { fragment = "addressSection" });
    }
    // === KẾT THÚC SỬA ĐỔI ===


    [HttpPost]
    public async Task<IActionResult> UploadAvatar(int MaTK, IFormFile avatarFile)
    {
        if (avatarFile != null && avatarFile.Length > 0)
        {
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == MaTK);
            if (taiKhoan == null) return NotFound();

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatarFile.FileName)}";
            // Đảm bảo thư mục "avatars" nằm trong "uploads"
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            // Lưu đường dẫn tương đối
            taiKhoan.Avatar = $"/uploads/avatars/{fileName}";
            _context.Update(taiKhoan);
            await _context.SaveChangesAsync();
        }

        // Chuyển hướng về tab thông tin cá nhân
        return RedirectToAction("Profile", "User", new { fragment = "profileSection" });
    }

    [HttpPost]
    public IActionResult DoiMatKhau(string OldPassword, string NewPassword, string ConfirmPassword)
    {
        var maTK = HttpContext.Session.GetInt32("MaTK");
        if (maTK == null) return RedirectToAction("DangNhap", "Auth");

        var user = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
        if (user == null) return NotFound();

        // CẢNH BÁO BẢO MẬT: Không nên so sánh mật khẩu plain-text.
        // Đây là cách làm tạm thời theo logic code của bạn.
        // Bạn nên sử dụng Hashing (ví dụ: BCrypt) cho mật khẩu.
        if (OldPassword != user.MatKhau)
        {
            TempData["PasswordChangeMessage"] = "❌ Mật khẩu hiện tại không đúng.";
        }
        else if (NewPassword != ConfirmPassword)
        {
            TempData["PasswordChangeMessage"] = "❌ Mật khẩu mới không khớp.";
        }
        else
        {
            user.MatKhau = NewPassword; // Cũng không nên lưu plain-text
            _context.SaveChanges();
            TempData["PasswordChangeMessage"] = "✅ Đổi mật khẩu thành công!";
        }

        // Chuyển hướng về tab đổi mật khẩu
        return RedirectToAction("Profile", "User", new { fragment = "changePasswordSection" });
    }
}