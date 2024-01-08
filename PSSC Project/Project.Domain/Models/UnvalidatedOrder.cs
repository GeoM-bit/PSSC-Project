namespace Project.Domain.Models
{
    public record UnvalidatedOrder(string UserRegistrationNumber, string OrderNumber, float? OrderPrice, string? OrderDeliveryAddress, string? OrderTelephone, string? CardNumber, string? CVV, DateTime? CardExpiryDate, List<UnvalidatedProduct>? OrderProducts);
}

