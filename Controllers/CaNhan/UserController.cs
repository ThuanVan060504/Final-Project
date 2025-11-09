using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; 
using System.IO; 
using System.Threading.Tasks; 
using System.Linq; 
using System; 
using System.Collections.Generic; 
using System.Security.Claims;

namespace Final_Project.Models.Helpers; 

// === THÊM VIEWMODEL MỚI (Copy từ ThanhToanController) ===
// Model này khớp với data mà script GHN gửi lên
public class DiaChiMoiViewModel
{
    public string TenNguoiNhan { get; set; }
    public string SoDienThoai { get; set; }
    public string DiaChiChiTiet { get; set; }
    public int ProvinceID { get; set; } // Nhận INT từ JS
    public string ProvinceName { get; set; }
    public int DistrictID { get; set; } // Nhận INT từ JS
    public string DistrictName { get; set; }
    public string WardCode { get; set; }
    public string WardName { get; set; }
}
public class DiaChiEditViewModel : DiaChiMoiViewModel
{
    public int MaDiaChi { get; set; }
}

public class UserController : Controller
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }
    // === BẮT ĐẦU THÊM MỚI (Action lấy chi tiết địa chỉ) ===
    [HttpGet]
    public async Task<IActionResult> GetDiaChiDetails(int id)
    {
        int? maTK = HttpContext.Session.GetInt32("MaTK");
        if (maTK == null)
            return Unauthorized(new { message = "Vui lòng đăng nhập lại." });

        var diaChi = await _context.DiaChiNguoiDungs
                            .FirstOrDefaultAsync(d => d.MaDiaChi == id && d.MaTK == maTK.Value);

        if (diaChi == null)
            return NotFound(new { message = "Không tìm thấy địa chỉ." });

        // Trả về dữ liệu JSON, bao gồm cả các ID
        return Json(new
        {
            maDiaChi = diaChi.MaDiaChi,
            tenNguoiNhan = diaChi.TenNguoiNhan,
            soDienThoai = diaChi.SoDienThoai,
            diaChiChiTiet = diaChi.DiaChiChiTiet,
            tinhTP = diaChi.TinhTP,
            quanHuyen = diaChi.QuanHuyen,
            phuongXa = diaChi.PhuongXa,
            provinceID = diaChi.ProvinceID, // ID Tỉnh (dạng string)
            districtID = diaChi.DistrictID, // ID Quận (dạng string)
            wardCode = diaChi.WardCode     // Code Phường (dạng string)
        });
    }
    
    // === (Action cập nhật địa chỉ) ===
    [HttpPost]
    public async Task<IActionResult> UpdateDiaChi([FromBody] DiaChiEditViewModel model)
    {
        int? maTK = HttpContext.Session.GetInt32("MaTK");
        if (maTK == null)
            return Json(new { success = false, message = "Vui lòng đăng nhập lại." });

        if (model == null)
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

        try
        {
            var diaChi = await _context.DiaChiNguoiDungs
                                .FirstOrDefaultAsync(d => d.MaDiaChi == model.MaDiaChi && d.MaTK == maTK.Value);

            if (diaChi == null)
                return Json(new { success = false, message = "Không tìm thấy địa chỉ để cập nhật." });

            // Cập nhật thông tin
            diaChi.TenNguoiNhan = model.TenNguoiNhan;
            diaChi.SoDienThoai = model.SoDienThoai;
            diaChi.DiaChiChiTiet = model.DiaChiChiTiet;
            diaChi.TinhTP = model.ProvinceName;
            diaChi.QuanHuyen = model.DistrictName;
            diaChi.PhuongXa = model.WardName;
            diaChi.ProvinceID = model.ProvinceID.ToString();
            diaChi.DistrictID = model.DistrictID.ToString();
            diaChi.WardCode = model.WardCode;

            _context.DiaChiNguoiDungs.Update(diaChi);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật địa chỉ thành công." });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Json(new { success = false, message = "Lỗi khi cập nhật địa chỉ." });
        }
    }
    
    // ===  ACTION XÓA ===
    [HttpPost]
    public async Task<IActionResult> DeleteDiaChi(int id)
    {
        int? maTK = HttpContext.Session.GetInt32("MaTK");
        if (maTK == null)
            return Json(new { success = false, message = "Vui lòng đăng nhập lại." });

        try
        {
            var diaChi = await _context.DiaChiNguoiDungs
                                .FirstOrDefaultAsync(d => d.MaDiaChi == id && d.MaTK == maTK.Value);

            if (diaChi == null)
                return Json(new { success = false, message = "Không tìm thấy địa chỉ." });

            // Kiểm tra quan trọng: Không cho xóa địa chỉ mặc định
            if (diaChi.MacDinh)
                return Json(new { success = false, message = "Không thể xóa địa chỉ mặc định. Vui lòng chọn địa chỉ khác làm mặc định trước." });

            _context.DiaChiNguoiDungs.Remove(diaChi);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa địa chỉ thành công." });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Json(new { success = false, message = "Lỗi khi xóa địa chỉ." });
        }
    }
    // === KẾT THÚC BỔ SUNG ACTION XÓA ===
    public async Task<IActionResult> Profile()
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
        var now = DateTime.Now;

        // 1. Lấy tất cả voucher người dùng đã lưu (và còn hạn)
        var savedVouchers = await _context.TaiKhoanVouchers
            .Where(tv => tv.MaTK == maTK.Value)
            .Include(tv => tv.Voucher)
            .Select(tv => tv.Voucher)
            .Where(v => v.IsActive == true && v.NgayKetThuc >= now)
            .ToListAsync();

        // 2. Lấy danh sách ID các voucher ĐÃ SỬ DỤNG (và không bị hủy)
        var usedVoucherCounts = await _context.DonHangs
            .Where(d => d.MaTK == maTK.Value &&
                        d.MaVoucherID != null &&
                        d.TrangThaiDonHang != "HuyDon")
            .GroupBy(d => d.MaVoucherID.Value)
            .Select(g => new { VoucherId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.VoucherId, x => x.Count);

        // 3. Lọc danh sách: Chỉ lấy voucher nào có (Số lần đã dùng < Số lần cho phép)
        var availableVouchers = savedVouchers.Where(v =>
        {
            // Lấy số lần đã dùng (mặc định là 0)
            int usedCount = usedVoucherCounts.ContainsKey(v.MaVoucherID) ? usedVoucherCounts[v.MaVoucherID] : 0;

            // Trả về true NẾU số lần dùng < số lần cho phép
            return usedCount < v.SoLanSuDungToiDaMoiNguoiDung;
        })
        .OrderBy(v => v.NgayKetThuc)
        .ToList();

        ViewBag.MyVouchers = availableVouchers;
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
        // Chuyển hướng về tab thông tin cá nhân (mặc định)
        return RedirectToAction("Profile", new { fragment = "profileSection" });
    }

    // === SỬA LỖI: THAY THẾ ACTION [HttpPost] ThemDiaChi CŨ ===
    // [HttpPost]
    // public IActionResult ThemDiaChi(DiaChiNguoiDung model) // <= XÓA ACTION CŨ NÀY

    // === THÊM MỚI: ACTION NÀY NHẬN DiaChiMoiViewModel (Giống ThanhToanController) ===
    [HttpPost]
    public async Task<IActionResult> ThemDiaChiMoi([FromBody] DiaChiMoiViewModel model)
    {
        int? maTK = HttpContext.Session.GetInt32("MaTK");
        if (maTK == null)
            return Json(new { success = false, message = "Vui lòng đăng nhập lại." });

        if (model == null)
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

        try
        {
            var diaChiMoi = new DiaChiNguoiDung
            {
                MaTK = maTK.Value,
                TenNguoiNhan = model.TenNguoiNhan,
                SoDienThoai = model.SoDienThoai,
                DiaChiChiTiet = model.DiaChiChiTiet,
                TinhTP = model.ProvinceName,
                QuanHuyen = model.DistrictName,
                PhuongXa = model.WardName,
                // Chuyển int (từ ViewModel) sang string (cho Model DB)
                ProvinceID = model.ProvinceID.ToString(),
                DistrictID = model.DistrictID.ToString(),
                WardCode = model.WardCode,
                MacDinh = false // Form này không set mặc định
            };

            _context.DiaChiNguoiDungs.Add(diaChiMoi);
            await _context.SaveChangesAsync();

            // Trả về JSON để script xử lý
            return Json(new { success = true, message = "Thêm địa chỉ mới thành công." });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Json(new { success = false, message = "Lỗi khi lưu địa chỉ." });
        }
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

        return RedirectToAction("Profile", "User", new { fragment = "profileSection" });
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

        return RedirectToAction("Profile", "User", new { fragment = "changePasswordSection" });
    }
}