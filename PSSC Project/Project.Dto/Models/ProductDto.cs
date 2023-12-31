using System.ComponentModel.DataAnnotations;

namespace Project.Dto.Models
{
    public record ProductDto
    {
        public string ProductName { get; set; }

        public int Quantity { get; set; }
    }
}
