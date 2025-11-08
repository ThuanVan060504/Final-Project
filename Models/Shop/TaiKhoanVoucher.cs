using Final_Project.Models.User;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("TaiKhoanVoucher")]
    public class TaiKhoanVoucher
    {
        [Key]
        public int MaTaiKhoanVoucher { get; set; }

        [Required]
        public int MaTK { get; set; }

        [Required]
        public int MaVoucherID { get; set; }

        [Required]
        public DateTime NgayLuu { get; set; }

        // Khóa ngoại
        [ForeignKey("MaTK")]
        public TaiKhoan TaiKhoan { get; set; }

        [ForeignKey("MaVoucherID")]
        public Voucher Voucher { get; set; }
    }
}