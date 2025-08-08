using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("NhaCungCap")] // Khớp tên bảng trong SQL Server
    public class NhaCungCap
    {
        [Key]
        public int MaNCC { get; set; }

        [Required, StringLength(100)]
        public string TenNCC { get; set; } = string.Empty;

        [StringLength(200)]
        public string? DiaChi { get; set; }  // nullable

        [StringLength(15)]
        public string? SoDienThoai { get; set; } // nullable
        [StringLength(100)]
        public string? Email { get; set; } // nullable

    }

}
