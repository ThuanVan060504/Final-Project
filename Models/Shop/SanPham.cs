using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("SanPham")]
    public class SanPham
    {
        [Key]
        public int MaSP { get; set; }

        [Required]
        public string TenSP { get; set; }

        [Required]
        public decimal DonGia { get; set; }

        public decimal? GiaGoc { get; set; } 

        public string? MoTa { get; set; }

        [NotMapped]
        public string? ChiTiet { get; set; }

        [Required]
        public int SoLuong { get; set; }

        public string? ImageURL { get; set; }

        public int? ChieuRong { get; set; }
        public int? ChieuCao { get; set; }
        public int? ChieuSau { get; set; }

        // ⚙️ Danh mục sản phẩm
        public int? MaDanhMuc { get; set; }

        [ForeignKey("MaDanhMuc")]
        public DanhMuc? DanhMuc { get; set; }

        // ⚙️ Thương hiệu sản phẩm
        public int? MaThuongHieu { get; set; }

        [ForeignKey("MaThuongHieu")]
        public ThuongHieu? ThuongHieu { get; set; }

        // 🔗 Danh sách đánh giá
        public ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    }
}
