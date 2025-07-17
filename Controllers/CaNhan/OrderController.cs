using Final_Project.Models;
using Final_Project.Models.Shop;
using Final_Project.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Final_Project.Controllers.CaNhan
{
    public class OrderController : Controller
    {
        private readonly AppDbContext context;

        public OrderController(AppDbContext _context)
        {
            context = _context;
        }

        public IActionResult History()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var user = context.TaiKhoans.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var orders = (from ctdh in context.ChiTietDonHangs
                          join dh in context.DonHangs on ctdh.MaDonHang equals dh.MaDonHang
                          join sp in context.SanPhams on ctdh.MaSP equals sp.MaSP
                          where dh.MaTK == user.MaTK
                          orderby dh.NgayDat descending
                          select new OrderHistoryViewModel
                          {
                              MaDonHang = dh.MaDonHang,
                              NgayDat = dh.NgayDat,
                              TenSP = sp.TenSP,
                              SoLuong = ctdh.SoLuong,
                              DonGia = ctdh.DonGia
                          }).ToList();

            return View(orders);
        }
    }
}
