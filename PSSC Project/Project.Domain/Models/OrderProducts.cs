using Project.Domain.Exceptions;

namespace Project.Domain.Models
{
    public class OrderProducts
    {
        public List<Product> OrderProductsList { get; }
        public OrderProducts(List<Product> orderProductsList)
        {
            OrderProductsList = new List<Product>();
            foreach (var product in orderProductsList)
            {
                if (IsValid(product))
                {
                    OrderProductsList.Add(product);
                }
                else
                {
                    OrderProductsList.Clear();
                    throw new InvalidOrderDeliveryAddress("Wrong Order Product: ProductName: " + product.productName + " Quantity: " + product.quantity + " Price: " + product.price);
                }
            }
        }
        private static bool IsValid(Product product)
        {
            if(product.quantity.Quantity > 0 && product.price.Price > 0) 
                return true;
            else
                return false;
        }      
    }
}
