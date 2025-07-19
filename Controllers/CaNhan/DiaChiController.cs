using Final_Project.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Final_Project.Models.Shop;

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
}
