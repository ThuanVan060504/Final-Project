using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("GioHang")]
    public class GioHang
    {
        [Key]
        public int MaGH { get; set; }

        public int MaTK { get; set; }

        public int MaSP { get; set; }

        public int SoLuong { get; set; }

        public DateTime NgayThem { get; set; }
    }
}
