namespace Project.Domain.Models
{
    public record UnvalidatedOrder(string userRegistrationNumber, string OrderNumber, float OrderPrice, string OrderDeliveryAddress, List<UnvalidatedProduct> OrderProducts)
    {
    }
}
