using LanguageExt;
using LanguageExt.ClassInstances.Pred;
using Project.Domain.Exceptions;
using static LanguageExt.Prelude;

namespace Project.Domain.Models
{
    public class OrderPrice
    {
        public double Price { get; }
        public OrderPrice(double price)
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

        private static bool IsValid(double value) => value >= 0;

        public static bool TryParse(double value, out OrderPrice orderPrice)
        {
            bool isValid = false;
            orderPrice = null;

            if (IsValid(value))
            {
                isValid = true;
                orderPrice = new(value);
            }

            return isValid;
        }

        public static Option<OrderPrice> TryParseOrderPrice(float orderPrice)
        {
            if (IsValid(orderPrice))
            {
                return Some<OrderPrice>(new(orderPrice));
            }
            else
            {
                return None;
            }
        }
    }
}
