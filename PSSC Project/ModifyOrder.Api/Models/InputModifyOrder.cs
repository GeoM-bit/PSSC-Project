using Project.Domain.Models;
using System.ComponentModel.DataAnnotations;
using static Project.Common.Validations.Validations;

namespace ModifyOrder.Api.Models
{
    public class InputModifyOrder
    {
        [Required]
        [RegularExpression(UserRegistrationNumber.Pattern)]
        public string ModifyOrderRegistrationNumber { get; set; }

        [Required]
        [RegularExpression(OrderNumber.Pattern)]
        public string ModifyOrderNumber { get; set; } 

        public string? DeliveryAddress { get; set; }

        [StringLength(10)]
        public string? Telephone { get; set; } 

        [StringLength(16)]
        public string? CardNumber { get; set; } 

        [StringLength(3)]
        public string? CVV { get; set; }

        [FromNow]
        public DateTime? CardExpiryDate { get; set; }

        [Required]
        public List<InputModifyProduct>? OrderProducts { get; set; } 
    }
}
