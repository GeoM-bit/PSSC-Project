using LanguageExt;

namespace Project.Domain.Models
{
    public record EvaluatedOrder(OrderNumber OrderNumber = default, OrderPrice OrderPrice = default, OrderDeliveryAddress OrderDeliveryAddress = default, OrderProducts OrderProducts = default)
    {

        public int OrderId { get; set; }
    }
}
  
