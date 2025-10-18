using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Final_Project.Models.PayPal;

namespace Final_Project.Services.PayPal
{
    public interface IPayPalService
    {
        Task<string> CreatePaymentUrlAsync(PayPalPaymentModel model, HttpContext context);
        Task<PayPalPaymentResponse> ExecutePaymentAsync(IQueryCollection query);
    }
}
