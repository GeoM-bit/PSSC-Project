namespace Project.Domain.Models
{
    public class CardDetailsDto
    {
        public string? UserRegistrationNumber { get; set; }
        public string? CardNumber { get; set; }
        public string? CVV { get; set; }
        public DateTime? CardExpiryDate { get; set; }
        public Double? Balance { get; set; }
        public bool ToUpdate { get; set; } = false;
    }
}
