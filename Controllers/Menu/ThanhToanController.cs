// Controllers/ThanhToanController.cs
using Final_Project.Models.Momo;
using Final_Project.Models.PayPal;
using Final_Project.Models.Shop;
using Final_Project.Models.VnPay;
using Final_Project.Service.VnPay;
using Final_Project.Services;
using Final_Project.Services.PayPal;
using Microsoft.AspNetCore.Http; // Cần cho Session
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json; // Cần cho Session
using System.Linq;
using System.Text;
using Final_Project.Models.User;
using System.Threading.Tasks; // Cần cho async Task
using System.Collections.Generic; // Cần cho List<>
using System; // Cần cho DateTime

namespace Final_Project.Controllers
{
    // == MODEL DÙNG CHO VIỆC THÊM ĐỊA CHỈ MỚI TỪ GHN ==
    // (Bạn có thể chuyển file này ra thư mục Models nếu muốn)
    public class DiaChiMoiViewModel
    {
        public string TenNguoiNhan { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChiChiTiet { get; set; }
        public string ProvinceID { get; set; }
        public string ProvinceName { get; set; }
        public string DistrictID { get; set; }
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
        }

        // [GET] /ThanhToan/LayDanhSachDiaChi
        // CẬP NHẬT: Trả về cả ProvinceID, DistrictID, WardCode
        [HttpGet]
        public IActionResult LayDanhSachDiaChi()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            // TODO: Đảm bảo Model "DiaChiNguoiDung" của bạn đã có 3 cột:
            // ProvinceID (int), DistrictID (int), WardCode (string)
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
                    // --- CẬP NHẬT BẮT BUỘC ---
                    d.ProvinceID,
                    d.DistrictID,
                    d.WardCode
                })
                .ToList();

            return Json(new { success = true, data = danhSach });
        }

        // [POST] /ThanhToan/ThemDiaChiMoi
        // MỚI: Thêm Action để lưu địa chỉ mới (từ form GHN)
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
                    ProvinceID = model.ProvinceID,
                    DistrictID = model.DistrictID,
                    WardCode = model.WardCode,
                    MacDinh = false // Không set mặc định khi thêm mới
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

        // [GET] /ThanhToan/ChonDiaChi
        // MỚI: Action để xử lý khi người dùng bấm "Chọn địa chỉ này"
        [HttpGet]
        public IActionResult ChonDiaChi(int maDiaChi)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            // Lấy danh sách địa chỉ của user
            var diaChiList = _context.DiaChiNguoiDungs.Where(d => d.MaTK == maTK).ToList();

            // Bỏ chọn tất cả địa chỉ cũ
            foreach (var dc in diaChiList)
            {
                dc.MacDinh = false;
            }

            // Chọn địa chỉ mới
            var diaChiMoi = diaChiList.FirstOrDefault(dc => dc.MaDiaChi == maDiaChi);
            if (diaChiMoi != null)
            {
                diaChiMoi.MacDinh = true;
                _context.SaveChanges();
            }

            // Tải lại trang thanh toán (Index GET)
            // Trang sẽ tự động đọc lại địa chỉ mặc định MỚI
            // và tự động tính lại phí vận chuyển.
            return RedirectToAction("Index");
        }

        // [GET] /ThanhToan/Index
        // MỚI: Tải trang thanh toán (sử dụng Session)
        [HttpGet]
        public IActionResult Index()
        {
            GanThongTinNguoiDung();
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            // Lấy danh sách sản phẩm đã chọn từ Session
            var chonSPJson = HttpContext.Session.GetString("ChonSP");
            if (string.IsNullOrEmpty(chonSPJson))
            {
                TempData["Error"] = "Vui lòng chọn sản phẩm từ giỏ hàng.";
                return RedirectToAction("Index", "GioHang");
            }
            var chonSP = JsonConvert.DeserializeObject<List<int>>(chonSPJson);

            // --- Lặp lại logic của XacNhanThanhToan ---
            var diaChiMacDinh = _context.DiaChiNguoiDungs
                .FirstOrDefault(d => d.MaTK == maTK && d.MacDinh);

            if (diaChiMacDinh == null)
            {
                TempData["Error"] = "⚠ Bạn chưa thiết lập địa chỉ mặc định. Vui lòng thêm hoặc chọn một địa chỉ.";
                // Cho phép vào trang, nhưng view sẽ xử lý (JS sẽ báo lỗi không tính được phí)
                // Hoặc bạn có thể redirect về giỏ hàng nếu muốn
                // return RedirectToAction("Index", "GioHang");
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

            // TODO: Đảm bảo Model "DiaChiNguoiDung" đã có 3 cột:
            // ProvinceID (int), DistrictID (int), WardCode (string)
            ViewBag.DiaChi = diaChiMacDinh;
            ViewBag.TongTien = gioHang.Sum(g => g.ThanhTien);

            return View("Index", gioHang); // Trả về view Index.cshtml
        }


        // [POST] /ThanhToan/XacNhanThanhToan
        // CẬP NHẬT: Lưu `chonSP` vào Session và gọi Action [GET] Index
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

            // Lưu danh sách SP vào Session
            var chonSPJson = JsonConvert.SerializeObject(chonSP);
            HttpContext.Session.SetString("ChonSP", chonSPJson);

            // Chuyển đến Action [GET] Index để tải trang
            return RedirectToAction("Index");
        }


        // [POST] /ThanhToan/ThanhToan
        // CẬP NHẬT: Thêm `shippingFee` và thay thế phí gán cứng
        [HttpPost]
        public async Task<IActionResult> ThanhToan(List<int> chonSP, string paymentMethod, decimal shippingFee)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return RedirectToAction("Login", "Auth");

            // Không cần lấy TaiKhoan ở đây vì phương thức COD không dùng
            // var TaiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK.Value);
            // if (TaiKhoan == null) return RedirectToAction("Login", "Auth");

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

            decimal tongTien = gioHang.Sum(gh =>
            {
                var sanPham = _context.SanPhams.First(sp => sp.MaSP == gh.MaSP);
                return gh.SoLuong * sanPham.DonGia;
            });

            string trangThaiThanhToan = "ChuaThanhToan"; // Mặc định cho COD
            string phuongThuc = "Thanh toán khi nhận hàng";

            if (paymentMethod != "COD")
            {
                // Các phương thức khác sẽ được xử lý riêng (Momo, VNPAY, Paypal)
                // và không nên gọi Action này.
                // Tuy nhiên, nếu có gọi, ta gán đúng tên.
                phuongThuc = paymentMethod;
            }

            var donHang = new DonHang
            {
                MaTK = maTK.Value,
                MaDiaChi = diaChiMacDinh.MaDiaChi,
                NgayDat = DateTime.Now,
                NgayYeuCau = DateTime.Now.AddDays(3),
                PhiVanChuyen = shippingFee, // <-- CẬP NHẬT
                TongTien = tongTien + shippingFee, // <-- CẬP NHẬT
                GiamGia = 0,
                PhuongThucThanhToan = phuongThuc,
                TrangThaiThanhToan = trangThaiThanhToan,
                TrangThaiDonHang = "DangXuLy",
                GhiChu = null
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync(); // Dùng async

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

            _context.GioHangs.RemoveRange(gioHang);
            await _context.SaveChangesAsync(); // Dùng async

            // Gửi email xác nhận đơn hàng
            await SendOrderConfirmationEmail(donHang.MaDonHang);

            TempData["Success"] = "Đặt hàng (COD) thành công!";
            return RedirectToAction("Index", "GioHang");
        }


        // [GET] /ThanhToan/TaoMomoQRCode
        // CẬP NHẬT: Nhận `tongTien` (đã bao gồm phí) và `shippingFee` từ view
        [HttpGet]
        public async Task<IActionResult> TaoMomoQRCode(List<int> chonSP, decimal tongTien, decimal shippingFee)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            // ================== FIX CS0103 START ==================
            // Lỗi CS0103 xảy ra ở dưới vì biến 'TaiKhoan' chưa được khai báo.
            // Cần lấy thông tin 'TaiKhoan' từ 'maTK' (lấy từ Session).
            var TaiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK.Value);

            // Rất quan trọng: Kiểm tra xem có tìm thấy tài khoản không
            if (TaiKhoan == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tài khoản. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Auth");
            }
            // =================== FIX CS0103 END ===================

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

            var donHang = new DonHang
            {
                MaTK = maTK.Value,
                MaDiaChi = diaChi.MaDiaChi,
                NgayDat = DateTime.Now,
                NgayYeuCau = DateTime.Now.AddDays(3),
                PhiVanChuyen = shippingFee, // <-- CẬP NHẬT
                TongTien = tongTien, // <-- CẬP NHẬT (Lấy tổng tiền cuối cùng từ view)
                GiamGia = 0,
                PhuongThucThanhToan = "Ví MOMO",
                TrangThaiThanhToan = "ChuaThanhToan", // <-- CẬP NHẬT (Callback sẽ đổi thành DaThanhToan)
                TrangThaiDonHang = "DangXuLy"
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

            _context.GioHangs.RemoveRange(gioHang);
            await _context.SaveChangesAsync();

            // Gửi email xác nhận
            await SendOrderConfirmationEmail(donHang.MaDonHang);

            // Tạo QR MOMO
            var orderInfo = new OrderInfoModel
            {
                FullName = "KH " + TaiKhoan.HoTen, // <-- Dòng này đã hợp lệ sau khi fix
                OrderId = donHang.MaDonHang.ToString(), // Gửi MaDonHang
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

        // [GET] /ThanhToan/TaoVnpayQRCode
        // CẬP NHẬT: Nhận `tongTien` (đã bao gồm phí) và `shippingFee` từ view
        [HttpGet]
        public async Task<IActionResult> TaoVnpayQRCode(List<int> chonSP, decimal tongTien, decimal shippingFee)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            // Không cần lấy TaiKhoan ở đây vì VNPAY không dùng TaiKhoan.HoTen
            // var TaiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK.Value);
            // if (TaiKhoan == null) return RedirectToAction("Login", "Auth");

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

            var donHang = new DonHang
            {
                MaTK = maTK.Value,
                MaDiaChi = diaChi.MaDiaChi,
                NgayDat = DateTime.Now,
                NgayYeuCau = DateTime.Now.AddDays(3),
                PhiVanChuyen = shippingFee, // <-- CẬP NHẬT
                TongTien = tongTien, // <-- CẬP NHẬT
                GiamGia = 0,
                PhuongThucThanhToan = "Ví VNPAY",
                TrangThaiThanhToan = "ChuaThanhToan", // <-- CẬP NHẬT
                TrangThaiDonHang = "DangXuLy"
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

            _context.GioHangs.RemoveRange(gioHang);
            await _context.SaveChangesAsync();

            // Gửi email xác nhận
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

        // [GET] /ThanhToan/TaoPaypalPayment
        // CẬP NHẬT: Nhận `tongTien` (đã bao gồm phí) và `shippingFee` từ view
        [HttpGet]
        public async Task<IActionResult> TaoPaypalPayment(List<int> chonSP, decimal tongTien, decimal shippingFee)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return RedirectToAction("Login", "Auth");

            // Không cần lấy TaiKhoan ở đây vì PayPal không dùng TaiKhoan.HoTen
            // var TaiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK.Value);
            // if (TaiKhoan == null) return RedirectToAction("Login", "Auth");

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

            var donHang = new DonHang
            {
                MaTK = maTK.Value,
                MaDiaChi = diaChi.MaDiaChi,
                NgayDat = DateTime.Now,
                NgayYeuCau = DateTime.Now.AddDays(3),
                PhiVanChuyen = shippingFee, // <-- CẬP NHẬT
                TongTien = tongTien, // <-- CẬP NHẬT
                GiamGia = 0,
                PhuongThucThanhToan = "PayPal",
                TrangThaiThanhToan = "ChuaThanhToan", // <-- CẬP NHẬT
                TrangThaiDonHang = "DangXuLy"
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

            _context.GioHangs.RemoveRange(gioHang);
            await _context.SaveChangesAsync();

            // Gửi email xác nhận
            await SendOrderConfirmationEmail(donHang.MaDonHang);

            // Lưu vào Session để dùng khi callback
            HttpContext.Session.SetInt32("CurrentOrderId", donHang.MaDonHang);

            // Quy đổi VNĐ → USD (Bạn nên lấy tỷ giá động, đây là ví dụ)
            decimal tyGia = 25000m;
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
        // == CÁC ACTION CALLBACK (Không cần sửa, đã xử lý đúng logic) ==
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
                await SendOrderConfirmationEmail(maDonHang);
                TempData["Success"] = "✅ Đơn hàng đã thanh toán thành công.";
            }
            else
            {
                // Không gửi email, chỉ báo lỗi
                TempData["Error"] = "❌ Thanh toán MOMO thất bại hoặc bị hủy.";

                // Cập nhật trạng thái thất bại nếu muốn
                // donHang.TrangThaiThanhToan = "ThanhToanLoi";
                // _context.SaveChanges();
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

                        // Gửi email xác nhận
                        await SendOrderConfirmationEmail(maDonHang);
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

                    await SendOrderConfirmationEmail(maDonHang);
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


        // =================== HÀM GỬI EMAIL (Không đổi) ===================
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
                    case "COD":
                        phuongThucThanhToan = "Thanh toán khi nhận hàng (COD)";
                        break;
                    case "VÍ MOMO":
                        phuongThucThanhToan = "Thanh toán qua Ví MoMo";
                        break;
                    case "VÍ VNPAY":
                        phuongThucThanhToan = "Thanh toán qua VNPay";
                        break;
                    case "PAYPAL":
                        phuongThucThanhToan = "Thanh toán qua PayPal";
                        break;
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
                             <tr>
                                <td colspan'4' style='padding:8px;font-size:1.1em;'>Tổng cộng:</td>
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