namespace Final_Project.Models.ViewModels
{
    public class DonHangViewModel
    {
        public int MaDonHang { get; set; }
        public DateTime NgayDat { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public string TrangThaiDonHang { get; set; }

        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string ImageURL { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuongDat { get; set; }
        public decimal DonGiaBan { get; set; }

        public decimal TongTien => SoLuongDat * DonGiaBan;
    }

}
