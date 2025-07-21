using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project.Models.Shop
{
    [Table("ThuongHieu")]
    public class ThuongHieu
    {
        [Key] // ✅ THÊM dòng này

        public int MaThuongHieu { get; set; }

        public string TenThuongHieu { get; set; }
        public string Logo { get; set; }

        public ICollection<SanPham> SanPhams { get; set; }
    }
}
