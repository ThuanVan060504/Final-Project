using Final_Project.Areas.Admin.Controllers;
using Final_Project.Models;
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

[Area("Admin")]
public class NhapKhoController : BaseAdminController
{
    private readonly AppDbContext _context;

    public NhapKhoController(AppDbContext context) : base(context)
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
    public async Task<IActionResult> Index(List<int> MaSP, List<int?> SoLuong, List<decimal?> DonGia,
                            decimal TongTien, decimal DaThanhToan, decimal ConNo,
                            DateTime NgayNhap, int MaNCC, int MaNguoiNhap, string GhiChu, string HinhThucThanhToan)
    {
        if (MaSP == null || MaSP.Count == 0)
        {
            ModelState.AddModelError("", "Bạn chưa chọn sản phẩm để nhập kho.");
            // Load lại dữ liệu ViewBag
            ViewBag.NhaCungCaps = await _context.NhaCungCaps.ToListAsync();
            ViewBag.SanPhams = await _context.SanPhams.ToListAsync();
            return View("~/Adminboot/Admin/Views/NhapKhoSP/Index.cshtml");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Tạo phiếu nhập kho mới
            var phieuNhap = new NhapKho
            {
                MaNCC = MaNCC,
                NgayNhap = NgayNhap,
                MaTK = MaNguoiNhap,
                GhiChu = GhiChu,
                TongTien = TongTien,
                DaThanhToan = DaThanhToan,
                ConNo = ConNo,
                HinhThucThanhToan = HinhThucThanhToan
            };

            _context.NhapKhos.Add(phieuNhap);
            await _context.SaveChangesAsync();

            // Lấy Id phiếu nhập vừa tạo
            int maNhapKho = phieuNhap.MaNhapKho;

            // Duyệt từng sản phẩm nhập
            for (int i = 0; i < MaSP.Count; i++)
            {
                int maSP = MaSP[i];
                int soLuong = SoLuong[i] ?? 0;
                decimal donGia = DonGia[i] ?? 0;

                if (soLuong <= 0 || donGia <= 0)
                    continue; // bỏ qua nếu số lượng hoặc giá nhập không hợp lệ

                var chiTiet = new ChiTietNhapKho
                {
                    MaNhapKho = maNhapKho,
                    MaSP = maSP,
                    SoLuong = soLuong,
                    DonGia = donGia,
                    ThanhTien = soLuong * donGia
                };

                _context.ChiTietNhapKhos.Add(chiTiet);

                // Cập nhật tồn kho sản phẩm (nếu có)
                var sp = await _context.SanPhams.FindAsync(maSP);
                if (sp != null)
                {
                    sp.SoLuong += soLuong;
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

            // Load lại dữ liệu ViewBag
            ViewBag.NhaCungCaps = await _context.NhaCungCaps.ToListAsync();
            ViewBag.SanPhams = await _context.SanPhams.ToListAsync();
            return View("~/Adminboot/Admin/Views/NhapKhoSP/Index.cshtml");
        }
    }

}

