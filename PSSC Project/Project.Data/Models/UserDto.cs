﻿namespace Project.Data.Models
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? CardNumber { get; set; }
        public int? CVV { get; set; }
        public DateTime? CardExpiryDate { get; set; }
        public Double? Balance { get; set; }
    }
}