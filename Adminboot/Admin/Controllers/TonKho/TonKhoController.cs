using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

[Area("Admin")]
public class TonKhoController : Controller
{
    private readonly AppDbContext _context;

    public TonKhoController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(string search, int? categoryId, int? brandId, bool onlyInStock = false)
    {
        var query = _context.SanPhams.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(sp => sp.TenSP.Contains(search)
                || sp.MaSP.ToString().Contains(search));
        }
        if (categoryId.HasValue)
        {
            query = query.Where(sp => sp.MaDanhMuc == categoryId.Value);
        }
        if (brandId.HasValue)
        {
            query = query.Where(sp => sp.MaThuongHieu == brandId.Value);
        }
        if (onlyInStock)
        {
            query = query.Where(sp => sp.SoLuong > 0);
        }

        var sanPhams = query.Select(sp => new
        {
            sp.MaSP,
            sp.TenSP,
            sp.SoLuong,
            VonTonKho = sp.SoLuong * (sp.GiaGoc ?? 0),
            GiaTriTon = sp.SoLuong * ((sp.GiaGoc ?? 0) * 1.2m)
        }).ToList();

        ViewBag.NgayLap = DateTime.Now.ToString("dd/MM/yyyy");
        ViewBag.TongSoLuongTon = sanPhams.Sum(sp => sp.SoLuong);
        ViewBag.TongVonTonKho = sanPhams.Sum(sp => sp.VonTonKho);
        ViewBag.TongGiaTriTon = sanPhams.Sum(sp => sp.GiaTriTon);

        return View("~/Adminboot/Admin/Views/TonKho/Index.cshtml", sanPhams);
    }
}
