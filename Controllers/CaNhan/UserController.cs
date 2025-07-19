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

        // Lấy thông tin tài khoản
        var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
        // Sau khi lấy taiKhoan
        _context.Entry(taiKhoan).Collection(t => t.DiaChiNguoiDungs).Load();

        if (taiKhoan == null)
        {
            return NotFound("Không tìm thấy tài khoản.");
        }

        // Lấy đơn hàng của người dùng
        var donHangs = _context.DonHangs
            .Include(dh => dh.ChiTietDonHangs)
                .ThenInclude(ct => ct.SanPham)
            .Where(dh => dh.MaTK == maTK)
            .OrderByDescending(dh => dh.NgayDat)
            .ToList();

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

    [HttpGet]
    public IActionResult ThemDiaChi()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ThemDiaChi(DiaChiNguoiDung diaChi)
    {
        int? maTK = HttpContext.Session.GetInt32("MaTK");
        if (maTK == null)
        {
            return RedirectToAction("DangNhap", "Auth");
        }

        if (ModelState.IsValid)
        {
            diaChi.MaTK = maTK.Value;
            _context.DiaChiNguoiDungs.Add(diaChi);
            _context.SaveChanges();
            TempData["Success"] = "Thêm địa chỉ thành công!";
        }
        else
        {
            TempData["Error"] = "Vui lòng điền đầy đủ thông tin địa chỉ!";
        }

        return RedirectToAction("Profile");
    }
}
