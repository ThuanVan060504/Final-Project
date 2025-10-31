// File: Models/Shop/ChiTietNhapKho.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("ChiTietNhapKho")]
    public class ChiTietNhapKho
    {
        [Key]
        [Column("MaChiTietNhap")]
        public int MaChiTietNhapKho { get; set; }
        public int MaNhapKho { get; set; }
        public int MaSP { get; set; }
        public int? SoLuong { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? DonGia { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ThanhTien { get; set; }

        // --- THÊM THUỘC TÍNH MỚI VÀO ĐÂY ---

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? GiaVonDonVi { get; set; } // Lỗi CS0117

        // --- (Giữ lại các quan hệ nếu có) ---
        // public virtual NhapKho? NhapKho { get; set; }
        // public virtual SanPham? SanPham { get; set; }
    }
}