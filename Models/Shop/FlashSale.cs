using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("FlashSale")]
    public class FlashSale
    {
        [Key]
        public int Id { get; set; }

        public int MaSP { get; set; }

        [ForeignKey("MaSP")]
        public SanPham SanPham { get; set; }

        public DateTime ThoiGianBatDau { get; set; } = DateTime.Now;

        public DateTime ThoiGianKetThuc { get; set; } = DateTime.Now.AddHours(6);

        public int DiscountPercent { get; set; }

        [NotMapped]
        public decimal GiaKhuyenMai => SanPham?.DonGia * (1 - DiscountPercent / 100m) ?? 0;
    }
}
