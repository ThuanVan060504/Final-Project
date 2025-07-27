using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
        _context.Entry(taiKhoan).Collection(t => t.DiaChiNguoiDungs).Load();

        ViewBag.Avatar = taiKhoan?.Avatar;
        ViewBag.HoTen = taiKhoan?.HoTen;

        if (taiKhoan == null)
        {
            return NotFound("Không tìm thấy tài khoản.");
        }

        var donHangs = _context.DonHangs
            .Where(d => d.MaTK == maTK)
            .Include(d => d.ChiTietDonHangs)
                .ThenInclude(ct => ct.SanPham)
            .Include(d => d.DiaChiNguoiDung)
            .OrderByDescending(d => d.NgayDat)
            .ToList();

        var yeuThich = _context.SanPhamYeuThichs
            .Where(y => y.MaTK == maTK)
            .Include(y => y.SanPham)
            .Select(y => y.SanPham)
            .ToList();

        ViewBag.SanPhamYeuThich = yeuThich;
        ViewBag.TaiKhoan = taiKhoan;

        return View(donHangs);
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
        return RedirectToAction("Profile");
    }

    [HttpPost]
    public IActionResult ThemDiaChi(ThemDiaChiViewModel model)
    {
        var taiKhoan = HttpContext.Session.GetObjectFromJson<TaiKhoan>("TaiKhoan");
        if (taiKhoan == null)
            return RedirectToAction("DangNhap", "Auth");

        if (model.MacDinh)
        {
            var diaChiCu = _context.DiaChiNguoiDungs
                .Where(d => d.MaTK == taiKhoan.MaTK && d.MacDinh)
                .ToList();
            foreach (var d in diaChiCu)
            {
                d.MacDinh = false;
            }
            _context.SaveChanges();
        }

        var diaChiMoi = new DiaChiNguoiDung
        {
            MaTK = taiKhoan.MaTK,
            TenNguoiNhan = model.TenNguoiNhan,
            SoDienThoai = model.SoDienThoai,
            DiaChiChiTiet = model.DiaChiChiTiet,
            PhuongXa = model.PhuongXa,
            QuanHuyen = model.QuanHuyen,
            TinhTP = model.TinhTP,
            MacDinh = model.MacDinh
        };

        _context.DiaChiNguoiDungs.Add(diaChiMoi);
        _context.SaveChanges();

        return RedirectToAction("Profile");
    }

    [HttpPost]
    public async Task<IActionResult> UploadAvatar(int MaTK, IFormFile avatarFile)
    {
        if (avatarFile != null && avatarFile.Length > 0)
        {
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == MaTK);
            if (taiKhoan == null) return NotFound();

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatarFile.FileName)}";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            taiKhoan.Avatar = $"/uploads/avatars/{fileName}";
            _context.Update(taiKhoan);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Profile", "User");
    }

    [HttpPost]
    public IActionResult DoiMatKhau(string OldPassword, string NewPassword, string ConfirmPassword)
    {
        var maTK = HttpContext.Session.GetInt32("MaTK");
        if (maTK == null) return RedirectToAction("DangNhap", "Auth");

        var user = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
        if (user == null) return NotFound();

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
            user.MatKhau = NewPassword;
            _context.SaveChanges();
            TempData["PasswordChangeMessage"] = "✅ Đổi mật khẩu thành công!";
        }

        return RedirectToAction("Profile", "User");
    }
}
