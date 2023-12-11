using LanguageExt;
using LanguageExt.ClassInstances.Pred;
using Project.Domain.Exceptions;
using static LanguageExt.Prelude;

namespace Project.Domain.Models
{
    public class OrderPrice
    {
        public float Price { get; }
        public OrderPrice(float price)
        {
            if (IsValid(price))
            {
                Price = price;
            }
            else
            {
                throw new InvalidOrderPrice($"{price} is an invalid order price.");
            }
        }

        private static bool IsValid(float value) => value > 0;

        public static bool TryParse(float value, out OrderPrice orderPrice)
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
