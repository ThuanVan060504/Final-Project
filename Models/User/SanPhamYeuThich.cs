using Final_Project.Models.Shop;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Final_Project.Models.User

{
    public class SanPhamYeuThich
    {
        [Key]
        public int Id { get; set; }
        public int MaTK { get; set; }
        public int MaSP { get; set; }
        public DateTime NgayThem { get; set; }
        [ForeignKey("MaTK")]
        public TaiKhoan TaiKhoan { get; set; }
        [ForeignKey("MaSP")]
        public SanPham SanPham { get; set; }
    }
}
