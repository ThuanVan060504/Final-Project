namespace Final_Project.ViewModels
{
    public class OrderHistoryViewModel
    {
        public int MaDonHang { get; set; }
        public DateTime NgayDat { get; set; }
        public string TenSP { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }
}
