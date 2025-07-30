namespace Final_Project.Models.Momo
{
    public class OrderInfoModel
    {
        public string FullName { get; set; }
        public string OrderInfo { get; set; }
        public string Amount { get; set; }
        public string OrderId { get; internal set; }
    }

}
