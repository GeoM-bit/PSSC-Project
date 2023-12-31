using LanguageExt;
using Project.Domain.Exceptions;
using static LanguageExt.Prelude;

namespace Project.Domain.Models
{
    public record UserCardBalance
    {
        public double? Value { get; }

        public UserCardBalance(double? value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new InvalidUserCardDetailsException($"Invalid card balance.");
            }
        }

        private static bool IsValid(double? value) => value==null || value>0;

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool TryParse(double value, out UserCardBalance UserCardBalance)
        {
            bool isValid = false;
            UserCardBalance = null;

            if (IsValid(value))
            {
                isValid = true;
                UserCardBalance = new(value);
            }

            return isValid;
        }

        public static Option<UserCardBalance> TryParse(double UserCardBalance)
        {
            if (IsValid(UserCardBalance))
            {
                return Some<UserCardBalance>(new(UserCardBalance));
            }
            else
            {
                return None;
            }
        }
    }
}
