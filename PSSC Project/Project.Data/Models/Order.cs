namespace Project.Data.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string OrderNumber { get; set; }
        public double TotalPrice { get; set; }
        public string DeliveryAddress { get; set; }
        public string Telephone { get; set; }
    }
}
