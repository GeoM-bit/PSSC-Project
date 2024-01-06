using System.ComponentModel.DataAnnotations;

namespace ModifyOrder.Api.Models
{
    public class InputModifyProduct
    {
        public InputModifyProduct(string productName, int quantity)
        {
            ProductName = productName;
            Quantity = quantity;
        }

        [Required]
        public string ProductName { get; set; }

        [Required]
        [Range(1, Int32.MaxValue)]
        public int Quantity { get; set; }
    }
}
