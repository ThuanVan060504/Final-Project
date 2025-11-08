using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("Voucher")]
    public class Voucher
    {
        [Key]
        public int MaVoucherID { get; set; }

        [Required]
        [StringLength(50)]
        public string MaCode { get; set; }

        [StringLength(500)]
        public string MoTa { get; set; }

        [Required]
        [StringLength(20)]
        public string LoaiGiamGia { get; set; } // "PhanTram" hoặc "SoTien"

        [Required]
        public decimal GiaTriGiam { get; set; }

        public decimal? GiamGiaToiDa { get; set; }

        [Required]
        public decimal DonHangToiThieu { get; set; }

        [Required]
        public DateTime NgayBatDau { get; set; }

        [Required]
        public DateTime NgayKetThuc { get; set; }

        public int? SoLuongToiDa { get; set; }

        [Required]
        public int SoLuongDaDung { get; set; }

        [Required]
        public int SoLanSuDungToiDaMoiNguoiDung { get; set; }

        [Required]
        public bool IsActive { get; set; }

        // Khóa ngoại: Một voucher có thể được lưu bởi nhiều tài khoản
        public ICollection<TaiKhoanVoucher> TaiKhoanVouchers { get; set; }

        // Khóa ngoại: Một voucher có thể được dùng cho nhiều đơn hàng
        public ICollection<DonHang> DonHangs { get; set; }
    }
}