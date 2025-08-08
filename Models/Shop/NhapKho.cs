using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("NhapKho")] // khớp với tên bảng SQL
    public class NhapKho
    {
        [Key]
        public int MaNhapKho { get; set; }

        public DateTime NgayNhap { get; set; }

        public decimal TongTien { get; set; }

        [Required]
        public int MaNCC { get; set; }  // <-- Thêm đúng tên cột

        [ForeignKey("MaNCC")]
        public NhaCungCap NhaCungCap { get; set; }
        public string? GhiChu { get; set; }

        public decimal ConNo { get; set; }

        public decimal DaThanhToan { get; set; }

        public float ChietKhau { get; set; }

        public string? HinhThucThanhToan { get; set; }

        public int MaTK { get; set; }

        public NhaCungCap? NhaCungCaps { get; set; }
        public List<ChiTietNhapKho>? ChiTietNhapKhos { get; set; }
    }
}
