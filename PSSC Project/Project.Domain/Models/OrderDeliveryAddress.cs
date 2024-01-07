using LanguageExt;
using static LanguageExt.Prelude;
using Project.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Project.Domain.Models
{
    public class OrderDeliveryAddress
    {
        public const string RegexPattern = "^[a-zA-Z]+$";
        private static readonly Regex validPattern = new(RegexPattern);
        public string? DeliveryAddress {  get;}

        public OrderDeliveryAddress(string? deliveryAddress)
        {
            if(IsValid(deliveryAddress)) 
            {
                DeliveryAddress = deliveryAddress;
            }
            else
            {
                throw new InvalidOrderDeliveryAddress("Wrong Address " + deliveryAddress);
            }
        }

        private static bool IsValid(string? value) => value == null || validPattern.IsMatch(value);

        public override string ToString()
        {
            return $" Adress : {DeliveryAddress}";
        }

        public static bool TryParse(string deliveryAddress, out OrderDeliveryAddress orderDeliveryAddress)
        {
            bool isValid = false;
            orderDeliveryAddress = null;

            if(IsValid(deliveryAddress)) 
            {
                isValid = true;
                orderDeliveryAddress = new(deliveryAddress);
            }

            return isValid;
        }

        public static Option<OrderDeliveryAddress> TryParseOrderDeliveryAddress(string deliveryAddress) 
        {
            if(IsValid(deliveryAddress)) 
            {
                return Some<OrderDeliveryAddress>(new(deliveryAddress));
            }
            else
            {
                return None;
            }
        }
    }
}
