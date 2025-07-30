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
                TrangThaiThanhToan = "DaThanhToan", // ✅ Chưa thanh toán
                TrangThaiDonHang = "DangXuLy"
            };

            // 4. Lưu đơn hàng vào database
            _context.DonHangs.Add(donHang);
            _context.SaveChanges();

            // 5. Lưu chi tiết đơn hàng và cập nhật số lượng tồn kho
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

            // 6. Tạo QR MOMO
            var orderInfo = new OrderInfoModel
            {
                FullName = "Người dùng " + maTK,
                OrderInfo = $"Thanh toán đơn hàng #{donHang.MaDonHang}",
                Amount = ((double)donHang.TongTien).ToString()
            };

            var response = _momoService.CreatePaymentAsync(orderInfo).Result;

            TempData["Success"] = "✅ Vui lòng quét mã QR để hoàn tất thanh toán.";
            return Redirect(response.PayUrl);
        }


        public IActionResult KetQuaThanhToan(int maDonHang, string resultCode, string amount)
        {
            var donHang = _context.DonHangs.FirstOrDefault(x => x.MaDonHang == maDonHang);
            if (donHang == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index", "GioHang");
            }

            if (resultCode == "0") // ✅ Thành công
            {
                donHang.TrangThaiThanhToan = "DaThanhToan";
                _context.SaveChanges();

                TempData["Success"] = "✅ Đơn hàng đã thanh toán thành công.";
            }
            else
            {
                TempData["Error"] = "❌ Thanh toán MOMO thất bại hoặc bị hủy. Đơn hàng vẫn được lưu với trạng thái Chưa thanh toán.";
            }

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
