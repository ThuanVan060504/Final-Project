using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_Project.Controllers.Menu
{
    public class SanPhamController : Controller
    {
        private readonly AppDbContext _context;

        public SanPhamController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string category, string search, string sort, int page = 1)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }
            int pageSize = 6;

            var products = _context.SanPhams
                .Include(p => p.DanhMuc)
                .AsQueryable();

            // Lọc theo danh mục
            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.DanhMuc.TenDanhMuc == category);
            }

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.TenSP.Contains(search));
            }

            // Sắp xếp
            switch (sort)
            {
                case "asc":
                    products = products.OrderBy(p => p.DonGia);
                    break;
                case "desc":
                    products = products.OrderByDescending(p => p.DonGia);
                    break;
                default:
                    products = products.OrderBy(p => p.MaSP); // Mặc định
                    break;
            }

            // Tính tổng số trang
            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Lấy sản phẩm theo trang
            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            // Tạo dictionary lưu số lượng đã bán theo MaSP
            var soldQuantities = _context.ChiTietDonHangs
                .GroupBy(ct => ct.MaSP)
                .Select(g => new { MaSP = g.Key, SoLuongDaBan = g.Sum(ct => ct.SoLuong) })
                .ToDictionary(x => x.MaSP, x => x.SoLuongDaBan);

            // Truyền sang ViewBag
            ViewBag.SoLuongDaBan = soldQuantities;


            // Truyền thông tin phân trang ra View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

          
            return View(pagedProducts);
        }


        public IActionResult Details(int id)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }

            var sp = _context.SanPhams
                .Include(s => s.DanhMuc)
                .FirstOrDefault(s => s.MaSP == id);

            if (sp == null)
                return NotFound();

            // 🔥 Kiểm tra sản phẩm có trong FlashSale không
            var flashSale = _context.FlashSales
                .Include(f => f.SanPham)
                .FirstOrDefault(f => f.MaSP == id && f.ThoiGianKetThuc > DateTime.Now);

            if (flashSale != null)
            {
                ViewBag.FlashSale = flashSale;
                ViewBag.GiaSauGiam = flashSale.GiaKhuyenMai;
            }
            else
            {
                ViewBag.FlashSale = null;
            }

            // Sản phẩm tương tự
            var tuongTu = _context.SanPhams
                .Where(s => s.DanhMuc == sp.DanhMuc && s.MaSP != sp.MaSP)
                .Take(4)
                .ToList();

            // Đánh giá
            var danhGiaList = _context.DanhGias
                .Where(d => d.SanPhamId == id)
                .OrderByDescending(d => d.ThoiGian)
                .ToList();

            double diemTrungBinh = danhGiaList.Any()
                ? Math.Round(danhGiaList.Average(d => d.Diem), 1)
                : 0;

            ViewBag.DiemTrungBinh = diemTrungBinh;
            ViewBag.SanPhamTuongTu = tuongTu;
            ViewBag.DanhGiaList = danhGiaList;

            return View(sp);
        }


        [HttpPost]
        public IActionResult GuiDanhGia(int SanPhamId, int Diem, string BinhLuan)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "TaiKhoan"); // hoặc trả lỗi
            }

            var danhGia = new DanhGia
            {
                SanPhamId = SanPhamId,
                TenNguoiDung = User.Identity.Name, // Lấy tên từ người dùng đăng nhập
                Diem = Diem,
                BinhLuan = BinhLuan,
                ThoiGian = DateTime.Now
            };

            _context.DanhGias.Add(danhGia);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = SanPhamId });
        }

        public IActionResult Search(string keyword, int page = 1)
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return RedirectToAction("Index");
            }

            int pageSize = 6;

            var products = _context.SanPhams
                .Include(p => p.DanhMuc)
                .Where(p => p.TenSP.Contains(keyword))
                .OrderBy(p => p.MaSP);

            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var soldQuantities = _context.ChiTietDonHangs
                .GroupBy(ct => ct.MaSP)
                .Select(g => new { MaSP = g.Key, SoLuongDaBan = g.Sum(ct => ct.SoLuong) })
                .ToDictionary(x => x.MaSP, x => x.SoLuongDaBan);

            ViewBag.SoLuongDaBan = soldQuantities;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword;

            return View("Index", pagedProducts);
        }


    }
}
