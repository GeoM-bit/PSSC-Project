using LanguageExt;
using LanguageExt.ClassInstances;
using Project.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                    throw new InvalidOrderDeliveryAddress("Wrong Order Product: ProductName: " + product.name + " Quantity: " + product.quantity + " Price: " + product.price);
                }
            }
        }
        private static bool IsValid(Product product)
        {
            if(product.quantity > 0 && product.price > 0) 
                return true;
            else
                return false;
        }      
    }
}
