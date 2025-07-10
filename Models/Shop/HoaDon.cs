using System;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models.Shop
{
    public class HoaDon
    {
        [Key]
        public int MaHD { get; set; }

        public int MaTK { get; set; }

        public DateTime NgayLap { get; set; } = DateTime.Now;

        public decimal TongTien { get; set; }

        public string PhuongThucThanhToan { get; set; }

        public string TrangThai { get; set; }
    }
}
