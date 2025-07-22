using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_Project.Controllers.CaNhan
{
    public class DonHangController : Controller
    {
        private readonly AppDbContext _context;

        public DonHangController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult HuyDon(int maDonHang)
        {
            var don = _context.DonHangs.FirstOrDefault(d => d.MaDonHang == maDonHang);
            if (don != null && (don.TrangThaiDonHang == "ChoXacNhan" || don.TrangThaiDonHang == "DangXuLy"))
            {
                // 1. Đổi trạng thái
                don.TrangThaiDonHang = "HuyDon";

                // 2. Lấy chi tiết đơn hàng để hoàn lại số lượng
                var chiTietDonHangs = _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == maDonHang)
                    .ToList();

                foreach (var item in chiTietDonHangs)
                {
                    var sanPham = _context.SanPhams.FirstOrDefault(sp => sp.MaSP == item.MaSP);
                    if (sanPham != null)
                    {
                        sanPham.SoLuong += item.SoLuong; // hoàn lại số lượng
                    }
                }

                _context.SaveChanges();
                TempData["Message"] = "Đã hủy đơn hàng thành công và hoàn lại số lượng sản phẩm.";
            }
            else
            {
                TempData["Error"] = "Không thể hủy đơn hàng này.";
            }

            return RedirectToAction("Profile", "User");
        }


    }
}
