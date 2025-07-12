using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    public class DanhGia
    {
        [Key]
        public int Id { get; set; }

        [Range(1, 5)]
        public int Diem { get; set; }

        [StringLength(500)]
        public string BinhLuan { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        // Khóa ngoại đến sản phẩm
        public int SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public SanPham SanPham { get; set; }

        // Tùy chọn: nếu có User
        public string TenNguoiDung { get; set; }
    }
}