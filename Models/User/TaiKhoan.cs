using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.User
{
    [Table("TaiKhoan")] // ✅ Rất quan trọn
    public class TaiKhoan
    {
        

        [Key] // ✅ Phải có dòng này
        public int MaTK { get; set; }

        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }
        public string MatKhau { get; set; }
        public string VaiTro { get; set; }
        public DateTime? NgayTao { get; set; }
        public virtual ICollection<DiaChiNguoiDung> DiaChiNguoiDungs { get; set; }
        public string? Avatar { get; set; }


    }
}
