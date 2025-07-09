using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models
{
    public class DanhMuc
    {
        [Key]
        public int MaDanhMuc { get; set; }

        [Required]
        public string TenDanhMuc { get; set; }

        public string? MoTa { get; set; }
    }
}
