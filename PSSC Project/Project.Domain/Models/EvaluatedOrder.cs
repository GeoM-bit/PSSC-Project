namespace Project.Domain.Models
{
    public record class EvaluatedOrder(OrderNumber OrderNumber, OrderPrice Price, OrderDeliveryAddress orderDeliveryAddress, OrderProducts orderProducts)
    {
        public int OrderId { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
