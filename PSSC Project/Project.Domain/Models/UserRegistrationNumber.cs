using LanguageExt;
using static LanguageExt.Prelude;
using System.Text.RegularExpressions;
using Project.Domain.Exceptions;

namespace Project.Domain.Models
{
    public record UserRegistrationNumber
    {
        public const string Pattern = "^PSSC[0-9]{3}$";
        private static readonly Regex PatternRegex = new(Pattern);

        public string Value { get; }

        public UserRegistrationNumber(string value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new InvalidUserRegistrationNumberException($"{value} is an invalid user registration number.");
            }
        }

        private static bool IsValid(string stringValue) => PatternRegex.IsMatch(stringValue);

        public override string ToString()
        {
            return Value;
        }

        public static bool TryParse(string stringValue, out UserRegistrationNumber userRegistrationNumber)
        {
            bool isValid = false;
            userRegistrationNumber = null;

            if (IsValid(stringValue))
            {
                isValid = true;
                userRegistrationNumber = new(stringValue);
            }

            return isValid;
        }

        public static Option<UserRegistrationNumber> TryParse(string userRegistrationNumber)
        {
            if (IsValid(userRegistrationNumber))
            {
                return Some<UserRegistrationNumber>(new(userRegistrationNumber));
            }
            else
            {
                return None;
            }
        }
    }
}

