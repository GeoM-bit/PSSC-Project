namespace Project.Domain.Models
{
    public class UserDto
    {
        public string UserRegistrationNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CardNumber { get; set; }
        public int CVV { get; set; }
        public DateTime CardExpiryDate { get; set; }
        public Double Balance { get; set; }
    }
}
