namespace Project.Common.Services
{
    public class ReceivedOrder
    {
        public string UserRegistrationNumber { get; set; }

        public string OrderNumber { get; set; }

        public string DeliveryAddress { get; set; }

        public string Telephone { get; set; }

        public string? CardNumber { get; set; }

        public string? CVV { get; set; }

        public DateTime? CardExpiryDate { get; set; }

        public List<ReceivedProduct>? OrderProducts { get; set; }
    }
}
