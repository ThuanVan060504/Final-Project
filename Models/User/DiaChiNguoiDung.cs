using Microsoft.EntityFrameworkCore;
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
        public TaiKhoan? TaiKhoan { get; set; } // Nên là nullable

        public string? DiaChiChiTiet { get; set; } // Ví dụ: "Số 123, đường ABC"

        // Các trường này (string) sẽ được điền từ các trường ẩn (hidden input)
        public string? TinhTP { get; set; }      // Ví dụ: "Hà Nội"
        public string? QuanHuyen { get; set; }  // Ví dụ: "Quận Ba Đình"
        public string? PhuongXa { get; set; }   // Ví dụ: "Phường Cống Vị"

        // === SỬA LỖI: Đổi kiểu dữ liệu từ int sang string ===
        // (Để khớp với kiểu dữ liệu trong SQL Server,
        // thường là NVARCHAR, để tránh lỗi InvalidCastException)

        [Column("MaTinhThanh")] // Ánh xạ ProvinceID (C#) -> MaTinhThanh (SQL)
        public string? ProvinceID { get; set; } // Sửa từ int sang string?

        [Column("MaQuanHuyen")] // Ánh xạ DistrictID (C#) -> MaQuanHuyen (SQL)
        public string? DistrictID { get; set; } // Sửa từ int sang string?

        [Column("MaPhuongXa")] // Ánh xạ WardCode (C#) -> MaPhuongXa (SQL)
        public string? WardCode { get; set; } // Giữ nguyên string, đổi sang nullable

        public bool MacDinh { get; set; }
    }
}
