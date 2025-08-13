using Final_Project.Areas.Admin.Controllers;
using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Final_Project.Adminboot.Admin.Controllers.KhachHang
{
    [Area("Admin")]
    public class KhachHangController : BaseAdminController
    {
        private readonly AppDbContext _context;

        public KhachHangController(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Chỉ lấy tài khoản có VaiTro là "KhachHang"
            var khachHangs = _context.TaiKhoans
                                .Where(kh => kh.VaiTro == "Customer")
                                .ToList();

            return View("~/Adminboot/Admin/Views/KhachHang/Index.cshtml", khachHangs);
        }
    }
}
