using LanguageExt;
using Project.Domain.Exceptions;
using static LanguageExt.Prelude;

namespace Project.Domain.Models
{
    public record UserCardExpiryDate
    {
        public DateTime? Value { get; }

        public UserCardExpiryDate(DateTime? value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new InvalidUserCardDetailsException($"Invalid card expiry date.");
            }
        }

        private static bool IsValid(DateTime? value) => value == null || value > DateTime.Now;

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool TryParse(DateTime value, out UserCardExpiryDate userCardExpiryDate)
        {
            bool isValid = false;
            userCardExpiryDate = null;

            if (IsValid(value))
            {
                isValid = true;
                userCardExpiryDate = new(value);
            }

            return isValid;
        }

        public static Option<UserCardExpiryDate> TryParse(DateTime userCardExpiryDate)
        {
            if (IsValid(userCardExpiryDate))
            {
                return Some<UserCardExpiryDate>(new(userCardExpiryDate));
            }
            else
            {
                return None;
            }
        }
    }
}
