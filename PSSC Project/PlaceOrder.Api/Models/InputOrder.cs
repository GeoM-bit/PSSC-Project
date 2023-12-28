using Project.Domain.Models;
using System.ComponentModel.DataAnnotations;
using static PlaceOrder.Api.Validations.Validations;

namespace PlaceOrder.Api.Models
{
    public class InputOrder
    {
        [Required]
        [RegularExpression(UserRegistrationNumber.Pattern)]
        public string RegistrationNumber { get; set; }

        [Required]
        public string DeliveryAddress { get; set; }

        //[Required]
        //public string PostalCode { get; set; }

        //[Required]
        //[StringLength(10)]
        //public string Telephone { get; set; }

        //[Required]
        //[StringLength(16)]
        //public string CardNumber { get; set; }

        //[Required]
        //[StringLength(3)]
        //public int CVV { get; set; }

        //[Required]
        //[FromNow]
        //public DateTime CardExpiryDate { get; set; }

        [Required]
        public List<InputProduct> OrderProducts {  get; set; }
    }
}
