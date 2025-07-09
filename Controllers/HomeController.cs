using System.Diagnostics;
using Final_Project.Models;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public static List<Product> Products = new List<Product>
        {
    new Product { Id = 2, Name = "Gạch ốp tường Đồng Tâm", Price = 195000, Description = "Họa tiết vân mây tinh tế, cho căn nhà thêm phần sang trọng và thanh lịch.", ImageUrl = "/images/product2.jpg" },
    new Product { Id = 21, Name = "Khóa cửa điện tử Xiaomi", Price = 3990000, Description = "Bảo mật cao, mở bằng vân tay và mã số tiện lợi.", ImageUrl = "/images/product21.jpg" },
    new Product { Id = 30, Name = "Kính chắn bếp cường lực", Price = 1550000, Description = "Kính màu xanh ngọc đẹp mắt, dễ vệ sinh, chịu nhiệt tốt.", ImageUrl = "/images/product30.jpg" }
        };

    public IActionResult Index()
    {
        return View(Products); 
    }
}
