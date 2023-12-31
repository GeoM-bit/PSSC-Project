using LanguageExt;
using Project.Domain.Exceptions;
using System.Text.RegularExpressions;
using static LanguageExt.Prelude;

namespace Project.Domain.Models
{
    public record OrderTelephone
    {
        public const string Pattern = "^0[0-9]{9}";
        private static readonly Regex PatternRegex = new(Pattern);

        public string Value { get; }

        public OrderTelephone(string value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new InvalidOrderTelephoneException($"{value} is an invalid telephone number.");
            }
        }

        private static bool IsValid(string stringValue) => PatternRegex.IsMatch(stringValue);

        public override string ToString()
        {
            return Value;
        }

        public static bool TryParse(string stringValue, out OrderTelephone orderTelephone)
        {
            bool isValid = false;
            orderTelephone = null;

            if (IsValid(stringValue))
            {
                isValid = true;
                orderTelephone = new(stringValue);
            }

            return isValid;
        }

        public static Option<OrderTelephone> TryParse(string orderTelephone)
        {
            if (IsValid(orderTelephone))
            {
                return Some<OrderTelephone>(new(orderTelephone));
            }
            else
            {
                return None;
            }
        }
    }
}
