using Microsoft.Extensions.Primitives;

namespace Final_Project.Services
{
    public class MomoExecuteResponseModel
    {
        public StringValues Amount { get; internal set; }
        public StringValues OrderId { get; internal set; }
        public StringValues OrderInfo { get; internal set; }
    }
}