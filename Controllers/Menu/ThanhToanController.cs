// Controllers/ThanhToanController.cs
using Final_Project.Models.Momo;
using Final_Project.Models.Shop;
using Final_Project.Models.VnPay;
using Final_Project.Service.VnPay;
using Final_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;

namespace Final_Project.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMomoService _momoService;
        private readonly IVnPayService _vnPayService;
        private readonly IEmailService _emailService; // ✅ THÊM TRƯỜNG NÀY
 
        public ThanhToanController(AppDbContext context, IMomoService momoService, IVnPayService vnPayService, IEmailService emailService)
        {
            _context = context;
            _momoService = momoService;
            _vnPayService = vnPayService;
            _emailService = emailService;
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
                    d.TinhTP
                })
                .ToList();

            return Json(new { success = true, data = danhSach });
        }
        [HttpPost]
        public IActionResult ChonDiaChiMacDinh(int maDiaChi)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            // Xóa mặc định cũ
            var diaChiCu = _context.DiaChiNguoiDungs.FirstOrDefault(dc => dc.MaTK == maTK && dc.MacDinh);
            if (diaChiCu != null)
            {
                diaChiCu.MacDinh = false;
            }

            // Cập nhật mới
            var diaChiMoi = _context.DiaChiNguoiDungs.FirstOrDefault(dc => dc.MaDiaChi == maDiaChi && dc.MaTK == maTK);
            if (diaChiMoi != null)
            {
                diaChiMoi.MacDinh = true;
                _context.SaveChanges();
            }

            return RedirectToAction("ThanhToan"); // hoặc trả về JSON nếu dùng fetch()
        }
        [HttpPost]
        public IActionResult SetDefault(int id)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            var diaChiList = _context.DiaChiNguoiDungs.Where(d => d.MaTK == maTK).ToList();
            foreach (var dc in diaChiList)
            {
                dc.MacDinh = dc.MaDiaChi == id;
            }

            _context.SaveChanges();

            return RedirectToAction("Index", "ThanhToan"); // hoặc action nào chứa view danh sách
        }

        [HttpPost]
        public async Task<IActionResult> ThanhToan(List<int> chonSP, string paymentMethod)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null)
                return RedirectToAction("Login", "Auth");

            if (chonSP == null || !chonSP.Any())
            {
                TempData["Success"] = "Vui lòng chọn sản phẩm để thanh toán.";
                return RedirectToAction("Index", "GioHang");
            }

            var danhSachDiaChi = _context.DiaChiNguoiDungs
                .Where(d => d.MaTK == maTK)
                .ToList();

            var diaChiMacDinh = danhSachDiaChi.FirstOrDefault(d => d.MacDinh);

            if (diaChiMacDinh == null)
            {
                TempData["Error"] = "⚠ Bạn chưa thiết lập địa chỉ mặc định. Vui lòng cập nhật trước khi thanh toán.";
                return RedirectToAction("Index", "GioHang");
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

            string trangThaiThanhToan;
            string phuongThuc;

            switch (paymentMethod)
            {
                case "COD":
                    trangThaiThanhToan = "ChuaThanhToan";
                    phuongThuc = "Thanh toán khi nhận hàng";
                    break;
                case "Momo":
                    trangThaiThanhToan = "DaThanhToan";
                    phuongThuc = "Ví MoMo";
                    break;
                case "VNPAY":
                    trangThaiThanhToan = "ChuaThanhToan";
                    phuongThuc = "Ví VNPAY";
                    break;
                default:
                    TempData["Success"] = "Phương thức thanh toán không hợp lệ.";
                    return RedirectToAction("XacNhanThanhToan", new { chonSP = string.Join(",", chonSP) });
            }

            var donHang = new DonHang
            {
                MaTK = maTK.Value,
                MaDiaChi = diaChiMacDinh.MaDiaChi,
                NgayDat = DateTime.Now,
                NgayYeuCau = DateTime.Now.AddDays(3),
                PhiVanChuyen = 17000,
                TongTien = tongTien + 17000,
                GiamGia = 0,
                PhuongThucThanhToan = phuongThuc,
                TrangThaiThanhToan = trangThaiThanhToan,
                TrangThaiDonHang = "DangXuLy",
                GhiChu = null
            };

            _context.DonHangs.Add(donHang);
            _context.SaveChanges();

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
            _context.SaveChanges();

            // ✅ Gửi email xác nhận đơn hàng
            await SendOrderConfirmationEmail(donHang.MaDonHang);

            if (paymentMethod == "Momo")
            {
                return RedirectToAction("TaoMomoQRCode", "ThanhToan", new { maDonHang = donHang.MaDonHang });
            }

            if (paymentMethod == "VNPAY")
            {
                var paymentModel = new PaymentInformationModel
                {
                    Amount = (decimal)donHang.TongTien,
                    Name = $"DonHang#{donHang.MaDonHang}",
                    OrderDescription = "Thanh toán đơn hàng",
                    OrderType = "billpayment"
                };

                var url = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);
                return Redirect(url);
            }

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction("Index", "GioHang");
        }





        [HttpPost]
        public IActionResult XacNhanThanhToan(List<int> chonSP)
        {
            GanThongTinNguoiDung();
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null || chonSP == null || !chonSP.Any())
                return RedirectToAction("Index", "GioHang");

            var diaChiMacDinh = _context.DiaChiNguoiDungs
                .FirstOrDefault(d => d.MaTK == maTK && d.MacDinh);

            if (diaChiMacDinh == null)
            {
                TempData["Error"] = "⚠ Bạn chưa thiết lập địa chỉ mặc định. Vui lòng cập nhật trước khi thanh toán.";
                return RedirectToAction("Index", "GioHang");
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

            ViewBag.DiaChi = diaChiMacDinh;
            ViewBag.TongTien = gioHang.Sum(g => g.ThanhTien);

            return View("Index", gioHang);
        }

        [HttpGet]
        public async Task<IActionResult> TaoMomoQRCode(List<int> chonSP, decimal tongTien)
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

            decimal tongTienHang = gioHang.Sum(g =>
            {
                var sp = _context.SanPhams.FirstOrDefault(s => s.MaSP == g.MaSP);
                return g.SoLuong * sp.DonGia;
            });

            var donHang = new DonHang
            {
                MaTK = maTK.Value,
                MaDiaChi = diaChi.MaDiaChi,
                NgayDat = DateTime.Now,
                NgayYeuCau = DateTime.Now.AddDays(3),
                PhiVanChuyen = 17000,
                TongTien = tongTienHang + 17000,
                GiamGia = 0,
                PhuongThucThanhToan = "Ví MOMO",
                TrangThaiThanhToan = "DaThanhToan", // Hoặc "ChuaThanhToan" nếu chờ MOMO callback
                TrangThaiDonHang = "DangXuLy"
            };

            _context.DonHangs.Add(donHang);
            _context.SaveChanges();

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
            _context.SaveChanges();

            // ✅ Gửi email xác nhận
            await SendOrderConfirmationEmail(donHang.MaDonHang);

            // Tạo QR MOMO
            var orderInfo = new OrderInfoModel
            {
                FullName = "Người dùng " + maTK,
                OrderInfo = $"Thanh toán đơn hàng #{donHang.MaDonHang}",
                Amount = ((double)donHang.TongTien).ToString()
            };

            var response = await _momoService.CreatePaymentAsync(orderInfo);

            TempData["Success"] = "✅ Vui lòng quét mã QR để hoàn tất thanh toán.";
            return Redirect(response.PayUrl);
        }


        [HttpGet]
        public IActionResult TaoVnpayQRCode(List<int> chonSP, decimal tongTien)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            // 1. Lấy địa chỉ mặc định
            var diaChi = _context.DiaChiNguoiDungs.FirstOrDefault(d => d.MaTK == maTK && d.MacDinh);
            if (diaChi == null)
            {
                TempData["Error"] = "⚠ Bạn chưa có địa chỉ mặc định.";
                return RedirectToAction("Index", "GioHang");
            }

            // 2. Lấy giỏ hàng đã chọn
            var gioHang = _context.GioHangs
                .Where(g => g.MaTK == maTK && chonSP.Contains(g.MaSP))
                .ToList();

            if (!gioHang.Any())
            {
                TempData["Error"] = "Không có sản phẩm nào trong giỏ.";
                return RedirectToAction("Index", "GioHang");
            }

            // 3. Tính tổng tiền đơn hàng
            decimal tongTienHang = gioHang.Sum(g =>
            {
                var sp = _context.SanPhams.FirstOrDefault(s => s.MaSP == g.MaSP);
                return g.SoLuong * sp.DonGia;
            });

            // 4. Tạo đơn hàng
            var donHang = new DonHang
            {
                MaTK = maTK.Value,
                MaDiaChi = diaChi.MaDiaChi,
                NgayDat = DateTime.Now,
                NgayYeuCau = DateTime.Now.AddDays(3),
                PhiVanChuyen = 17000,
                TongTien = tongTienHang + 17000,
                GiamGia = 0,
                PhuongThucThanhToan = "Ví VNPAY",
                TrangThaiThanhToan = "DaThanhToan",
                TrangThaiDonHang = "DangXuLy"
            };

            _context.DonHangs.Add(donHang);
            _context.SaveChanges();

            // 5. Thêm chi tiết đơn hàng và cập nhật kho
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

            // 6. Xóa giỏ hàng
            _context.GioHangs.RemoveRange(gioHang);
            _context.SaveChanges();

            // 7. Gửi request tạo thanh toán VNPAY
            var paymentModel = new PaymentInformationModel
            {
                Amount = (decimal)donHang.TongTien,
                OrderId = donHang.MaDonHang.ToString(),
                Name = $"DonHang#{donHang.MaDonHang}",
                OrderDescription = "Thanh toán đơn hàng",
                OrderType = "billpayment"
            };

            var url = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);

            TempData["Success"] = "✅ Đang chuyển hướng đến VNPAY...";
            return Redirect(url);
        }

        public async Task<IActionResult> KetQuaThanhToan(int maDonHang, string resultCode, string amount)
        {
            Console.WriteLine("📩 Callback MOMO nhận được. Mã đơn: " + maDonHang + " | resultCode: " + resultCode);

            var donHang = _context.DonHangs.FirstOrDefault(x => x.MaDonHang == maDonHang);
            if (donHang == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                Console.WriteLine("❌ Không tìm thấy đơn hàng.");
                return RedirectToAction("Index", "GioHang");
            }

            if (resultCode == "0") // ✅ Thành công
            {
                donHang.TrangThaiThanhToan = "DaThanhToan";
                _context.SaveChanges();

                // ✅ Gửi email xác nhận
                await SendOrderConfirmationEmail(maDonHang);

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

                        // ✅ Gửi email xác nhận
                        await SendOrderConfirmationEmail(maDonHang);

                        TempData["Success"] = "✅ Thanh toán VNPAY thành công!";
                    }
                    else
                    {
                        TempData["Error"] = "❗Không tìm thấy đơn hàng.";
                        Console.WriteLine("❌ Không tìm thấy đơn hàng khi callback VNPAY.");
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


        // =================== HÀM GỬI EMAIL ===================
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
                // Ví dụ: COD, MOMO, VNPAY => hiển thị đẹp hơn
                switch (phuongThucThanhToan.ToUpper())
                {
                    case "COD":
                        phuongThucThanhToan = "Thanh toán khi nhận hàng (COD)";
                        break;
                    case "MOMO":
                        phuongThucThanhToan = "Thanh toán qua Ví MoMo";
                        break;
                    case "VNPAY":
                        phuongThucThanhToan = "Thanh toán qua VNPay";
                        break;
                }

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

                var body = $@"
<div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;border:1px solid #eee;padding:20px;border-radius:10px;'>
    <h2 style='color:#2E86C1;'>G3TD - Xác nhận thanh toán thành công #{donHang.MaDonHang}</h2>
    <p>Xin chào <strong>{taiKhoan.HoTen}</strong>,</p>
    <p>Cảm ơn bạn đã mua hàng tại <strong>G3TD</strong>. Dưới đây là thông tin đơn hàng của bạn:</p>
    
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
    </table>

    <p style='margin-top:15px;'><strong>Phương thức thanh toán:</strong> {phuongThucThanhToan}</p>
    <p><strong>Trạng thái thanh toán:</strong> {donHang.TrangThaiThanhToan}</p>
    <p><strong>Trạng thái đơn hàng:</strong> {donHang.TrangThaiDonHang}</p>
    <p><strong>Tổng cộng:</strong> {donHang.TongTien:N0} VND</p>

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
