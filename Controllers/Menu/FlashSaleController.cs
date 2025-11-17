using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Final_Project.Models;
using Final_Project.Models.Shop; 
using Final_Project.Models.ViewModels; 

namespace Final_Project.Controllers
{
    public class FlashSaleController : Controller
    {
        private readonly AppDbContext _context;
        public FlashSaleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            // 1. Tìm đợt sale đang hoạt động
            var activeSaleDot = await _context.DotFlashSale
                .Where(d => d.IsActive && now >= d.ThoiGianBatDau && now <= d.ThoiGianKetThuc)
                .OrderBy(d => d.ThoiGianKetThuc)
                .FirstOrDefaultAsync();

            if (activeSaleDot == null)
            {
                return View(null); // Không có sale, trả về View rỗng
            }

            // 2. Tạo ViewModel cho trang
            var viewModel = new FlashSalePageViewModel
            {
                MaDot = activeSaleDot.MaDot,
                TenDot = activeSaleDot.TenDot,
                ThoiGianKetThuc = activeSaleDot.ThoiGianKetThuc
            };

            // 3. Lấy sản phẩm chi tiết thuộc đợt sale
            var saleProducts = await _context.ChiTietFlashSale
                .Where(ct => ct.MaDot == activeSaleDot.MaDot)
                .Include(ct => ct.SanPham) // JOIN với bảng SanPham
                .Select(ct => new FlashSaleProductViewModel
                {
                    MaSP = ct.MaSP,
                    TenSP = ct.SanPham.TenSP,
                    ImageURL = ct.SanPham.ImageURL,
                    DonGiaGoc = ct.SanPham.DonGia, // Giá gốc
                    GiaSauGiam = ct.GiaSauGiam,     // Giá sale
                    PhanTramGiam = ct.PhanTramGiam,
                    SoLuongGiamGia = ct.SoLuongGiamGia,
                    SoLuongDaBan = ct.SoLuongDaBan
                })
                .ToListAsync();

            viewModel.SanPhams = saleProducts;

            return View(viewModel);
        }
    }
}