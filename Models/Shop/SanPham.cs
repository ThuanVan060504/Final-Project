using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("SanPham")]
    public class SanPham
    {
        [Key] // Đây là điều bắt buộc
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        [ForeignKey("MaDanhMuc")]
        public DanhMuc DanhMuc { get; set; }
        public decimal DonGia { get; set; }
        public string? MoTa { get; set; }
        [NotMapped]
        public string? ChiTiet { get; set; }
        public string? ImageURL { get; set; }
        public int? MaDanhMuc { get; set; }
        public int? ChieuRong { get; set; }
        public int? ChieuCao { get; set; }
        public int? ChieuSau { get; set; }
        public ICollection<DanhGia> DanhGias { get; set; }

    }
}
