using System.ComponentModel.DataAnnotations;

namespace Project.ReturnOrder.Models
{
    public class ReturnOrderInput
    {
        [Required]
        [RegularExpression("^ORD[0-9]{6}$")]
        public string InputOrderNumber { get; set; }

        [Required]
        [RegularExpression("^USER[0-9]{7}$")]
        public string UserRegistrationNumber { get; set; }
    }
}
