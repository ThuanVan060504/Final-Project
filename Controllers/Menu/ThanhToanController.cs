// Controllers/ThanhToanController.cs
using Final_Project.Models.Momo;
using Final_Project.Models.PayPal;
using Final_Project.Models.Shop;
using Final_Project.Models.VnPay;
using Final_Project.Service.VnPay;
using Final_Project.Services;
using Final_Project.Services.PayPal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using Final_Project.Models.User;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Security.Claims; // Đã thêm

namespace Final_Project.Controllers
{

    public class DiaChiMoiViewModel
    {
        public string TenNguoiNhan { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChiChiTiet { get; set; }
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; }
        public int DistrictID { get; set; }
        public string DistrictName { get; set; }
        public string WardCode { get; set; }
        public string WardName { get; set; }
    }

    public class ThanhToanController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMomoService _momoService;
        private readonly IVnPayService _vnPayService;
        private readonly IEmailService _emailService;
        private readonly IPayPalService _paypalService;

        public ThanhToanController(AppDbContext context, IMomoService momoService, IVnPayService vnPayService, IEmailService emailService, IPayPalService paypalService)
        {
            _context = context;
            _momoService = momoService;
            _vnPayService = vnPayService;
            _emailService = emailService;
            _paypalService = paypalService;
        }

        private void GanThongTinNguoiDung()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar ?? "Avatar.jpg";
                ViewBag.HoTen = taiKhoan?.HoTen ?? "Ẩn danh";
            }

            var danhMucs = _context.DanhMucs
                .Include(d => d.SanPhams)
                .ToList();

            ViewBag.DanhMucs = danhMucs;
        }

        [HttpGet]
        public IActionResult LayDanhSachDiaChi()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }


            var danhSach = _context.DiaChiNguoiDungs
                .Where(d => d.MaTK == maTK)
                .Select(d => new
                {
                    d.MaDiaChi,
                    d.TenNguoiNhan,
                    d.SoDienThoai,
                    d.DiaChiChiTiet,
                    d.PhuongXa,
                    d.QuanHuyen,
                    d.TinhTP,
                    d.ProvinceID,
                    d.DistrictID,
                    d.WardCode
                })
                .ToList();

            return Json(new { success = true, data = danhSach });
        }


        [HttpPost]
        public async Task<IActionResult> ThemDiaChiMoi([FromBody] DiaChiMoiViewModel model)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập lại." });

            if (model == null)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            try
            {
                var diaChiMoi = new DiaChiNguoiDung
                {
                    MaTK = maTK.Value,
                    TenNguoiNhan = model.TenNguoiNhan,
                    SoDienThoai = model.SoDienThoai,
                    DiaChiChiTiet = model.DiaChiChiTiet,
                    TinhTP = model.ProvinceName,
                    QuanHuyen = model.DistrictName,
                    PhuongXa = model.WardName,

                    ProvinceID = model.ProvinceID.ToString(),
                    DistrictID = model.DistrictID.ToString(),
                    WardCode = model.WardCode,
                    MacDinh = false
                };

                _context.DiaChiNguoiDungs.Add(diaChiMoi);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm địa chỉ mới thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "Lỗi khi lưu địa chỉ." });
            }
        }


        [HttpGet]
        public IActionResult ChonDiaChi(int maDiaChi)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");
            var diaChiList = _context.DiaChiNguoiDungs.Where(d => d.MaTK == maTK).ToList();
            foreach (var dc in diaChiList)
            {
                dc.MacDinh = false;
            }

            var diaChiMoi = diaChiList.FirstOrDefault(dc => dc.MaDiaChi == maDiaChi);
            if (diaChiMoi != null)
            {
                diaChiMoi.MacDinh = true;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Index() // <-- Sửa: Thêm async
        {
            GanThongTinNguoiDung();
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            var chonSPJson = HttpContext.Session.GetString("ChonSP");
            if (string.IsNullOrEmpty(chonSPJson))
            {
                TempData["Error"] = "Vui lòng chọn sản phẩm từ giỏ hàng.";
                return RedirectToAction("Index", "GioHang");
            }

            // ========================================================
            // TẢI VOUCHER CỦA NGƯỜI DÙNG
            // ========================================================
            var now = DateTime.Now;

            // 1. Lấy tất cả voucher người dùng đã lưu (và còn hạn)
            var savedVouchers = await _context.TaiKhoanVouchers
                .Where(tv => tv.MaTK == maTK.Value)
                .Include(tv => tv.Voucher)
                .Select(tv => tv.Voucher)
                .Where(v => v.IsActive == true && v.NgayKetThuc >= now)
                .ToListAsync();

            // 2. Lấy danh sách ID các voucher ĐÃ SỬ DỤNG (và không bị hủy)
            var usedVoucherCounts = await _context.DonHangs
                .Where(d => d.MaTK == maTK.Value &&
                            d.MaVoucherID != null &&
                            d.TrangThaiDonHang != "HuyDon")
                .GroupBy(d => d.MaVoucherID.Value)
                .Select(g => new { VoucherId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.VoucherId, x => x.Count);

            // 3. Lọc danh sách: Chỉ lấy voucher nào có (Số lần đã dùng < Số lần cho phép)
            var availableVouchers = savedVouchers.Where(v =>
            {
                int usedCount = usedVoucherCounts.ContainsKey(v.MaVoucherID) ? usedVoucherCounts[v.MaVoucherID] : 0;
                return usedCount < v.SoLanSuDungToiDaMoiNguoiDung;
            })
            .OrderBy(v => v.NgayKetThuc)
            .ToList();

            ViewBag.MyVouchers = availableVouchers; // Gửi danh sách đã lọc
            // ========================================================

            var chonSP = JsonConvert.DeserializeObject<List<int>>(chonSPJson);

            var diaChiMacDinh = _context.DiaChiNguoiDungs
                .FirstOrDefault(d => d.MaTK == maTK && d.MacDinh);

            if (diaChiMacDinh == null)
            {
                TempData["Error"] = "⚠ Bạn chưa thiết lập địa chỉ mặc định. Vui lòng thêm hoặc chọn một địa chỉ.";
            }

            var gioHang = (from gh in _context.GioHangs
                           join sp in _context.SanPhams on gh.MaSP equals sp.MaSP
                           where gh.MaTK == maTK && chonSP.Contains(sp.MaSP)
                           select new GioHangViewModel
                           {
                               MaSP = sp.MaSP,
                               TenSP = sp.TenSP,
                               SoLuong = gh.SoLuong,
                               DonGia = sp.DonGia,
                               ImageURL = sp.ImageURL
                           }).ToList();

            if (!gioHang.Any())
            {
                TempData["Error"] = "Sản phẩm đã chọn không còn trong giỏ hàng.";
                return RedirectToAction("Index", "GioHang");
            }

            ViewBag.DiaChi = diaChiMacDinh;
            ViewBag.TongTien = gioHang.Sum(g => g.ThanhTien);

            return View("Index", gioHang);
        }


        [HttpPost]
        public IActionResult XacNhanThanhToan(List<int> chonSP)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            if (chonSP == null || !chonSP.Any())
            {
                TempData["Error"] = "Vui lòng chọn sản phẩm để thanh toán.";
                return RedirectToAction("Index", "GioHang");
            }

            var chonSPJson = JsonConvert.SerializeObject(chonSP);
            HttpContext.Session.SetString("ChonSP", chonSPJson);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ApDungVoucher([FromForm] int voucherId, [FromForm] decimal cartTotal)
        {
            var maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
            {
                return Unauthorized(new { success = false, message = "Bạn cần đăng nhập." });
            }

            try
            {
                var now = DateTime.Now;
                // 1. Kiểm tra xem user có sở hữu voucher này VÀ voucher có hợp lệ không
                var userVoucher = await _context.TaiKhoanVouchers
                    .Include(tv => tv.Voucher)
                    .FirstOrDefaultAsync(tv => tv.MaTK == maTK.Value &&
                                                tv.MaVoucherID == voucherId &&
                                                tv.Voucher.IsActive == true &&
                                                tv.Voucher.NgayKetThuc >= now &&
                                                (tv.Voucher.SoLuongToiDa == null || tv.Voucher.SoLuongDaDung < tv.Voucher.SoLuongToiDa));

                if (userVoucher == null)
                {
                    return NotFound(new { success = false, message = "Voucher không hợp lệ hoặc không tìm thấy." });
                }

                var voucher = userVoucher.Voucher;
                // Đếm số đơn hàng (chưa bị hủy) mà user này đã dùng voucher này
                int usedCount = await _context.DonHangs
                    .CountAsync(d => d.MaTK == maTK.Value &&
                                     d.MaVoucherID == voucherId &&
                                     d.TrangThaiDonHang != "HuyDon"); // Rất quan trọng: Không tính đơn đã hủy

                // So sánh với quy tắc (mặc định là 1)
                if (usedCount >= voucher.SoLanSuDungToiDaMoiNguoiDung)
                {
                    return BadRequest(new { success = false, message = "Bạn đã sử dụng voucher này rồi." });
                }
                // 2. Kiểm tra điều kiện đơn hàng tối thiểu (so với tổng tiền hàng)
                if (cartTotal < voucher.DonHangToiThieu)
                {
                    return BadRequest(new { success = false, message = $"Voucher này yêu cầu đơn hàng tối thiểu {voucher.DonHangToiThieu:N0} đ." });
                }

                // 3. Tính toán số tiền giảm giá
                decimal discountAmount = 0;
                if (voucher.LoaiGiamGia == "SoTien")
                {
                    discountAmount = voucher.GiaTriGiam;
                }
                else if (voucher.LoaiGiamGia == "PhanTram")
                {
                    discountAmount = (cartTotal * voucher.GiaTriGiam) / 100;
                    // Kiểm tra giảm giá tối đa
                    if (voucher.GiamGiaToiDa != null && discountAmount > voucher.GiamGiaToiDa.Value)
                    {
                        discountAmount = voucher.GiamGiaToiDa.Value;
                    }
                }

                // Đảm bảo không giảm giá nhiều hơn tổng tiền
                if (discountAmount > cartTotal)
                {
                    discountAmount = cartTotal;
                }

                return Ok(new
                {
                    success = true,
                    message = "Áp dụng voucher thành công!",
                    discountAmount = discountAmount,
                    maCode = voucher.MaCode,
                    maVoucherID = voucher.MaVoucherID
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ThanhToan(List<int> chonSP, string paymentMethod, decimal shippingFee, decimal discountAmount, int maVoucherID)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return RedirectToAction("Login", "Auth");


            if (chonSP == null || !chonSP.Any())
            {
                TempData["Success"] = "Vui lòng chọn sản phẩm để thanh toán.";
                return RedirectToAction("Index", "GioHang");
            }

            var diaChiMacDinh = _context.DiaChiNguoiDungs
                .FirstOrDefault(d => d.MaTK == maTK && d.MacDinh);

            if (diaChiMacDinh == null)
            {
                TempData["Error"] = "⚠ Bạn chưa thiết lập địa chỉ mặc định.";
                return RedirectToAction("Index", "GioHang"); // Quay về giỏ hàng
            }

            var gioHang = _context.GioHangs
                .Where(g => g.MaTK == maTK && chonSP.Contains(g.MaSP))
                .ToList();

            if (!gioHang.Any())
            {
                TempData["Success"] = "Không có sản phẩm nào được chọn.";
                return RedirectToAction("Index", "GioHang");
            }

            decimal tongTienHang = gioHang.Sum(gh =>
            {
                var sanPham = _context.SanPhams.First(sp => sp.MaSP == gh.MaSP);
                return gh.SoLuong * sanPham.DonGia;
            });

            // ========================================================
            // KIỂM TRA BẢO MẬT VOUCHER (COD)
            // ========================================================
            if (maVoucherID > 0)
            {
                var voucher = await _context.Vouchers.FindAsync(maVoucherID);
                if (voucher == null)
                {
                    TempData["Error"] = "Voucher không tồn tại.";
                    return RedirectToAction("Index", "GioHang");
                }

                int usedCount = await _context.DonHangs
                    .CountAsync(d => d.MaTK == maTK.Value &&
                                     d.MaVoucherID == maVoucherID &&
                                     d.TrangThaiDonHang != "HuyDon");

                if (usedCount >= voucher.SoLanSuDungToiDaMoiNguoiDung)
                {
                    TempData["Error"] = "Voucher đã được sử dụng.";
                    return RedirectToAction("Index", "GioHang");
                }
            }
            // ========================================================

            // TỔNG TIỀN CUỐI CÙNG
            decimal tongThanhToan = tongTienHang + shippingFee - discountAmount;
            if (tongThanhToan < 0) tongThanhToan = 0; // Đảm bảo không âm

            string trangThaiThanhToan = "ChuaThanhToan";
            string phuongThuc = "Thanh toán khi nhận hàng";
            if (paymentMethod != "COD") phuongThuc = paymentMethod;

            var donHang = new DonHang
            {
                MaTK = maTK.Value,
                MaDiaChi = diaChiMacDinh.MaDiaChi,
                NgayDat = DateTime.Now,
                NgayYeuCau = DateTime.Now.AddDays(3),
                PhiVanChuyen = shippingFee,
                TongTien = tongThanhToan,     // <-- Tổng cuối cùng
                GiamGia = discountAmount, // <-- Lưu giảm giá
                PhuongThucThanhToan = phuongThuc,
                TrangThaiThanhToan = trangThaiThanhToan,
                TrangThaiDonHang = "DangXuLy",
                MaVoucherID = (maVoucherID > 0) ? maVoucherID : (int?)null // <-- Lưu ID voucher
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            foreach (var item in gioHang)
            {
                var sanPham = _context.SanPhams.First(sp => sp.MaSP == item.MaSP);

                if (item.SoLuong > sanPham.SoLuong)
                {
                    TempData["Success"] = $"Sản phẩm {sanPham.TenSP} không đủ hàng.";
                    return RedirectToAction("Index", "GioHang");
                }

                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSP = item.MaSP,
                    SoLuong = item.SoLuong,
                    DonGia = sanPham.DonGia
                });

                sanPham.SoLuong -= item.SoLuong;
            }

            // CẬP NHẬT SỐ LƯỢNG VOUCHER ĐÃ DÙNG
            if (maVoucherID > 0)
            {
                var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.MaVoucherID == maVoucherID);
                if (voucher != null)
                {
                    voucher.SoLuongDaDung += 1;
                }
            }

            _context.GioHangs.RemoveRange(gioHang);
            await _context.SaveChangesAsync();

            await SendOrderConfirmationEmail(donHang.MaDonHang);

            TempData["Success"] = "Đặt hàng (COD) thành công!";
            return RedirectToAction("Index", "GioHang");
        }


        [HttpGet]
        public async Task<IActionResult> TaoMomoQRCode(List<int> chonSP, decimal tongTien, decimal shippingFee, decimal discountAmount, int maVoucherID)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            var TaiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK.Value);

            if (TaiKhoan == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tài khoản. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Auth");
            }

            var diaChi = _context.DiaChiNguoiDungs.FirstOrDefault(d => d.MaTK == maTK && d.MacDinh);
            if (diaChi == null)
            {
                TempData["Error"] = "⚠ Bạn chưa có địa chỉ mặc định.";
                return RedirectToAction("Index", "GioHang");
            }

            var gioHang = _context.GioHangs
                .Where(g => g.MaTK == maTK && chonSP.Contains(g.MaSP))
                .ToList();

            if (!gioHang.Any())
            {
                TempData["Error"] = "Không có sản phẩm nào trong giỏ.";
                return RedirectToAction("Index", "GioHang");
            }

            // ========================================================
            // ✅ SỬA LỖI: THÊM KIỂM TRA BẢO MẬT VOUCHER (MOMO)
            // ========================================================
            if (maVoucherID > 0)
            {
                var voucher = await _context.Vouchers.FindAsync(maVoucherID);
                if (voucher == null)
                {
                    TempData["Error"] = "Voucher không tồn tại.";
                    return RedirectToAction("Index", "GioHang");
                }

                int usedCount = await _context.DonHangs
                    .CountAsync(d => d.MaTK == maTK.Value &&
                                     d.MaVoucherID == maVoucherID &&
                                     d.TrangThaiDonHang != "HuyDon");

                if (usedCount >= voucher.SoLanSuDungToiDaMoiNguoiDung)
                {
                    TempData["Error"] = "Voucher đã được sử dụng.";
                    return RedirectToAction("Index", "GioHang");
                }
            }
            // ========================================================

            var donHang = new DonHang
            {
                MaTK = maTK.Value,
                MaDiaChi = diaChi.MaDiaChi,
                NgayDat = DateTime.Now,
                NgayYeuCau = DateTime.Now.AddDays(3),
                PhiVanChuyen = shippingFee,
                TongTien = tongTien,         // <-- Tổng cuối cùng
                GiamGia = discountAmount, // <-- Lưu giảm giá
                PhuongThucThanhToan = "Ví MOMO",
                TrangThaiThanhToan = "ChuaThanhToan",
                TrangThaiDonHang = "DangXuLy",
                MaVoucherID = (maVoucherID > 0) ? maVoucherID : (int?)null
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            foreach (var item in gioHang)
            {
                var sp = _context.SanPhams.First(s => s.MaSP == item.MaSP);
                if (item.SoLuong > sp.SoLuong)
                {
                    TempData["Error"] = $"❌ Sản phẩm {sp.TenSP} không đủ hàng.";
                    return RedirectToAction("Index", "GioHang");
                }
                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSP = item.MaSP,
                    SoLuong = item.SoLuong,
                    DonGia = sp.DonGia
                });
                sp.SoLuong -= item.SoLuong;
            }

            // ========================================================
            // ✅ SỬA LỖI: THÊM CẬP NHẬT SỐ LƯỢNG VOUCHER (MOMO)
            // ========================================================
            if (maVoucherID > 0)
            {
                var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.MaVoucherID == maVoucherID);
                if (voucher != null)
                {
                    voucher.SoLuongDaDung += 1;
                }
            }
            // ========================================================

            _context.GioHangs.RemoveRange(gioHang);
            await _context.SaveChangesAsync();

            await SendOrderConfirmationEmail(donHang.MaDonHang);

            // Tạo QR MOMO
            var orderInfo = new OrderInfoModel
            {
                FullName = "KH " + TaiKhoan.HoTen,
                OrderId = donHang.MaDonHang.ToString(),
                OrderInfo = $"Thanh toán đơn hàng #{donHang.MaDonHang}",
                Amount = ((double)donHang.TongTien).ToString()
            };

            var response = await _momoService.CreatePaymentAsync(orderInfo);

            if (response != null && !string.IsNullOrEmpty(response.PayUrl))
            {
                return Redirect(response.PayUrl);
            }

            TempData["Error"] = "Không thể tạo thanh toán Momo.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> TaoVnpayQRCode(List<int> chonSP, decimal tongTien, decimal shippingFee, decimal discountAmount, int maVoucherID)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            var diaChi = _context.DiaChiNguoiDungs.FirstOrDefault(d => d.MaTK == maTK && d.MacDinh);
            if (diaChi == null)
            {
                TempData["Error"] = "⚠ Bạn chưa có địa chỉ mặc định.";
                return RedirectToAction("Index", "GioHang");
            }

            var gioHang = _context.GioHangs
                .Where(g => g.MaTK == maTK && chonSP.Contains(g.MaSP))
                .ToList();

            if (!gioHang.Any())
            {
                TempData["Error"] = "Không có sản phẩm nào trong giỏ.";
                return RedirectToAction("Index", "GioHang");
            }

            // ========================================================
            // ✅ SỬA LỖI: THÊM KIỂM TRA BẢO MẬT VOUCHER (VNPAY)
            // ========================================================
            if (maVoucherID > 0)
            {
                var voucher = await _context.Vouchers.FindAsync(maVoucherID);
                if (voucher == null)
                {
                    TempData["Error"] = "Voucher không tồn tại.";
                    return RedirectToAction("Index", "GioHang");
                }

                int usedCount = await _context.DonHangs
                    .CountAsync(d => d.MaTK == maTK.Value &&
                                     d.MaVoucherID == maVoucherID &&
                                     d.TrangThaiDonHang != "HuyDon");

                if (usedCount >= voucher.SoLanSuDungToiDaMoiNguoiDung)
                {
                    TempData["Error"] = "Voucher đã được sử dụng.";
                    return RedirectToAction("Index", "GioHang");
                }
            }
            // ========================================================

            var donHang = new DonHang
            {
                MaTK = maTK.Value,
                MaDiaChi = diaChi.MaDiaChi,
                NgayDat = DateTime.Now,
                NgayYeuCau = DateTime.Now.AddDays(3),
                PhiVanChuyen = shippingFee,
                TongTien = tongTien,
                GiamGia = discountAmount,
                PhuongThucThanhToan = "Ví VNPAY",
                TrangThaiThanhToan = "ChuaThanhToan",
                TrangThaiDonHang = "DangXuLy",
                MaVoucherID = (maVoucherID > 0) ? maVoucherID : (int?)null
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            foreach (var item in gioHang)
            {
                var sp = _context.SanPhams.First(s => s.MaSP == item.MaSP);
                if (item.SoLuong > sp.SoLuong)
                {
                    TempData["Error"] = $"❌ Sản phẩm {sp.TenSP} không đủ hàng.";
                    return RedirectToAction("Index", "GioHang");
                }
                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSP = item.MaSP,
                    SoLuong = item.SoLuong,
                    DonGia = sp.DonGia
                });
                sp.SoLuong -= item.SoLuong;
            }

            // ========================================================
            // ✅ SỬA LỖI: THÊM CẬP NHẬT SỐ LƯỢNG VOUCHER (VNPAY)
            // ========================================================
            if (maVoucherID > 0)
            {
                var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.MaVoucherID == maVoucherID);
                if (voucher != null)
                {
                    voucher.SoLuongDaDung += 1;
                }
            }
            // ========================================================

            _context.GioHangs.RemoveRange(gioHang);
            await _context.SaveChangesAsync();

            await SendOrderConfirmationEmail(donHang.MaDonHang);

            // Gửi request tạo thanh toán VNPAY
            var paymentModel = new PaymentInformationModel
            {
                Amount = (decimal)donHang.TongTien,
                OrderId = donHang.MaDonHang.ToString(),
                Name = $"DonHang#{donHang.MaDonHang}",
                OrderDescription = "Thanh toán đơn hàng",
                OrderType = "billpayment"
            };

            var url = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);
            return Redirect(url);
        }

        [HttpGet]
        public async Task<IActionResult> TaoPaypalPayment(List<int> chonSP, decimal tongTien, decimal shippingFee, decimal discountAmount, int maVoucherID)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return RedirectToAction("Login", "Auth");

            var diaChi = _context.DiaChiNguoiDungs.FirstOrDefault(d => d.MaTK == maTK && d.MacDinh);
            if (diaChi == null)
            {
                TempData["Error"] = "⚠ Bạn chưa có địa chỉ mặc định.";
                return RedirectToAction("Index", "GioHang");
            }

            var gioHang = _context.GioHangs
                .Where(g => g.MaTK == maTK && chonSP.Contains(g.MaSP))
                .ToList();

            if (!gioHang.Any())
            {
                TempData["Error"] = "Không có sản phẩm nào trong giỏ.";
                return RedirectToAction("Index", "GioHang");
            }

            // ========================================================
            // ✅ SỬA LỖI: THÊM KIỂM TRA BẢO MẬT VOUCHER (PAYPAL)
            // ========================================================
            if (maVoucherID > 0)
            {
                var voucher = await _context.Vouchers.FindAsync(maVoucherID);
                if (voucher == null)
                {
                    TempData["Error"] = "Voucher không tồn tại.";
                    return RedirectToAction("Index", "GioHang");
                }

                int usedCount = await _context.DonHangs
                    .CountAsync(d => d.MaTK == maTK.Value &&
                                     d.MaVoucherID == maVoucherID &&
                                     d.TrangThaiDonHang != "HuyDon");

                if (usedCount >= voucher.SoLanSuDungToiDaMoiNguoiDung)
                {
                    TempData["Error"] = "Voucher đã được sử dụng.";
                    return RedirectToAction("Index", "GioHang");
                }
            }
            // ========================================================

            var donHang = new DonHang
            {
                MaTK = maTK.Value,
                MaDiaChi = diaChi.MaDiaChi,
                NgayDat = DateTime.Now,
                NgayYeuCau = DateTime.Now.AddDays(3),
                PhiVanChuyen = shippingFee,
                TongTien = tongTien,
                GiamGia = discountAmount,
                PhuongThucThanhToan = "PayPal",
                TrangThaiThanhToan = "ChuaThanhToan",
                TrangThaiDonHang = "DangXuLy",
                MaVoucherID = (maVoucherID > 0) ? maVoucherID : (int?)null
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            foreach (var item in gioHang)
            {
                var sp = _context.SanPhams.FirstOrDefault(s => s.MaSP == item.MaSP);
                if (sp != null)
                {
                    if (item.SoLuong > sp.SoLuong)
                    {
                        TempData["Error"] = $"❌ Sản phẩm {sp.TenSP} không đủ hàng.";
                        return RedirectToAction("Index", "GioHang");
                    }
                    var chiTiet = new ChiTietDonHang
                    {
                        MaDonHang = donHang.MaDonHang,
                        MaSP = sp.MaSP,
                        SoLuong = item.SoLuong,
                        DonGia = sp.DonGia
                    };
                    _context.ChiTietDonHangs.Add(chiTiet);
                    sp.SoLuong -= item.SoLuong; // Trừ kho
                }
            }

            // ========================================================
            // ✅ SỬA LỖI: THÊM CẬP NHẬT SỐ LƯỢNG VOUCHER (PAYPAL)
            // ========================================================
            if (maVoucherID > 0)
            {
                var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.MaVoucherID == maVoucherID);
                if (voucher != null)
                {
                    voucher.SoLuongDaDung += 1;
                }
            }
            // ========================================================

            _context.GioHangs.RemoveRange(gioHang);
            await _context.SaveChangesAsync();

            await SendOrderConfirmationEmail(donHang.MaDonHang);

            // Lưu vào Session để dùng khi callback
            HttpContext.Session.SetInt32("CurrentOrderId", donHang.MaDonHang);

            // Quy đổi VNĐ → USD (Bạn nên lấy tỷ giá động, đây là ví dụ)
            decimal tyGia = 26315m;
            decimal amountUsd = Math.Round(donHang.TongTien / tyGia, 2, MidpointRounding.AwayFromZero);

            var returnUrl = Url.Action("PayPalSuccess", "ThanhToan", new { orderId = donHang.MaDonHang }, Request.Scheme);
            var cancelUrl = Url.Action("PayPalCancel", "ThanhToan", new { orderId = donHang.MaDonHang }, Request.Scheme);

            var paymentUrl = await _paypalService.CreatePaymentUrlAsync(
                new PayPalPaymentModel
                {
                    Amount = amountUsd,
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl,
                    Description = $"Thanh toán đơn hàng #{donHang.MaDonHang} - {amountUsd} USD (≈ {donHang.TongTien:N0} VNĐ)"
                },
                HttpContext
            );

            if (!string.IsNullOrEmpty(paymentUrl))
            {
                return Redirect(paymentUrl);
            }

            TempData["Error"] = "Không thể tạo thanh toán PayPal.";
            return RedirectToAction("Index");
        }


        // =================================================================
        // == CÁC ACTION CALLBACK (Không cần sửa) ==
        // =================================================================

        [HttpGet]
        public async Task<IActionResult> KetQuaThanhToan(string orderId, string resultCode)
        {
            Console.WriteLine("📩 Callback MOMO nhận được. Mã đơn: " + orderId + " | resultCode: " + resultCode);

            if (!int.TryParse(orderId, out int maDonHang))
            {
                TempData["Error"] = "Mã đơn hàng Momo không hợp lệ.";
                return RedirectToAction("Index", "GioHang");
            }

            var donHang = _context.DonHangs.FirstOrDefault(x => x.MaDonHang == maDonHang);
            if (donHang == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index", "GioHang");
            }

            if (resultCode == "0") // Thành công
            {
                donHang.TrangThaiThanhToan = "DaThanhToan";
                _context.SaveChanges();

                // Gửi email xác nhận (chỉ khi thành công)
                // await SendOrderConfirmationEmail(maDonHang); // Tạm tắt để tránh spam, bạn có thể mở lại
                TempData["Success"] = "✅ Đơn hàng đã thanh toán thành công.";
            }
            else
            {
                TempData["Error"] = "❌ Thanh toán MOMO thất bại hoặc bị hủy.";
            }

            return RedirectToAction("Index", "GioHang");
        }


        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            Console.WriteLine("📩 Callback VNPAY nhận được: " + Request.QueryString);
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.Success)
            {
                if (int.TryParse(response.OrderId, out int maDonHang))
                {
                    var donHang = _context.DonHangs.FirstOrDefault(x => x.MaDonHang == maDonHang);
                    if (donHang != null)
                    {
                        donHang.TrangThaiThanhToan = "DaThanhToan";
                        _context.SaveChanges();

                        // await SendOrderConfirmationEmail(maDonHang); // Tạm tắt
                        TempData["Success"] = "✅ Thanh toán VNPAY thành công!";
                    }
                    else
                    {
                        TempData["Error"] = "❗Không tìm thấy đơn hàng.";
                    }
                }
                else
                {
                    TempData["Error"] = "❗Mã đơn hàng không hợp lệ.";
                }
            }
            else
            {
                var responseCode = Request.Query["vnp_ResponseCode"].ToString();
                if (responseCode == "24")
                {
                    TempData["Error"] = "🚫 Bạn đã huỷ thanh toán VNPAY.";
                }
                else
                {
                    TempData["Error"] = "❌ Thanh toán VNPAY thất bại.";
                }
            }

            return RedirectToAction("Index", "GioHang");
        }

        [HttpGet]
        public async Task<IActionResult> PayPalSuccess(string orderId)
        {
            var response = await _paypalService.ExecutePaymentAsync(Request.Query);

            if (!int.TryParse(orderId, out int maDonHang))
            {
                var sessionOrderId = HttpContext.Session.GetInt32("CurrentOrderId");
                if (sessionOrderId.HasValue)
                    maDonHang = sessionOrderId.Value;
                else
                {
                    TempData["Error"] = "⚠ Không tìm thấy mã đơn hàng trong phản hồi từ PayPal.";
                    return RedirectToAction("Index", "GioHang");
                }
            }

            if (response.Success)
            {
                var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDonHang == maDonHang);
                if (donHang != null)
                {
                    donHang.TrangThaiThanhToan = "DaThanhToan";
                    _context.SaveChanges();

                    // await SendOrderConfirmationEmail(maDonHang); // Tạm tắt
                    TempData["Success"] = "✅ Thanh toán PayPal thành công!";
                }
                else
                {
                    TempData["Error"] = "❌ Không tìm thấy đơn hàng trong hệ thống.";
                }
            }
            else
            {
                TempData["Error"] = "❌ Thanh toán PayPal thất bại hoặc bị hủy.";
            }

            return RedirectToAction("Index", "GioHang");
        }

        [HttpGet]
        public IActionResult PayPalCancel()
        {
            TempData["Error"] = "🚫 Bạn đã hủy thanh toán PayPal.";
            return RedirectToAction("Index", "GioHang");
        }


        // =================== HÀM GỬI EMAIL (Đã cập nhật logic Giảm giá) ===================
        private async Task SendOrderConfirmationEmail(int maDonHang)
        {
            try
            {
                Console.WriteLine("🚀 Bắt đầu gửi email cho đơn hàng #" + maDonHang);

                var donHang = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(ct => ct.SanPham)
                    .Include(d => d.DiaChiNguoiDung)
                    .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

                if (donHang == null)
                    throw new Exception("Không tìm thấy đơn hàng.");

                var taiKhoan = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.MaTK == donHang.MaTK);

                if (taiKhoan == null || string.IsNullOrEmpty(taiKhoan.Email))
                    throw new Exception("Không tìm thấy email khách hàng.");

                // Xác định phương thức thanh toán
                string phuongThucThanhToan = donHang.PhuongThucThanhToan ?? "Không xác định";
                switch (phuongThucThanhToan.ToUpper())
                {
                    case "COD": phuongThucThanhToan = "Thanh toán khi nhận hàng (COD)"; break;
                    case "VÍ MOMO": phuongThucThanhToan = "Thanh toán qua Ví MoMo"; break;
                    case "VÍ VNPAY": phuongThucThanhToan = "Thanh toán qua VNPay"; break;
                    case "PAYPAL": phuongThucThanhToan = "Thanh toán qua PayPal"; break;
                }

                // Xác định trạng thái thanh toán
                string trangThaiTT = donHang.TrangThaiThanhToan == "DaThanhToan" ? "Đã thanh toán" : "Chưa thanh toán";


                var sb = new StringBuilder();
                foreach (var ct in donHang.ChiTietDonHangs)
                {
                    var imageUrl = string.IsNullOrEmpty(ct.SanPham?.ImageURL)
                        ? "https://via.placeholder.com/80"
                        : ct.SanPham.ImageURL;

                    sb.Append($@"
                <tr>
                    <td style='border:1px solid #ddd;text-align:center;padding:8px;'>
                        <img src='{imageUrl}' width='80' style='border-radius:5px;'/>
                    </td>
                    <td style='border:1px solid #ddd;padding:8px;'>{ct.SanPham?.TenSP}</td>
                    <td style='border:1px solid #ddd;text-align:center;padding:8px;'>{ct.SoLuong}</td>
                    <td style='border:1px solid #ddd;text-align:right;padding:8px;'>{ct.DonGia:N0} VND</td>
                    <td style='border:1px solid #ddd;text-align:right;padding:8px;'>{(ct.SoLuong * ct.DonGia):N0} VND</td>
                </tr>");
                }

                // Tính toán
                decimal tongTienHang = donHang.ChiTietDonHangs.Sum(ct => ct.SoLuong * ct.DonGia);

                // Logic tạo chuỗi giảm giá
                string giamGiaRow = "";
                if (donHang.GiamGia > 0)
                {
                    giamGiaRow = $@"
                <tr style='font-weight:bold; text-align:right; color:red;'>
                    <td colspan='4' style='padding:8px;'>Giảm giá (Voucher):</td>
                    <td style='padding:8px;'>-{donHang.GiamGia:N0} VND</td>
                </tr>";
                }

                var body = $@"
                <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;border:1px solid #eee;padding:20px;border-radius:10px;'>
                    <h2 style='color:#2E86C1;'>G3TD - Xác nhận đơn hàng #{donHang.MaDonHang}</h2>
                    <p>Xin chào <strong>{taiKhoan.HoTen}</strong>,</p>
                    <p>Cảm ơn bạn đã mua hàng tại <strong>G3TD</strong>. Đơn hàng của bạn đã được xác nhận:</p>
            
                    <table style='border-collapse:collapse;width:100%;margin-top:10px;'>
                        <thead>
                            <tr style='background:#f4f4f4;'>
                                <th>Ảnh</th>
                                <th>Sản phẩm</th>
                                <th>Số lượng</th>
                                <th>Đơn giá</th>
                                <th>Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            {sb}
                        </tbody>
                         <tfoot style='font-weight:bold; text-align:right;'>
                            <tr>
                                <td colspan='4' style='padding:8px;'>Tổng tiền hàng:</td>
                                <td style='padding:8px;'>{tongTienHang:N0} VND</td>
                            </tr>
                            <tr>
                                <td colspan='4' style='padding:8px;'>Phí vận chuyển:</td>
                                <td style='padding:8px;'>{donHang.PhiVanChuyen:N0} VND</td>
                            </tr>
                            {giamGiaRow} 
                            <tr style='border-top: 2px solid #ddd;'>
                                <td colspan='4' style='padding:8px;font-size:1.1em;'>Tổng cộng:</td>
                                <td style='padding:8px;font-size:1.1em;color:#D9534F;'>{donHang.TongTien:N0} VND</td>
                            </tr>
                        </tfoot>
                    </table>

                    <h3 style='margin-top:20px;border-bottom:1px solid #eee;padding-bottom:5px;'>Thông tin nhận hàng</h3>
                    <p>
                        <strong>Người nhận:</strong> {donHang.DiaChiNguoiDung.TenNguoiNhan}<br/>
                        <strong>Điện thoại:</strong> {donHang.DiaChiNguoiDung.SoDienThoai}<br/>
                        <strong>Địa chỉ:</strong> {donHang.DiaChiNguoiDung.DiaChiChiTiet}, {donHang.DiaChiNguoiDung.PhuongXa}, {donHang.DiaChiNguoiDung.QuanHuyen}, {donHang.DiaChiNguoiDung.TinhTP}
                    </p>

                    <h3 style='margin-top:20px;border-bottom:1px solid #eee;padding-bottom:5px;'>Thông tin thanh toán</h3>
                    <p><strong>Phương thức:</strong> {phuongThucThanhToan}</p>
                    <p><strong>Trạng thái:</strong> {trangThaiTT}</p>

                    <hr/>
                    <p style='font-size:14px;color:#555;'>
                        Shop G3TD - Nội thất chất lượng<br/>
                        📞 0909 123 456<br/>
                        📧 support@g3td.com
                    </p>
                </div>";

                await _emailService.SendEmailAsync(
                    taiKhoan.Email,
                    $"Xác nhận đơn hàng #{donHang.MaDonHang}",
                    body
                );

                Console.WriteLine("✅ Email đã gửi thành công đến: " + taiKhoan.Email);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi khi gửi email đơn hàng: " + ex.Message);
            }
        }
    }
}