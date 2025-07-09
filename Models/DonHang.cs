using System;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models
{
    public class DonHang
    {
        [Key]
        public int MaDonHang { get; set; }

        public int MaTK { get; set; }

        public int MaDiaChi { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;
        public DateTime? NgayYeuCau { get; set; }
        public DateTime? NgayGiao { get; set; }

        public decimal PhiVanChuyen { get; set; }

        public decimal TongTien { get; set; }

        public decimal GiamGia { get; set; }

        public string PhuongThucThanhToan { get; set; }

        public string TrangThaiThanhToan { get; set; }

        public string TrangThaiDonHang { get; set; }

        public string? GhiChu { get; set; }
    }
}
