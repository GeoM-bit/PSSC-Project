namespace Project.Domain.Models
{
    public record EvaluatedModifiedOrder(OrderNumber OrderNumber, OrderPrice OrderPrice, OrderDeliveryAddress? OrderDeliveryAddress, OrderTelephone? OrderTelephone, OrderProducts? OrderProducts)
    {
        public UserRegistrationNumber UserRegistrationNumber { get; set; }
        public CardDetails? CardDetails { get; set; }
    }
}