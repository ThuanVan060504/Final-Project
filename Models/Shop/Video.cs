using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("Video")] // Tên bảng trong CSDL
    public class Video
    {
        [Key]
        public int MaVideo { get; set; }

        [Required]
        [StringLength(255)]
        public string TieuDe { get; set; }

        public string? VideoUrl { get; set; } // Đường dẫn file video (ví dụ: /videos/review.mp4)
        public string? ThumbnailUrl { get; set; } // Đường dẫn ảnh bìa (ví dụ: /images/thumb.jpg)

        public DateTime? NgayTao { get; set; }

        // Khóa ngoại liên kết đến sản phẩm
        [Required]
        public int MaSP { get; set; }

        [ForeignKey("MaSP")]
        public virtual SanPham? SanPham { get; set; }
    }
}