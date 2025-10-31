// File: Models/Shop/NhapKho.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("NhapKho")] 
    public class NhapKho
    {
        [Key]
        public int MaNhapKho { get; set; }
        public int? MaNCC { get; set; }
        public DateTime? NgayNhap { get; set; }

        [Column(TypeName = "decimal(18, 2)")] // Đảm bảo là decimal
        public decimal? TongTien { get; set; }

        public string? GhiChu { get; set; }

        [Column(TypeName = "decimal(18, 2)")] // Đảm bảo là decimal
        public decimal? ConNo { get; set; }

        [Column(TypeName = "decimal(18, 2)")] // Đảm bảo là decimal
        public decimal? DaThanhToan { get; set; }

        [Column(TypeName = "decimal(18, 2)")] // Đảm bảo là decimal
        public decimal? ChietKhau { get; set; } // Sửa đây từ float/double sang decimal

        public string? HinhThucThanhToan { get; set; }
        public int? MaTK { get; set; }

        // --- THÊM CÁC THUỘC TÍNH MỚI VÀO ĐÂY ---

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ChiPhiPhatSinh { get; set; } // Lỗi CS0117

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TongHoaDonThucTe { get; set; } // Lỗi CS0117

        // --- (Giữ lại các quan hệ nếu có) ---
        // public virtual TaiKhoan? TaiKhoan { get; set; }
        // public virtual NhaCungCap? NhaCungCap { get; set; }
        // public virtual ICollection<ChiTietNhapKho> ChiTietNhapKhos { get; set; }
    }
}