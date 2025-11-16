using System;
using System.Collections.Generic;

// ĐÃ CẬP NHẬT NAMESPACE THEO YÊU CẦU CỦA BẠN
namespace Final_Project.Models.ViewModels
{
    /// <summary>
    /// Model chính cho toàn bộ trang /FlashSale
    /// </summary>
    public class FlashSalePageViewModel
    {
        public int MaDot { get; set; }
        public string TenDot { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public List<FlashSaleProductViewModel> SanPhams { get; set; }

        public FlashSalePageViewModel()
        {
            SanPhams = new List<FlashSaleProductViewModel>();
        }
    }

    /// <summary>
    /// Model cho từng sản phẩm hiển thị trong danh sách sale
    /// </summary>
    public class FlashSaleProductViewModel
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string ImageURL { get; set; }
        public decimal DonGiaGoc { get; set; }
        public decimal GiaSauGiam { get; set; }
        public int PhanTramGiam { get; set; }
        public int SoLuongGiamGia { get; set; }
        public int SoLuongDaBan { get; set; }

        public int PhanTramDaBan
        {
            get
            {
                if (SoLuongGiamGia <= 0) return 100;
                return (int)Math.Round((double)SoLuongDaBan / SoLuongGiamGia * 100);
            }
        }

        public string ProgressBarText
        {
            get
            {
                if (SoLuongDaBan >= SoLuongGiamGia) return "Đã hết hàng";
                if (PhanTramDaBan > 80) return "Sắp hết";
                return $"Đã bán {SoLuongDaBan}";
            }
        }
    }
}