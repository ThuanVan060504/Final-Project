namespace Final_Project.Models.Vnpay
{
    public class PaymentInformationModel
    {
        public string OrderType { get; set; } = string.Empty; // Fixed by initializing with a default value
        public int Amount { get; set; } // Đã fix
        public string OrderDescription { get; set; } = string.Empty; // Fixed by initializing with a default value
        public string Name { get; set; } = string.Empty; // Fixed by initializing with a default value
    }
}
