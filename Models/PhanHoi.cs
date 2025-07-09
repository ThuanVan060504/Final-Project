using System;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models
{
    public class PhanHoi
    {
        [Key]
        public int MaPhanHoi { get; set; }

        public int MaTK { get; set; }

        public int MaSP { get; set; }

        [Range(1, 5)]
        public int DanhGia { get; set; }

        public string? BinhLuan { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.Now;
    }
}
