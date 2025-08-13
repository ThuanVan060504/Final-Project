using Final_Project.Models.Chat;
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
        public int TotalVisits { get; set; }         // Tổng lượt truy cập
        public int TotalUserVisits { get; set; }     // Truy cập của user đã login
        public int TotalGuestVisits { get; set; }    // Truy cập của guest

        public List<ProductViewModel> RecentProducts { get; set; } = new();
        public List<TinNhan> RecentMessages { get; set; }
    }

    public class ProductViewModel
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal DonGia { get; set; }
        public string ImageURL { get; set; }
    }
}
