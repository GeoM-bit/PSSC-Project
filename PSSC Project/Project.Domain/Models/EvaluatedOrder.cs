using LanguageExt;

namespace Project.Domain.Models
{
    public record EvaluatedOrder(OrderNumber OrderNumber, OrderPrice OrderPrice, OrderDeliveryAddress OrderDeliveryAddress, OrderProducts OrderProducts)
    {
        public UserRegistrationNumber UserRegistrationNumber { get; set; }
    }
}
  
