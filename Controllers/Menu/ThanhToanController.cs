// Controllers/ThanhToanController.cs
using Final_Project.Models.Momo;
using Final_Project.Models.Shop;
using Final_Project.Models.VnPay;
using Final_Project.Service.VnPay;
using Final_Project.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Final_Project.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMomoService _momoService;

        private readonly IVnPayService _vnPayService;

        public ThanhToanController(AppDbContext context, IMomoService momoService, IVnPayService vnPayService)
        {
            _context = context;
            _momoService = momoService;
            _vnPayService = vnPayService;
        }


        [HttpPost]
        public IActionResult ThanhToan(List<int> chonSP, string paymentMethod)
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
                TempData["Success"] = "Bạn chưa có địa chỉ mặc định.";
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

            if (paymentMethod == "Momo")
            {
                return RedirectToAction("TaoMomoQRCode", "ThanhToan", new { maDonHang = donHang.MaDonHang });
            }

            if (paymentMethod == "VNPAY")
            {
                var paymentModel = new PaymentInformationModel
                {
                    Amount = (double)donHang.TongTien,

                    Name = $"DonHang#{donHang.MaDonHang}",
                    OrderDescription = "Thanh toán đơn hàng",
                    OrderType = "billpayment"
                };

                var url = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);
                return Redirect(url);
            }

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction("Index", "GioHang"); // 🔁 Dòng này đảm bảo tất cả đường dẫn đều có return
        }




        [HttpPost]
        public IActionResult XacNhanThanhToan(List<int> chonSP)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null || chonSP == null || !chonSP.Any())
                return RedirectToAction("Index", "GioHang");

            var diaChiMacDinh = _context.DiaChiNguoiDungs
                .FirstOrDefault(d => d.MaTK == maTK && d.MacDinh);

            if (diaChiMacDinh == null)
            {
                TempData["Error"] = "⚠ Bạn chưa thiết lập địa chỉ mặc định. Vui lòng cập nhật trước khi thanh toán.";
                return RedirectToAction("DanhSachDiaChi", "User");
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
        public IActionResult TaoMomoQRCode(List<int> chonSP, decimal tongTien)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK == null) return RedirectToAction("Login", "Auth");

            string orderId = Guid.NewGuid().ToString();
            string redirectUrl = Url.Action("KetQuaThanhToan", "ThanhToan", new { orderId }, Request.Scheme);

            var orderInfo = new OrderInfoModel
            {
                FullName = "Người dùng " + maTK,
                OrderInfo = "Thanh toán đơn hàng #" + orderId,
                Amount = tongTien.ToString()
            };

            var response = _momoService.CreatePaymentAsync(orderInfo).Result;

            TempData["chonSP"] = string.Join(",", chonSP);
            TempData["orderId"] = orderId;
            TempData["amount"] = tongTien.ToString();

            return Redirect(response.PayUrl);
        }
        public IActionResult KetQuaThanhToan(string orderId, string resultCode, string amount)
        {
            // resultCode = "0" tức là thanh toán thành công
            if (resultCode == "0")
            {
                var chonSPString = TempData["chonSP"] as string;
                if (string.IsNullOrEmpty(chonSPString))
                {
                    TempData["Error"] = "Không thể xác nhận sản phẩm đã chọn.";
                    return RedirectToAction("Index", "GioHang");
                }

                var chonSP = chonSPString.Split(',').Select(int.Parse).ToList();

                // Gọi lại phương thức ThanhToan để lưu đơn hàng (phương thức POST bạn đã làm)
                return ThanhToan(chonSP, "ChuyenKhoan");
            }

            TempData["Error"] = "❌ Thanh toán thất bại hoặc bị hủy.";
            return RedirectToAction("Index", "GioHang");
        }
        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
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
                TempData["Error"] = "❌ Thanh toán thất bại hoặc bị hủy.";
            }

            return RedirectToAction("Index", "GioHang");
        }


    }
}
