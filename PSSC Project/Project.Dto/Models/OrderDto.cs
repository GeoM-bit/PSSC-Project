using System.ComponentModel.DataAnnotations;

namespace Project.Dto.Models
{
    public record OrderDto
    {
        public string UserRgistrationNumber { get; init; }

        public string OrderNumber { get; init; }

        public string DeliveryAddress { get; init; }

        public string Telephone { get; init; }

        public List<ProductDto> OrderProducts { get; init; }
    }
}
