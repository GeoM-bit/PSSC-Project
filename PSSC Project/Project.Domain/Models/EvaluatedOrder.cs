using LanguageExt;

namespace Project.Domain.Models
{
    public record EvaluatedOrder(OrderNumber OrderNumber, OrderPrice OrderPrice, OrderDeliveryAddress OrderDeliveryAddress, OrderTelephone OrderTelephone, OrderProducts OrderProducts)
    {
        public UserRegistrationNumber UserRegistrationNumber { get; set; }
        public CardDetails CardDetails { get; set; }
    }
}
  
