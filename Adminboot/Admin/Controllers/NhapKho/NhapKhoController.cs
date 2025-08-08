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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        int MaNCC,
        DateTime NgayNhap,

        string GhiChu,
        string HinhThucThanhToan,
        decimal? TongTien,
        decimal? DaThanhToan,
        decimal? ConNo,
        List<int> MaSP,
        List<int?> SoLuong,
        List<decimal?> DonGia)
    {
        Console.WriteLine($"DEBUG: MaNCC={MaNCC}, NgayNhap={NgayNhap}");
        Console.WriteLine($"DEBUG: MaSP.Count={(MaSP != null ? MaSP.Count : 0)}");

        if (MaSP == null || MaSP.Count == 0)
        {
            ModelState.AddModelError("", "Phải chọn ít nhất một sản phẩm để nhập kho.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.NhaCungCaps = await _context.NhaCungCaps.ToListAsync();
            ViewBag.SanPhams = await _context.SanPhams.ToListAsync();
            return View("~/Adminboot/Admin/Views/NhapKhoSP/Index.cshtml");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var nhapKho = new NhapKho
            {
                MaNCC = MaNCC,
                NgayNhap = NgayNhap,
                GhiChu = GhiChu,
                HinhThucThanhToan = HinhThucThanhToan,
                TongTien = TongTien ?? 0,
                DaThanhToan = DaThanhToan ?? 0,
                ConNo = ConNo ?? 0
                
            };

            _context.NhapKhos.Add(nhapKho);
            await _context.SaveChangesAsync();

            int maNhapKho = nhapKho.MaNhapKho;

            for (int i = 0; i < MaSP.Count; i++)
            {
                var chiTiet = new ChiTietNhapKho
                {
                    MaNhapKho = maNhapKho,
                    MaSP = MaSP[i],
                    SoLuong = SoLuong[i] ?? 0,
                    DonGia = DonGia[i] ?? 0
                };
                _context.ChiTietNhapKhos.Add(chiTiet);

                var sp = await _context.SanPhams.FindAsync(MaSP[i]);
                if (sp != null)
                {
                    sp.SoLuong += SoLuong[i] ?? 0;
                    _context.SanPhams.Update(sp);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["SuccessMessage"] = "Nhập kho thành công!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
            ViewBag.NhaCungCaps = await _context.NhaCungCaps.ToListAsync();
            ViewBag.SanPhams = await _context.SanPhams.ToListAsync();
            return View("~/Adminboot/Admin/Views/NhapKhoSP/Index.cshtml");
        }
    }
}
