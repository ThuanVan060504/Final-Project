using Final_Project.Models.Shop; // Cần using entity
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// Namespace cho ViewModels
namespace Final_Project.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho trang Index, hiển thị danh sách các đợt sale
    /// </summary>
    public class FlashSaleEventViewModel
    {
        public int MaDot { get; set; }
        public string TenDot { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public bool IsActive { get; set; }
        public string TrangThai { get; set; } // Sắp diễn ra, Đang diễn ra, Đã kết thúc
        public int SoLuongSanPham { get; set; }
    }

    /// <summary>
    /// ViewModel chính cho trang Details (Quản lý sản phẩm trong 1 đợt sale)
    /// </summary>
    public class FlashSaleManageViewModel
    {
        // Thông tin về đợt sale
        public DotFlashSale EventDetails { get; set; }

        // Danh sách sản phẩm ĐÃ CÓ trong đợt sale này
        public List<ChiTietFlashSaleViewModel> ProductsInSale { get; set; }

        // Dùng cho form "Thêm sản phẩm mới"
        public AddProductToSaleViewModel AddProductForm { get; set; }

        // Dùng cho dropdown chọn sản phẩm
        public SelectList SanPhamList { get; set; }
    }

    public class ChiTietFlashSaleViewModel
    {
        public int MaChiTiet { get; set; }
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string ImageURL { get; set; }
        public decimal GiaGoc { get; set; }
        public int PhanTramGiam { get; set; }
        public decimal GiaSauGiam { get; set; }
        public int SoLuongGiamGia { get; set; }
        public int SoLuongDaBan { get; set; }
    }

    /// <summary>
    /// ViewModel cho form "Thêm sản phẩm"
    /// </summary>
    public class AddProductToSaleViewModel
    {
        [Required]
        public int MaDot { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn sản phẩm")]
        [Display(Name = "Sản phẩm")]
        public int MaSP { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập % giảm")]
        [Range(1, 99, ErrorMessage = "Phải từ 1-99%")]
        [Display(Name = "Phần trăm giảm (%)")]
        public int PhanTramGiam { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng bán sale")]
        public int SoLuongGiamGia { get; set; }
    }
}