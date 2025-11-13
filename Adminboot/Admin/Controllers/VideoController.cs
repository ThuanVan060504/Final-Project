using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http; // Cần thêm IFormFile

namespace Final_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VideoController : BaseAdminController
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VideoController(AppDbContext context, IWebHostEnvironment webHostEnvironment) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Admin/Video
        public async Task<IActionResult> Index()
        {
            var videos = await _context.Videos
                .Include(v => v.SanPham)
                .OrderByDescending(v => v.NgayTao)
                .ToListAsync();

            // Dùng đúng đường dẫn view từ lần trước
            return View("~/Adminboot/Admin/Views/Video/Index.cshtml", videos);
        }

        // GET: /Admin/Video/Create
        public IActionResult Create()
        {
            // Cung cấp danh sách sản phẩm cho form
            // Dùng cú pháp giống SanPhamController
            ViewBag.SanPhamList = new SelectList(_context.SanPhams.OrderBy(s => s.TenSP), "MaSP", "TenSP");
            return View("~/Adminboot/Admin/Views/Video/Create.cshtml");
        }

        // POST: /Admin/Video/Create
        // === THAY ĐỔI: Sử dụng tham số riêng lẻ, không dùng model binding ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string TieuDe,
            int MaSP,
            IFormFile VideoFile,
            IFormFile ThumbnailFile)
        {
            if (string.IsNullOrEmpty(TieuDe) || MaSP <= 0)
            {
                ModelState.AddModelError("", "Vui lòng nhập Tiêu đề và chọn Sản phẩm.");
                ViewBag.SanPhamList = new SelectList(_context.SanPhams.OrderBy(s => s.TenSP), "MaSP", "TenSP", MaSP);
                return View("~/Adminboot/Admin/Views/Video/Create.cshtml");
            }

            var video = new Video
            {
                TieuDe = TieuDe,
                MaSP = MaSP,
                NgayTao = DateTime.Now
            };

            // Xử lý upload file video
            if (VideoFile != null && VideoFile.Length > 0)
            {
                video.VideoUrl = await SaveFile(VideoFile, "videos");
            }

            // Xử lý upload file thumbnail
            if (ThumbnailFile != null && ThumbnailFile.Length > 0)
            {
                video.ThumbnailUrl = await SaveFile(ThumbnailFile, "thumbnails");
            }

            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            TempData["Success"] = " Thêm video mới thành công!";
            return RedirectToAction("Index");
        }

        // GET: /Admin/Video/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null) return NotFound();

            // Dùng cú pháp giống SanPhamController (thêm tham số thứ 4 để chọn giá trị hiện tại)
            ViewBag.SanPhamList = new SelectList(_context.SanPhams.OrderBy(s => s.TenSP), "MaSP", "TenSP", video.MaSP);
            return View("~/Adminboot/Admin/Views/Video/Edit.cshtml", video);
        }

        // POST: /Admin/Video/Edit/5
        // === THAY ĐỔI: Sử dụng tham số riêng lẻ, không dùng model binding ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int MaVideo,
            string TieuDe,
            int MaSP,
            IFormFile VideoFile,
            IFormFile ThumbnailFile)
        {
            var videoToUpdate = await _context.Videos.FindAsync(MaVideo);
            if (videoToUpdate == null) return NotFound();

            if (string.IsNullOrEmpty(TieuDe) || MaSP <= 0)
            {
                ModelState.AddModelError("", "Vui lòng nhập Tiêu đề và chọn Sản phẩm.");
                ViewBag.SanPhamList = new SelectList(_context.SanPhams.OrderBy(s => s.TenSP), "MaSP", "TenSP", MaSP);
                return View("~/Adminboot/Admin/Views/Video/Edit.cshtml", videoToUpdate);
            }

            // Cập nhật thông tin
            videoToUpdate.TieuDe = TieuDe;
            videoToUpdate.MaSP = MaSP;

            // Xử lý upload file video mới (nếu có)
            if (VideoFile != null && VideoFile.Length > 0)
            {
                videoToUpdate.VideoUrl = await SaveFile(VideoFile, "videos");
            }

            // Xử lý upload file thumbnail mới (nếu có)
            if (ThumbnailFile != null && ThumbnailFile.Length > 0)
            {
                videoToUpdate.ThumbnailUrl = await SaveFile(ThumbnailFile, "thumbnails");
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi cập nhật video: " + ex.Message;
                ViewBag.SanPhamList = new SelectList(_context.SanPhams.OrderBy(s => s.TenSP), "MaSP", "TenSP", MaSP);
                return View("~/Adminboot/Admin/Views/Video/Edit.cshtml", videoToUpdate);
            }

            TempData["Success"] = " Cập nhật video thành công!";
            return RedirectToAction("Index");
        }

        // POST: /Admin/Video/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null)
            {
                return Json(new { success = false, message = "Không tìm thấy video." });
            }

            try
            {
                _context.Videos.Remove(video);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa video thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa video: " + ex.Message });
            }
        }

        // Hàm helper để lưu file
        private async Task<string> SaveFile(IFormFile file, string subFolder)
        {
            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{subFolder}/{fileName}";
        }
    }
}