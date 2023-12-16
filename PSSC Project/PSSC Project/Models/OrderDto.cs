namespace PSSC_Project.Models
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string? OrderNumber { get; set; }
        public double TotalPrice { get; set; }
        public string DeliveryAddress { get; set; }
        public string PostalCode { get; set; }
        public string Telephone { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
