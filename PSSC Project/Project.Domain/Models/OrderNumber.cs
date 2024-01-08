using LanguageExt;
using static LanguageExt.Prelude;
using System.Text.RegularExpressions;
using Project.Domain.Exceptions;

namespace Project.Domain.Models
{
    public record OrderNumber
    {
        public const string Pattern = "^PSSC[0-9]{3}$";
        private static readonly Regex PatternRegex = new(Pattern);

        public string Value { get; }

        public OrderNumber(string value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new InvalidOrderNumberException($"{value} is an invalid order number.");
            }
        }

        private static bool IsValid(string stringValue) => PatternRegex.IsMatch(stringValue);

        public override string ToString()
        {
            return Value;
        }

        public static bool TryParse(string stringValue, out OrderNumber orderNumber)
        {
            bool isValid = false;
            orderNumber = null;

            if (IsValid(stringValue))
            {
                isValid = true;
                orderNumber = new(stringValue);
            }

            return isValid;
        }

        public static Option<OrderNumber> TryParse(string orderNumber)
        {
            if (IsValid(orderNumber))
            {
                return Some<OrderNumber>(new(orderNumber));
            }
            else
            {
                return None;
            }
        }
    }
}
