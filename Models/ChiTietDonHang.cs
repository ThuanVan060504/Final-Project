using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models
{
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
