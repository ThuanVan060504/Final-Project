namespace Final_Project.Models.Shop
{
    public class GioHangViewModel
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public string ImageURL { get; set; }

        public decimal ThanhTien => SoLuong * DonGia;
    }
}
