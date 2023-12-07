namespace Project.Domain.Models
{
    public record class OrderCalculatedPrice
    {
        public int OrderId { get; set; }
        public bool IsUpdated { get; set; }
    }
}
