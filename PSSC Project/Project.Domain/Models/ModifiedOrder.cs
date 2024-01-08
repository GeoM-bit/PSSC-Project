namespace Project.Domain.Models
{
    public record ModifiedOrder(OrderNumber OrderNumber, OrderPrice OrderPrice, OrderDeliveryAddress OrderDeliveryAddress, OrderTelephone OrderTelephone, OrderProducts OrderProducts, CardDetails CardDetails);
}
