using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.User
{
    [Table("DiaChiNguoiDung")]
    public class DiaChiNguoiDung
    {
        [Key]
        public int MaDiaChi { get; set; }

        public string? TenNguoiNhan { get; set; }
        public string? SoDienThoai { get; set; }

        [ForeignKey("TaiKhoan")]
        public int MaTK { get; set; }
        public TaiKhoan TaiKhoan { get; set; }

        public string? DiaChiChiTiet { get; set; } // Ví dụ: "Số 123, đường ABC"

        // Các trường này (string) sẽ được điền từ các trường ẩn (hidden input)
        public string? TinhTP { get; set; }      // Ví dụ: "Hà Nội"
        public string? QuanHuyen { get; set; }  // Ví dụ: "Quận Ba Đình"
        public string? PhuongXa { get; set; }   // Ví dụ: "Phường Cống Vị"

        public bool MacDinh { get; set; }
    }
}