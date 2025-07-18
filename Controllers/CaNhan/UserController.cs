using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // Thông tin người dùng
        var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
        if (taiKhoan == null)
        {
            return NotFound("Không tìm thấy tài khoản.");
        }

        // Lịch sử đơn hàng
        var donHangs = _context.DonHangs
            .Include(dh => dh.ChiTietDonHangs)
            .ThenInclude(ct => ct.SanPham)
            .Where(dh => dh.MaTK == maTK)
            .OrderByDescending(dh => dh.NgayDat)
            .ToList();

        ViewBag.TaiKhoan = taiKhoan;
        return View(donHangs); // Trả về danh sách đơn hàng cho View
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


}
