using LanguageExt;
using Project.Domain.Exceptions;
using static LanguageExt.Prelude;

namespace Project.Domain.Models
{
    public class ProductQuantity
    {
        public int Quantity { get; }
        public ProductQuantity(int quantity)
        {
            if (IsValid(quantity))
            {
                Quantity = quantity;
            }
            else
            {
                throw new InvalidPrice($"{quantity} is an invalid order quantity.");
            }
        }

        private static bool IsValid(int value) => value > 0;

        public static bool TryParse(int value, out ProductQuantity productQuantity)
        {
            bool isValid = false;
            productQuantity = null;

            if (IsValid(value))
            {
                isValid = true;
                productQuantity = new(value);
            }

            return isValid;
        }

        public static Option<ProductQuantity> TryParseOrderPrice(int productQuantity)
        {
            if (IsValid(productQuantity))
            {
                return Some<ProductQuantity>(new(productQuantity));
            }
            else
            {
                return None;
            }
        }
    }
}
