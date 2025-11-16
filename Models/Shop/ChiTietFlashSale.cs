using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("ChiTietFlashSale")]
    public class ChiTietFlashSale
    {
        [Key]
        public int MaChiTiet { get; set; }

        public int MaDot { get; set; }

        public int MaSP { get; set; }

        [Range(1, 100, ErrorMessage = "Giảm giá phải từ 1 đến 100%")]
        public int PhanTramGiam { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal GiaSauGiam { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuongGiamGia { get; set; }

        public int SoLuongDaBan { get; set; } = 0;

        // Navigation properties
        [ForeignKey("MaDot")]
        public virtual DotFlashSale DotFlashSale { get; set; }

        [ForeignKey("MaSP")]
        public virtual SanPham SanPham { get; set; } 
    }
}