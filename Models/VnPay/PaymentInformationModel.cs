namespace Final_Project.Models.VnPay
{
    public class PaymentInformationModel
    {
        public string OrderType { get; set; }
        public decimal Amount { get; set; }
        public string OrderDescription { get; set; }
        public string Name { get; set; }
        public string OrderId { get; set; }
        public string PayUrl { get; set; }

    }

}
