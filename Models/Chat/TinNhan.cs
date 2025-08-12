using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Final_Project.Models.User;

namespace Final_Project.Models.Chat
{
    [Table("TinNhan")]
    public class TinNhan
    {
        [Key]
        public int MaTinNhan { get; set; }

        [Required]
        public int NguoiGuiId { get; set; }

        [Required]
        public int NguoiNhanId { get; set; }

        [Required]
        public string NoiDung { get; set; }

        public DateTime ThoiGianGui { get; set; } = DateTime.Now;

        public bool DaXem { get; set; } = false;

        [ForeignKey(nameof(NguoiGuiId))]
        public virtual TaiKhoan NguoiGui { get; set; }

        [ForeignKey(nameof(NguoiNhanId))]
        public virtual TaiKhoan NguoiNhan { get; set; }
    }
}
