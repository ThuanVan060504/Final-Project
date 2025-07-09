using Final_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Final_Project.Controllers
{
    public class HomeController : Controller
    {
        // Danh sách sản phẩm mẫu cho trang chủ
        public static List<SanPham> SanPhamMau = new List<SanPham>
        {
            new SanPham
            {
                MaSP = 2,
                TenSP = "Gạch ốp tường Đồng Tâm",
                DonGia = 195000,
                MoTa = "Họa tiết vân mây tinh tế, cho căn nhà thêm phần sang trọng và thanh lịch.",
                ImageURL = "/images/product2.jpg"
            },
            new SanPham
            {
                MaSP = 21,
                TenSP = "Khóa cửa điện tử Xiaomi",
                DonGia = 3990000,
                MoTa = "Bảo mật cao, mở bằng vân tay và mã số tiện lợi.",
                ImageURL = "/images/product21.jpg"
            },
            new SanPham
            {
                MaSP = 30,
                TenSP = "Kính chắn bếp cường lực",
                DonGia = 1550000,
                MoTa = "Kính màu xanh ngọc đẹp mắt, dễ vệ sinh, chịu nhiệt tốt.",
                ImageURL = "/images/product30.jpg"
            }
        };

        public IActionResult Index()
        {
            return View(SanPhamMau);
        }
    }
}
