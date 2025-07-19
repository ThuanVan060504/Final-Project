using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // THÊM DÒNG NÀY

namespace Final_Project.Models.User
{
    [Table("DiaChiNguoiDung")] 
    public class DiaChiNguoiDung
    {
        [Key]
        public int MaDiaChi { get; set; }

        public int MaTK { get; set; }

        public string DiaChiChiTiet { get; set; }

        public string? TinhTP { get; set; }
        public string? QuanHuyen { get; set; }
        public string? PhuongXa { get; set; }

        public bool MacDinh { get; set; } = false;
        [ForeignKey("MaTK")]
        public TaiKhoan TaiKhoan { get; set; }
    }
}
