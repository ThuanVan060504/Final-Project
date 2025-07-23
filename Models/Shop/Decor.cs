using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("Decor")]
    public class Decor
    {
        [Key]
        public int MaDecor { get; set; }
        public string TenDecor { get; set; }
        public string MoTa { get; set; }

        public string Link3D { get; set; }
        // 👉 Khóa ngoại
        public int MaDanhMuc { get; set; }
        [ForeignKey("MaDanhMuc")]
        public DanhMucDecor DanhMuc { get; set; }

    }
}
