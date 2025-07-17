using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // cần để dùng [Table]

namespace Final_Project.Models.Shop
{
    [Table("ChiTietDonHang")] // 👈 ánh xạ chính xác với bảng trong DB
    public class ChiTietDonHang
    {
        [Key]
        public int MaChiTiet { get; set; }

        public int MaDonHang { get; set; }

        public int MaSP { get; set; }

        public int SoLuong { get; set; }

        public decimal DonGia { get; set; }
    }
}
