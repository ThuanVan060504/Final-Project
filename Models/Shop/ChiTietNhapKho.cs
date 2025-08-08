using Final_Project.Models.Shop;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models
{
    [Table("ChiTietNhapKho")] // khớp với tên bảng
    public class ChiTietNhapKho
    {
        [Key]
        public int MaChiTietNhap { get; set; }

        public int MaNhapKho { get; set; }

        public int MaSP { get; set; }

        public int SoLuong { get; set; }

        public decimal DonGia { get; set; }

        public decimal? ThanhTien { get; set; }

        [ForeignKey("MaNhapKho")]
        public NhapKho? NhapKho { get; set; }

        [ForeignKey("MaSP")]
        public SanPham? SanPham { get; set; }
    }
}
