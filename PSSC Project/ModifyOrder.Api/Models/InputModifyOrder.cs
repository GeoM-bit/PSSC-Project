using Project.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace ModifyOrder.Api.Models
{
    public class InputModifyOrder
    {
        [Required]
        [RegularExpression(UserRegistrationNumber.Pattern)]

        public string RegistrationNumber { get; set; }

        [Required]
        //[RegularExpression(OrderNumber.Pattern)]
        public string OrderNumber { get; set; } //= EventService.currentOrder.OrderNumber;

        [Required]
        public string? DeliveryAddress { get; set; }// = EventService.currentOrder.DeliveryAddress;

        [Required]
        [StringLength(10)]
        public string? Telephone { get; set; } //= EventService.currentOrder.Telephone;

        [StringLength(16)]
        public string? CardNumber { get; set; } 

        [StringLength(3)]
        public string? CVV { get; set; }

        //[FromNow]
        public DateTime? CardExpiryDate { get; set; }

      //  [Required]
       // public List<InputProduct>? OrderProducts { get; set; } 
    }
}
