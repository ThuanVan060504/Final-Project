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

        // 🟢 Lấy thông tin user + danh mục chung
        private void LoadCommonData()
        {
            int? maTK = HttpContext.Session.GetInt32("MaTK");
            if (maTK != null)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTK == maTK);
                ViewBag.Avatar = taiKhoan?.Avatar;
                ViewBag.HoTen = taiKhoan?.HoTen;
            }

            // ✅ Lấy tất cả danh mục kèm số sản phẩm
            ViewBag.DanhMucs = _context.DanhMucs
                .Include(d => d.SanPhams)
                .ToList();
        }

        public IActionResult Index(string category, string search, string sort, int page = 1)
        {
            LoadCommonData(); // gọi hàm chung

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
                    products = products.OrderBy(p => p.MaSP);
                    break;
            }

            // Phân trang
            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Số lượng đã bán
            var soldQuantities = _context.ChiTietDonHangs
                .GroupBy(ct => ct.MaSP)
                .Select(g => new { MaSP = g.Key, SoLuongDaBan = g.Sum(ct => ct.SoLuong) })
                .ToDictionary(x => x.MaSP, x => x.SoLuongDaBan);

            ViewBag.SoLuongDaBan = soldQuantities;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedProducts);
        }

        public IActionResult Details(int id)
        {
            LoadCommonData();

            var sp = _context.SanPhams
                .Include(s => s.DanhMuc)
                .FirstOrDefault(s => s.MaSP == id);

            if (sp == null) return NotFound();

            // FlashSale
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
                return RedirectToAction("Login", "TaiKhoan");
            }

            var danhGia = new DanhGia
            {
                SanPhamId = SanPhamId,
                TenNguoiDung = User.Identity.Name,
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
            LoadCommonData();

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
        // GET: /SanPham/QuickView?id=123
        [HttpGet]
        public IActionResult QuickView(int id)
        {
            // 1. Tìm sản phẩm (tải kèm DanhMuc để hiển thị)
            var sanPham = _context.SanPhams
                                  .Include(sp => sp.DanhMuc)
                                  .FirstOrDefault(sp => sp.MaSP == id);

            // 2. Nếu không tìm thấy
            if (sanPham == null)
            {
                // Trả về lỗi 404, AJAX sẽ bắt được trong 'error:'
                return NotFound("<p class='text-danger text-center'>Không tìm thấy sản phẩm.</p>");
            }

            // 3. Trả về một PartialView (HTML)
            // QUAN TRỌNG: Tên file PartialView phải khớp với tên bạn sẽ tạo
            return PartialView("_QuickViewPartial", sanPham);
        }
    }
}
