using Project.Dto.Models;
using System.Text;

namespace Project.Dto.Events
{
    public record ModifiedOrderEvent
    {
        public OrderDto Order { get; init; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            Console.WriteLine();
            Console.WriteLine("-----Modified order-------");

            stringBuilder.AppendLine($"User Registration Number: {Order.UserRgistrationNumber}");
            stringBuilder.AppendLine($"Order Number: {Order.OrderNumber}");
            stringBuilder.AppendLine($"Delivery Address: {Order.DeliveryAddress}");
            stringBuilder.AppendLine($"Telephone: {Order.Telephone}");
            stringBuilder.AppendLine($"Products:");
            foreach (var product in Order.OrderProducts)
            {
                stringBuilder.AppendLine($"\tProduct Name: {product.ProductName}");
                stringBuilder.AppendLine($"\tQuantity: {product.Quantity}");
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }
    }
}
