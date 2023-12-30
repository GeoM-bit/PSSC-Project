using LanguageExt;
using Project.Domain.Exceptions;
using static LanguageExt.Prelude;

namespace Project.Domain.Models
{
    public record UserCardCVV
    {
        public string? Value { get; }

        public UserCardCVV(string? value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new InvalidUserCardDetailsException($"Invalid card CVV.");
            }
        }

        private static bool IsValid(string? value) => value == null || value.Length==3;

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool TryParse(string value, out UserCardCVV userCardCVV)
        {
            bool isValid = false;
            userCardCVV = null;

            if (IsValid(value))
            {
                isValid = true;
                userCardCVV = new(value);
            }

            return isValid;
        }

        public static Option<UserCardCVV> TryParse(string userCardCVV)
        {
            if (IsValid(userCardCVV))
            {
                return Some<UserCardCVV>(new(userCardCVV));
            }
            else
            {
                return None;
            }
        }
    }
}
