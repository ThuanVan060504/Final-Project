using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Final_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminVoucherController : BaseAdminController
    {
        private readonly AppDbContext _context;

        public AdminVoucherController(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // ===================================
        // READ: Danh sách Voucher
        // ===================================
        public async Task<IActionResult> Index()
        {
            var vouchers = await _context.Vouchers
                                         .OrderByDescending(v => v.MaVoucherID)
                                         .ToListAsync();
            // Sửa đường dẫn View
            return View("~/Adminboot/Admin/Views/AdminVoucher/Index.cshtml", vouchers);
        }

        // ===================================
        // CREATE (GET)
        // ===================================
        public IActionResult Create()
        {
            var model = new Voucher
            {
                NgayBatDau = DateTime.Now,
                NgayKetThuc = DateTime.Now.AddDays(30),
                IsActive = true,
                SoLanSuDungToiDaMoiNguoiDung = 1,
                LoaiGiamGia = "SoTien",
                DonHangToiThieu = 0,
                SoLuongDaDung = 0
            };
            // Sửa đường dẫn View
            return View("~/Adminboot/Admin/Views/AdminVoucher/Create.cshtml", model);
        }

        // ===================================
        // CREATE (POST)
        // ===================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string MaCode, string MoTa, string LoaiGiamGia,
            decimal GiaTriGiam, decimal? GiamGiaToiDa, decimal DonHangToiThieu,
            DateTime NgayBatDau, DateTime NgayKetThuc,
            int? SoLuongToiDa, int SoLanSuDungToiDaMoiNguoiDung, bool IsActive)
        {
            // 1. Tạo đối tượng Voucher thủ công (giống DanhmucSP)
            var voucher = new Voucher
            {
                MaCode = MaCode,
                MoTa = MoTa,
                LoaiGiamGia = LoaiGiamGia,
                GiaTriGiam = GiaTriGiam,
                GiamGiaToiDa = (LoaiGiamGia == "PhanTram") ? GiamGiaToiDa : null, // Chỉ set nếu là Phần Trăm
                DonHangToiThieu = DonHangToiThieu,
                NgayBatDau = NgayBatDau,
                NgayKetThuc = NgayKetThuc,
                SoLuongToiDa = SoLuongToiDa,
                SoLuongDaDung = 0, // Mặc định khi tạo mới
                SoLanSuDungToiDaMoiNguoiDung = SoLanSuDungToiDaMoiNguoiDung,
                IsActive = IsActive
            };

            // 2. Kiểm tra validation thủ công (giống DanhmucSP)
            if (string.IsNullOrEmpty(MaCode))
            {
                ModelState.AddModelError("MaCode", "Vui lòng nhập Mã Code.");
            }
            else if (await _context.Vouchers.AnyAsync(v => v.MaCode == MaCode))
            {
                ModelState.AddModelError("MaCode", "Mã code này đã tồn tại.");
            }

            if (string.IsNullOrEmpty(MoTa))
            {
                ModelState.AddModelError("MoTa", "Vui lòng nhập Mô tả.");
            }

            // 3. Kiểm tra ModelState
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(voucher);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Tạo voucher mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu: " + ex.Message);
                }
            }

            // 4. Nếu lỗi, trả về View với dữ liệu đã nhập
            return View("~/Adminboot/Admin/Views/AdminVoucher/Create.cshtml", voucher);
        }

        // ===================================
        // EDIT (GET)
        // ===================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound();

            // Sửa đường dẫn View
            return View("~/Adminboot/Admin/Views/AdminVoucher/Edit.cshtml", voucher);
        }

        // ===================================
        // EDIT (POST)
        // ===================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
             int MaVoucherID, string MaCode, string MoTa, string LoaiGiamGia,
             decimal GiaTriGiam, decimal? GiamGiaToiDa, decimal DonHangToiThieu,
             DateTime NgayBatDau, DateTime NgayKetThuc,
             int? SoLuongToiDa, int SoLuongDaDung, int SoLanSuDungToiDaMoiNguoiDung, bool IsActive)
        {
            // 1. Kiểm tra validation thủ công
            if (await _context.Vouchers.AnyAsync(v => v.MaCode == MaCode && v.MaVoucherID != MaVoucherID))
            {
                ModelState.AddModelError("MaCode", "Mã code này đã tồn tại.");
            }

            if (string.IsNullOrEmpty(MoTa))
            {
                ModelState.AddModelError("MoTa", "Vui lòng nhập Mô tả.");
            }

            // 2. Tạo đối tượng Voucher thủ công
            var voucher = new Voucher
            {
                MaVoucherID = MaVoucherID,
                MaCode = MaCode,
                MoTa = MoTa,
                LoaiGiamGia = LoaiGiamGia,
                GiaTriGiam = GiaTriGiam,
                GiamGiaToiDa = (LoaiGiamGia == "PhanTram") ? GiamGiaToiDa : null,
                DonHangToiThieu = DonHangToiThieu,
                NgayBatDau = NgayBatDau,
                NgayKetThuc = NgayKetThuc,
                SoLuongToiDa = SoLuongToiDa,
                SoLuongDaDung = SoLuongDaDung, // Giữ nguyên số lượng đã dùng
                SoLanSuDungToiDaMoiNguoiDung = SoLanSuDungToiDaMoiNguoiDung,
                IsActive = IsActive
            };

            // 3. Kiểm tra ModelState
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(voucher);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật voucher thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật: " + ex.Message);
                }
            }

            // 4. Nếu lỗi, trả về View với dữ liệu đã nhập
            return View("~/Adminboot/Admin/Views/AdminVoucher/Edit.cshtml", voucher);
        }

        // ===================================
        // ✅ THÊM HÀM NÀY VÀO (Giống DanhmucSP)
        // ===================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                TempData["ErrorMessage"] = "❌ Voucher không tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra xem voucher đã được dùng chưa (giống DanhmucSP kiểm tra SanPham)
            if (voucher.SoLuongDaDung > 0)
            {
                TempData["ErrorMessage"] = "❌ Voucher đã được sử dụng (Số lượng > 0), không thể xóa!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Vouchers.Remove(voucher);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "✅ Xóa voucher thành công!";
            }
            catch (Exception ex)
            {
                // Lỗi khóa ngoại (nếu có bảng ghi liên kết khác)
                TempData["ErrorMessage"] = "❌ Không thể xóa. Voucher có thể đã được sử dụng hoặc có liên kết.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}