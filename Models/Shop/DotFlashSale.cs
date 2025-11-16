using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("DotFlashSale")]
    public class DotFlashSale
    {
        [Key]
        public int MaDot { get; set; }

        [Required(ErrorMessage = "Tên đợt sale là bắt buộc")]
        [StringLength(255)]
        public string TenDot { get; set; }

        [StringLength(500)]
        public string MoTa { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        public DateTime ThoiGianBatDau { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        public DateTime ThoiGianKetThuc { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<ChiTietFlashSale> ChiTietFlashSales { get; set; }

        public DotFlashSale()
        {
            ChiTietFlashSales = new HashSet<ChiTietFlashSale>();
        }
    }
}