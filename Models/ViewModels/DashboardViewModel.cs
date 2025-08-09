using System;
using System.Collections.Generic;

namespace Final_Project.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalSuppliers { get; set; }
        public decimal TotalSaleAmount { get; set; }
        public int TotalSalesInvoice { get; set; }
        public List<ProductViewModel> RecentProducts { get; set; } = new();
    }

    public class ProductViewModel
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal DonGia { get; set; }
        public string ImageURL { get; set; }
    }
}
