using LanguageExt;
using Project.Domain.Exceptions;
using static LanguageExt.Prelude;

namespace Project.Domain.Models
{
    public class ProductPrice
    {
        public float Price { get; }
        public ProductPrice(float price)
        {
            if (IsValid(price))
            {
                Price = price;
            }
            else
            {
                throw new InvalidPrice($"{price} is an invalid order price.");
            }
        }

        private static bool IsValid(float value) => value > 0;

        public static bool TryParse(float value, out ProductPrice ProductPrice)
        {
            bool isValid = false;
            ProductPrice = null;

            if (IsValid(value))
            {
                isValid = true;
                ProductPrice = new(value);
            }

            return isValid;
        }

        public static Option<ProductPrice> TryParseProductPrice(float ProductPrice)
        {
            if (IsValid(ProductPrice))
            {
                return Some<ProductPrice>(new(ProductPrice));
            }
            else
            {
                return None;
            }
        }
    }
}

