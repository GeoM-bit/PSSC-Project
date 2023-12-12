using Project.Domain.Exceptions;

namespace Project.Domain.Models
{
    public class OrderProducts
    {

        public List<EvaluatedProduct> OrderProductsList { get; }
        public OrderProducts(List<EvaluatedProduct> orderProductsList)
        {
            OrderProductsList = new List<EvaluatedProduct>();
            foreach (var product in orderProductsList)
            {
                if (IsValid(product))
                {
                    OrderProductsList.Add(product);
                }
                else
                {
                    OrderProductsList.Clear();
                    throw new InvalidOrderDeliveryAddress("Wrong Order Product: ProductName: " + product.ProductName + " Quantity: " + product.Quantity.Quantity + " Price: " + product.Price.Price);
                }
            }
        }

        private static bool IsValid(EvaluatedProduct product)
        {
            if(product.Quantity.Quantity > 0 && product.Price.Price > 0) 
                return true;
            else
                return false;
        }      
    }
}
