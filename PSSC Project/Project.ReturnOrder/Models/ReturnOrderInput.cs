using System.ComponentModel.DataAnnotations;

namespace Project.ReturnOrder.Models
{
    public class ReturnOrderInput
    {
        [Required]
        [RegularExpression("^PSSC[0-9]{3}$")]
        public string OrderNumber { get; set; }

        [Required]
        [RegularExpression("^PSSC[0-9]{3}$")]
        public string UserRegistrationNumber { get; set; }
    }
}
