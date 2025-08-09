using Final_Project.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace Final_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly string _connectionString;

        public HomeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                TotalCustomers = 0,
                TotalSuppliers = 0,
                TotalSaleAmount = 0,
                TotalSalesInvoice = 0,
                RecentProducts = new List<ProductViewModel>()
            };

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                // Customers
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM TaiKhoan WHERE VaiTro = 'Khách hàng'", con))
                    model.TotalCustomers = (int)cmd.ExecuteScalar();

                // Suppliers
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM NhaCungCap", con))
                    model.TotalSuppliers = (int)cmd.ExecuteScalar();

                // Total Sale Amount
                using (var cmd = new SqlCommand("SELECT ISNULL(SUM(TongTien),0) FROM HoaDon", con))
                    model.TotalSaleAmount = Convert.ToDecimal(cmd.ExecuteScalar());

                // Sales Invoice Count
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM HoaDon", con))
                    model.TotalSalesInvoice = (int)cmd.ExecuteScalar();

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

            return View("~/Adminboot/Admin/Views/Home/Index.cshtml", model);
        }
    }
}