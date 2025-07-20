using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("DanhMuc")] // ✅ nên thêm để tránh lỗi tên bảng
    public class DanhMuc
    {
        [Key]
        public int MaDanhMuc { get; set; }

        [Required]
        public string TenDanhMuc { get; set; }

        public string? MoTa { get; set; }

        public string Logo { get; set; }

        // ✅ Navigation property: Danh mục này có nhiều sản phẩm
        public ICollection<SanPham> SanPhams { get; set; }
        // Không lưu trong DB – chỉ dùng cho hiển thị
        [NotMapped]
        public int TongSanPham { get; set; }
    }
}
