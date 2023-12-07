namespace PSSC_Project.Models
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public float TotalPrice { get; set; }
        public string DeliveryAddress { get; set; }
        public string PostalCOode { get; set; }
        public string Telephone { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
