using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("SanPham")]
    public class SanPham
    {
        [Key]
        public int MaSP { get; set; }

        [Required]
        public string TenSP { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Thêm TypeName cho nhất quán
        public decimal DonGia { get; set; } // Đây là Giá Bán

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? GiaGoc { get; set; } // Đây là Giá Vốn Trung Bình (đã có)

        public string? MoTa { get; set; }

        // [Required] // <- Bỏ [Required]
        public int? SoLuong { get; set; } // <<< SỬA 1: Chuyển từ int sang int? (nullable)
                                          //     Điều này để sửa lỗi CS0019 (khi dùng sp.SoLuong ?? 0)

        public string? ImageURL { get; set; }

        public int? ChieuRong { get; set; }
        public int? ChieuCao { get; set; }
        public int? ChieuSau { get; set; }

        // Khóa ngoại đến DanhMuc
        public int? MaDanhMuc { get; set; }

        [ForeignKey("MaDanhMuc")]
        public DanhMuc? DanhMuc { get; set; }

        // Khóa ngoại đến ThuongHieu
        public int? MaThuongHieu { get; set; }

        [ForeignKey("MaThuongHieu")]
        public ThuongHieu? ThuongHieu { get; set; }

        public DateTime? NgayTao { get; set; }

        // --- THÊM 2 TRƯỜNG MỚI ĐỂ SỬA LỖI CS1061 ---

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? MarkupRatio { get; set; } // <<< THÊM 1: Tỷ lệ lợi nhuận (ví dụ: 1.5)

        public bool? TuDongCapNhatGiaBan { get; set; } // <<< THÊM 2: Cờ (true/false) cho phép tự động cập nhật giá bán

        // --- HẾT PHẦN THÊM MỚI ---


        // Danh sách đánh giá (nếu có)
        public ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();

        // Không lưu trong DB
        [NotMapped]
        public string? ChiTiet { get; set; }
        [NotMapped]
        public string TenDanhMuc => DanhMuc?.TenDanhMuc;

    }
}