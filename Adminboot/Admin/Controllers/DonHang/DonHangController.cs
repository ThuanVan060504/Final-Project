using Final_Project.Models.Shop;
using Final_Project.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
[Area("Admin")]
public class DonHangController : Controller
{
    private readonly AppDbContext _context;

    public DonHangController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var donHangList = from dh in _context.DonHangs
                          join ct in _context.ChiTietDonHangs on dh.MaDonHang equals ct.MaDonHang
                          join sp in _context.SanPhams on ct.MaSP equals sp.MaSP
                          select new DonHangViewModel
                          {
                              MaDonHang = dh.MaDonHang,
                              NgayDat = dh.NgayDat,
                              PhuongThucThanhToan = dh.PhuongThucThanhToan,
                              TrangThaiDonHang = dh.TrangThaiDonHang,
                              MaSP = sp.MaSP,
                              TenSP = sp.TenSP,
                              ImageURL = sp.ImageURL,
                              DonGia = sp.DonGia,
                              SoLuongDat = ct.SoLuong,
                              DonGiaBan = ct.DonGia
                          };

        return View("~/Adminboot/Admin/Views/DonHangSP/Index.cshtml",donHangList.ToList());
    }
    [HttpPost]
    public IActionResult UpdateTrangThai(int maDonHang, string trangThai)
    {
        var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDonHang == maDonHang);
        if (donHang != null)
        {
            donHang.TrangThaiDonHang = trangThai;
            _context.SaveChanges(); // <<== câu lệnh UPDATE sẽ xảy ra ở đây
            return Ok();
        }
        return NotFound();
    }


}
