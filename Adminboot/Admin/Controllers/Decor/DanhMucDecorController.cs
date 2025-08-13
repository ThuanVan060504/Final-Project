using Final_Project.Areas.Admin.Controllers;
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;
namespace Final_Project.Adminboot.Admin.Controllers.Decor
{
    [Area("Admin")]

    public class DanhMucDecorController : BaseAdminController
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; 

        // ✅ Inject cả ApplicationDbContext và IWebHostEnvironment
        public DanhMucDecorController(AppDbContext context, IWebHostEnvironment env) : base(context)
        {
            _context = context;
            _env = env; // ✅ gán vào biến
        }

        public IActionResult Index()
        {
            var list = _context.DanhMucDecors.ToList();
            return View("~/Adminboot/Admin/Views/DanhMucDecor/Index.cshtml",list);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string tenDanhMuc, IFormFile logoFile)
        {
            string logoPath = null;

            if (logoFile != null && logoFile.Length > 0)
            {
                string uploadFolder = Path.Combine(_env.WebRootPath, "uploads/danhmuc"); 
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                string fileName = Path.GetFileNameWithoutExtension(logoFile.FileName)
                                  + "_" + Guid.NewGuid().ToString().Substring(0, 6)
                                  + Path.GetExtension(logoFile.FileName);

                string filePath = Path.Combine(uploadFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(stream);
                }

                logoPath = "/uploads/danhmuc/" + fileName;
            }

            var dm = new DanhMucDecor
            {
                TenDanhMuc = tenDanhMuc,
                Logo = logoPath
            };

            _context.DanhMucDecors.Add(dm);
            await _context.SaveChangesAsync();

            return RedirectToAction("~/Adminboot/Admin/Views/DanhMucDecor/Index.cshtml");
        }
    }
}