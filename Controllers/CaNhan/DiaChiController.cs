using Final_Project.Models.Helpers;
using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

public class DiaChiController : Controller
{
    private readonly AppDbContext _context;

    public DiaChiController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var taiKhoanId = HttpContext.Session.GetInt32("MaTK");
        var diaChiList = _context.DiaChiNguoiDungs
            .Where(dc => dc.MaTK == taiKhoanId)
            .ToList();

        return View(diaChiList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(DiaChiNguoiDung diaChi)
    {
        diaChi.MaTK = (int)HttpContext.Session.GetInt32("MaTK");

        _context.DiaChiNguoiDungs.Add(diaChi);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult SetDefault(int id)
    {
        var diaChi = _context.DiaChiNguoiDungs.FirstOrDefault(d => d.MaDiaChi == id);
        if (diaChi == null) return NotFound();

        // Lấy các địa chỉ của userS
        var diaChiKhac = _context.DiaChiNguoiDungs
            .Where(d => d.MaTK == diaChi.MaTK && d.MaTK != id).ToList();

        // Set tất cả về false
        foreach (var d in diaChiKhac)
        {
            d.MacDinh = false;
        }

        // Set địa chỉ được chọn thành mặc định
        diaChi.MacDinh = true;

        _context.SaveChanges();

        return RedirectToAction("ThongTinTaiKhoan", "TaiKhoan");
    }


}
