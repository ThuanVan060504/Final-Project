using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

// Thêm using này nếu chưa có
using System.Collections.Generic;

public class FlashSaleController : Controller
{
    private readonly AppDbContext _context;

    public FlashSaleController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        LoadCommonData();

        var now = DateTime.Now;

        // Code logic của FlashSale giữ nguyên
        var flashSales = _context.FlashSales
            .Include(fs => fs.SanPham)
            .ToList();

        foreach (var fs in flashSales)
        {
            var sp = fs.SanPham;

            if (now >= fs.ThoiGianBatDau && now < fs.ThoiGianKetThuc)
            {
                if (sp.GiaGoc == null || sp.GiaGoc == 0)
                {
                    sp.GiaGoc = sp.DonGia;
                    sp.DonGia = sp.DonGia * (1 - fs.DiscountPercent / 100m);
                }
            }
            else
            {
                if (sp.GiaGoc != null && sp.GiaGoc > 0)
                {
                    sp.DonGia = sp.GiaGoc.Value;
                    sp.GiaGoc = null;
                }
            }
        }

        _context.SaveChanges();

        var activeFlashSales = flashSales
            .Where(fs => now >= fs.ThoiGianBatDau && now < fs.ThoiGianKetThuc)
            .Take(10)
            .ToList();

        return View(activeFlashSales);
    }

    private void LoadCommonData()
    {
        // Lấy thông tin User
        int? maTK = HttpContext.Session.GetInt32("MaTK");
        if (maTK != null)
        {
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
            ViewBag.Avatar = taiKhoan?.Avatar;
            ViewBag.HoTen = taiKhoan?.HoTen;
        }

        // Lấy danh mục
        var danhMucs = _context.DanhMucs
            .Include(d => d.SanPhams)
            .ToList();

        ViewBag.DanhMucs = danhMucs;
    }
}