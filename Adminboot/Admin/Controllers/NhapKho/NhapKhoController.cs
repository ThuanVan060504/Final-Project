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

    private const decimal DEFAULT_MARKUP_RATIO = 1.5m;

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
    public async Task<IActionResult> Index(
        List<int> MaSP,
        List<decimal?> SoLuong,
        List<decimal?> DonGia,
        List<decimal?> MarkupRatio, // <<< THÊM 1: Nhận danh sách MarkupRatio
        decimal TongTien,
        decimal DaThanhToan,
        DateTime NgayNhap,
        int MaNCC,
        int MaNguoiNhap,
        string GhiChu,
        string HinhThucThanhToan,
        decimal ChietKhau = 0,
        decimal ChiPhiPhatSinh = 0)
    {
        // <<< SỬA 2: Thêm kiểm tra MarkupRatio
        if (MaSP == null || MaSP.Count == 0 || SoLuong == null || DonGia == null || MarkupRatio == null ||
            MaSP.Count != SoLuong.Count || MaSP.Count != DonGia.Count || MaSP.Count != MarkupRatio.Count)
        {
            ModelState.AddModelError("", "Dữ liệu gửi lên không hợp lệ hoặc không khớp.");
            ViewBag.NhaCungCaps = await _context.NhaCungCaps.ToListAsync();
            ViewBag.SanPhams = await _context.SanPhams.ToListAsync();
            return View("~/Adminboot/Admin/Views/NhapKhoSP/Index.cshtml");
        }


        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            decimal chiPhiVaChietKhauRong = ChiPhiPhatSinh - ChietKhau;
            decimal tongHoaDonThucTe = TongTien + chiPhiVaChietKhauRong;
            decimal conNoTinhToan = tongHoaDonThucTe - DaThanhToan;

            var phieuNhap = new NhapKho
            {
                // ... (giữ nguyên thông tin phiếu nhập)
                MaNCC = MaNCC,
                NgayNhap = NgayNhap,
                MaTK = MaNguoiNhap,
                GhiChu = GhiChu,
                TongTien = TongTien,
                ChiPhiPhatSinh = ChiPhiPhatSinh,
                ChietKhau = ChietKhau,
                TongHoaDonThucTe = tongHoaDonThucTe,
                DaThanhToan = DaThanhToan,
                ConNo = conNoTinhToan,
                HinhThucThanhToan = HinhThucThanhToan
            };

            _context.NhapKhos.Add(phieuNhap);
            await _context.SaveChangesAsync();

            int maNhapKho = phieuNhap.MaNhapKho;

            for (int i = 0; i < MaSP.Count; i++)
            {
                int soLuongNhap = (int)Math.Floor(SoLuong[i] ?? 0);
                decimal donGiaNhap = DonGia[i] ?? 0;
                decimal? newMarkup = MarkupRatio[i]; // <<< THÊM 3: Lấy markup mới từ form

                if (soLuongNhap <= 0 || donGiaNhap <= 0 || !newMarkup.HasValue || newMarkup.Value <= 0)
                    continue; // Bỏ qua nếu S/L, Giá, hoặc Markup không hợp lệ

                // ... (tính toán GiaVonDonViMoi - giữ nguyên)
                decimal thanhTienGoc = soLuongNhap * donGiaNhap;
                decimal tyLePhanBo = (TongTien > 0) ? (thanhTienGoc / TongTien) : 0;
                decimal chiPhiPhanBo = chiPhiVaChietKhauRong * tyLePhanBo;
                decimal tongGiaTriVonNhapMoi = thanhTienGoc + chiPhiPhanBo;
                decimal giaVonDonViMoi = (soLuongNhap > 0) ? (tongGiaTriVonNhapMoi / soLuongNhap) : 0;

                var chiTiet = new ChiTietNhapKho
                {
                    // ... (thêm chi tiết nhập kho - giữ nguyên)
                    MaNhapKho = maNhapKho,
                    MaSP = MaSP[i],
                    SoLuong = soLuongNhap,
                    DonGia = donGiaNhap,
                    GiaVonDonVi = giaVonDonViMoi,
                    ThanhTien = thanhTienGoc
                };
                _context.ChiTietNhapKhos.Add(chiTiet);

                var sp = await _context.SanPhams.FindAsync(MaSP[i]);
                if (sp != null)
                {
                    // ... (tính toán GiaGocTrungBinhMoi - giữ nguyên)
                    int soLuongTonHienTai = sp.SoLuong ?? 0;
                    decimal giaGocHienTai = sp.GiaGoc ?? 0;
                    decimal tongGiaTriTonHienTai = soLuongTonHienTai * giaGocHienTai;
                    int tongSoLuongMoi = soLuongTonHienTai + soLuongNhap;
                    decimal tongGiaTriKhoMoi = tongGiaTriTonHienTai + tongGiaTriVonNhapMoi;
                    decimal giaGocTrungBinhMoi = (tongSoLuongMoi > 0) ? (tongGiaTriKhoMoi / tongSoLuongMoi) : 0;

                    sp.SoLuong = tongSoLuongMoi;
                    sp.GiaGoc = giaGocTrungBinhMoi;

                    // --- SỬA LOGIC CẬP NHẬT MARKUP VÀ GIÁ BÁN ---

                    // 1. Cập nhật MarkupRatio mới vào sản phẩm
                    sp.MarkupRatio = newMarkup.Value;

                    // 2. Tính Giá Bán MỚI dựa trên MarkupRatio MỚI
                    if (sp.TuDongCapNhatGiaBan ?? true)
                    {
                        // Dùng newMarkup.Value thay vì markup cũ
                        decimal giaBanMoi = giaGocTrungBinhMoi * newMarkup.Value;
                        decimal giaBanMoiLamTron = Math.Ceiling(giaBanMoi / 1000) * 1000;
                        sp.DonGia = giaBanMoiLamTron;
                    }
                    // --- KẾT THÚC SỬA ---
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["SuccessMessage"] = "Nhập kho thành công! Giá vốn và giá bán đã được cập nhật.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message + (ex.InnerException != null ? " -> " + ex.InnerException.Message : ""));

            ViewBag.NhaCungCaps = await _context.NhaCungCaps.ToListAsync();
            ViewBag.SanPhams = await _context.SanPhams.ToListAsync();
            return View("~/Adminboot/Admin/Views/NhapKhoSP/Index.cshtml");
        }
    }
}