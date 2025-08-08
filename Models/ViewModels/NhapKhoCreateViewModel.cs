namespace Final_Project.Models.ViewModels
{
    public class NhapKhoCreateViewModel
    {
        public int MaNCC { get; set; }
        public DateTime NgayNhap { get; set; }
        public string GhiChu { get; set; }
        public string HinhThucThanhToan { get; set; }
        public decimal TongTien { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo { get; set; }
        public List<ChiTietNhapKhoViewModel> ChiTietNhapKho { get; set; }
    }

    public class ChiTietNhapKhoViewModel
    {
        public int MaSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }

}
