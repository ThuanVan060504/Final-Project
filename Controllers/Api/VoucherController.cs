using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Final_Project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VoucherController(AppDbContext context)
        {
            _context = context;
        }

        // ===============================================
        // 💾 API ĐỂ LƯU VOUCHER
        // [POST] /api/voucher/save
        // ===============================================
        [HttpPost("save")]
        public async Task<IActionResult> SaveVoucher([FromForm] int voucherId)
        {
            // 1. Kiểm tra User đã đăng nhập chưa
            var maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
            {
                return Unauthorized(new { success = false, message = "Bạn cần đăng nhập để lưu voucher." });
            }

            try
            {
                // 2. Kiểm tra voucher có tồn tại và hợp lệ không
                var now = DateTime.Now;
                var voucher = await _context.Vouchers
                    .FirstOrDefaultAsync(v => v.MaVoucherID == voucherId &&
                                        v.IsActive == true &&
                                        v.NgayKetThuc >= now);

                if (voucher == null)
                {
                    return NotFound(new { success = false, message = "Voucher không tồn tại hoặc đã hết hạn." });
                }

                // 3. Kiểm tra xem user đã lưu voucher này CHƯA
                bool daLuu = await _context.TaiKhoanVouchers
                    .AnyAsync(tv => tv.MaTK == maTK.Value && tv.MaVoucherID == voucherId);

                if (daLuu)
                {
                    return Conflict(new { success = false, message = "Bạn đã lưu voucher này rồi." });
                }

                // 4. Tạo bản ghi mới
                var taiKhoanVoucher = new TaiKhoanVoucher
                {
                    MaTK = maTK.Value,
                    MaVoucherID = voucherId,
                    NgayLuu = DateTime.Now
                };

                _context.TaiKhoanVouchers.Add(taiKhoanVoucher);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Lưu voucher thành công!" });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi (nếu cần)
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi. Vui lòng thử lại." });
            }
        }
    }
}