using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("DanhMucDecor")]
    public class DanhMucDecor
    {
        [Key]
        public int MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }
        public string? Logo { get; set; }

        public ICollection<Decor> Decors { get; set; }
    }
}
