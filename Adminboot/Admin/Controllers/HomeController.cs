using Final_Project.Models.ViewModels;
using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Final_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : BaseAdminController
    {
        private readonly string _connectionString;
        private readonly AppDbContext _context;

        public HomeController(IConfiguration configuration, AppDbContext context) : base(context)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _context = context;
        }

        // 📌 Hàm ghi nhận lượt truy cập
        private void GhiNhanLuotTruyCap()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var userId = HttpContext.Session.GetInt32("MaTK"); // Lấy id từ session nếu có
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var ua = Request.Headers["User-Agent"].ToString();

                using (var cmd = new SqlCommand(
                    "INSERT INTO LuotTruyCap (MaTK, IPAddress, UserAgent) VALUES (@MaTK, @IP, @UA)", con))
                {
                    cmd.Parameters.AddWithValue("@MaTK", (object?)userId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IP", ip ?? "");
                    cmd.Parameters.AddWithValue("@UA", ua ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public IActionResult Index()
        {
            // Ghi nhận lượt truy cập trước khi load dữ liệu dashboard
            GhiNhanLuotTruyCap();

            var model = new DashboardViewModel
            {
                TotalCustomers = 0,
                TotalSuppliers = 0,
                TotalSaleAmount = 0,
                TotalSalesInvoice = 0,
                TotalVisits = 0,
                TotalUserVisits = 0,
                TotalGuestVisits = 0,
                RecentProducts = new List<ProductViewModel>(),
                RecentMessages = new List<Models.Chat.TinNhan>()
            };

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                // Customers
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM TaiKhoan WHERE VaiTro = 'Customer'", con))
                    model.TotalCustomers = (int)cmd.ExecuteScalar();

                // Suppliers
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM NhaCungCap", con))
                    model.TotalSuppliers = (int)cmd.ExecuteScalar();

                // Total Sale Amount
                using (var cmd = new SqlCommand("SELECT ISNULL(SUM(TongTien),0) FROM DonHang", con))
                    model.TotalSaleAmount = Convert.ToDecimal(cmd.ExecuteScalar());

                // Sales Invoice Count
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM DonHang", con))
                    model.TotalSalesInvoice = (int)cmd.ExecuteScalar();

                // Tổng lượt truy cập
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM LuotTruyCap", con))
                    model.TotalVisits = (int)cmd.ExecuteScalar();

                // Lượt truy cập user login
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM LuotTruyCap WHERE MaTK IS NOT NULL", con))
                    model.TotalUserVisits = (int)cmd.ExecuteScalar();

                // Lượt truy cập guest
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM LuotTruyCap WHERE MaTK IS NULL", con))
                    model.TotalGuestVisits = (int)cmd.ExecuteScalar();

                // Recently Added Products
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 5 MaSP, TenSP, DonGia, ImageURL
                    FROM SanPham
                    ORDER BY NgayTao DESC", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.RecentProducts.Add(new ProductViewModel
                            {
                                MaSP = reader.GetInt32(0),
                                TenSP = reader.GetString(1),
                                DonGia = reader.GetDecimal(2),
                                ImageURL = reader.IsDBNull(3) ? "" : reader.GetString(3)
                            });
                        }
                    }
                }
            }

            // 📩 Lấy tin nhắn mới nhất gửi tới admin
            model.RecentMessages = _context.TinNhans
                .Include(t => t.NguoiGui)
                .Where(t => t.NguoiNhan.VaiTro == "Admin")
                .OrderByDescending(t => t.ThoiGianGui)
                .Take(5)
                .ToList();

            return View("~/Adminboot/Admin/Views/Home/Index.cshtml", model);
        }
    }
}
