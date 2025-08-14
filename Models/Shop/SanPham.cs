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
        public string TenSP { get; set; } = null!; // Thêm = null! để tránh cảnh báo null

        [Required]
        public decimal DonGia { get; set; }

        public decimal? GiaGoc { get; set; }

        public string? MoTa { get; set; }

        [Required]
        public int SoLuong { get; set; }

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

        // Ngày tạo - nullable để tránh lỗi Null
        public DateTime? NgayTao { get; set; }

        // Danh sách đánh giá (nếu có)
        public ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();

        // Không lưu trong DB
        [NotMapped]
        public string? ChiTiet { get; set; }
        [NotMapped]
        public string TenDanhMuc => DanhMuc?.TenDanhMuc;

    }
}
