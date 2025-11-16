using Final_Project.Areas.Admin.Controllers;
using Final_Project.Models.Shop;
using Final_Project.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
[Area("Admin")]
public class DonHangController : BaseAdminController
{
    private readonly AppDbContext _context;

    public DonHangController(AppDbContext context) : base(context)
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

        return View("~/Adminboot/Admin/Views/DonHangSP/Index.cshtml", donHangList.ToList());
    }
    // Trong DonHangController.cs

    [HttpPost]
    public IActionResult UpdateTrangThai(int maDonHang, string trangThai)
    {
        var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDonHang == maDonHang);

        if (donHang == null)
        {
            return NotFound();
        }
        if (donHang.TrangThaiDonHang == "DangGiao" || donHang.TrangThaiDonHang == "DaGiao")
        {
            if (trangThai == "HuyDon" || trangThai == "DangXuLy")
            {
                return BadRequest("Không thể chuyển đơn hàng từ trạng thái Đang Giao/Đã Giao sang Hủy Đơn hoặc Đang Xử Lý.");
            }
        }

        // Nếu trạng thái mới giống trạng thái cũ, không cần làm gì
        if (donHang.TrangThaiDonHang == trangThai)
        {
            return Ok();
        }

        // Cập nhật trạng thái mới nếu hợp lệ
        donHang.TrangThaiDonHang = trangThai;

        // Đặc biệt, nếu chuyển sang 'DaGiao', cập nhật NgayGiao
        if (trangThai == "DaGiao")
        {
            donHang.NgayGiao = DateTime.Now;
        }

        _context.SaveChanges();
        return Ok();
    }

    // Action để xem chi tiết đơn hàng và thông tin khách hàng
    public IActionResult ChiTietDonHang(int maDonHang)
    {
        // Sử dụng Eager loading như trước
        var donHang = _context.DonHangs
            .Include(dh => dh.TaiKhoan)
            .Include(dh => dh.DiaChiNguoiDung)
            .Include(dh => dh.ChiTietDonHangs)
                .ThenInclude(ct => ct.SanPham)
            .FirstOrDefault(dh => dh.MaDonHang == maDonHang);

        if (donHang == null)
        {
            // Có thể trả về Partial View rỗng hoặc thông báo lỗi
            return PartialView("_NotFoundPartial");
        }

        // Trả về Partial View chứa nội dung chi tiết
        return PartialView("~/Adminboot/Admin/Views/DonHangSP/_ChiTietDonHangPartial.cshtml", donHang);
    }

}
