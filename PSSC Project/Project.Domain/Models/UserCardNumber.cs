using LanguageExt;
using LanguageExt.Pipes;
using Project.Domain.Exceptions;
using System.Text.RegularExpressions;
using static LanguageExt.Prelude;

namespace Project.Domain.Models
{
    public record UserCardNumber
    {
        public string? CardNumber { get; }

        public UserCardNumber(string? cardNumber)
        {
            if (IsValid(cardNumber))
            {
                CardNumber = cardNumber;
            }
            else
            {
                throw new InvalidUserCardDetailsException($"Invalid user card number.");
            }
        }

        private static bool IsValid(string? CardNumber) => CardNumber == null || (new Regex("[0-9]{16}")).IsMatch(CardNumber);

        public override string ToString()
        {
            return CardNumber;
        }

        public static bool TryParse(string stringValue, out UserCardNumber userCardNumber)
        {
            bool isValid = false;
            userCardNumber = null;

            if (IsValid(stringValue))
            {
                isValid = true;
                userCardNumber = new(stringValue);
            }

            return isValid;
        }

        public static Option<UserCardNumber> TryParse(string userCardNumber)
        {
            if (IsValid(userCardNumber))
            {
                return Some<UserCardNumber>(new(userCardNumber));
            }
            else
            {
                return None;
            }
        }
    }
}
