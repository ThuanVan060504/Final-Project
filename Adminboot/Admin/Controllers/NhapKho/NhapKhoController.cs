using Final_Project.Models;
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

[Area("Admin")]
public class NhapKhoController : Controller
{
    private readonly AppDbContext _context;

    public NhapKhoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.NhaCungCaps = await _context.NhaCungCaps.ToListAsync();
        ViewBag.SanPhams = await _context.SanPhams.ToListAsync();
        return View("~/Adminboot/Admin/Views/NhapKhoSP/Index.cshtml");
    }

    [HttpPost]
    public IActionResult Index(List<int> MaSP, List<int?> SoLuong, List<decimal?> DonGia,
                           decimal TongTien, decimal DaThanhToan, decimal ConNo,
                           DateTime NgayNhap)
    {
        // Debug log dữ liệu nhận từ form
        Console.WriteLine("===== DỮ LIỆU FORM GỬI LÊN =====");
        Console.WriteLine("Ngày nhập: " + NgayNhap.ToString("dd/MM/yyyy"));
        Console.WriteLine("Tổng tiền: " + TongTien);
        Console.WriteLine("Đã thanh toán: " + DaThanhToan);
        Console.WriteLine("Còn nợ: " + ConNo);

        if (MaSP != null && MaSP.Count > 0)
        {
            for (int i = 0; i < MaSP.Count; i++)
            {
                Console.WriteLine($"SP {MaSP[i]} | SL: {SoLuong[i]} | Giá: {DonGia[i]}");
            }
        }
        else
        {
            Console.WriteLine("Không có sản phẩm nào được chọn!");
        }

        // TODO: Thêm code lưu vào DB ở đây

        return RedirectToAction("Index");
    }

}

