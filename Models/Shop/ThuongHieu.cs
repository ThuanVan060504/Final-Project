using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Final_Project.Models.Shop
{
    [Table("ThuongHieu")]
    public class ThuongHieu
    {
        [Key]
        public int MaThuongHieu { get; set; }

        public string? TenThuongHieu { get; set; }  // Nullable string

        public string? Logo { get; set; }  // Nullable string

        public ICollection<SanPham>? SanPhams { get; set; } = new List<SanPham>();  // Nullable collection, khởi tạo mặc định
    }
}
