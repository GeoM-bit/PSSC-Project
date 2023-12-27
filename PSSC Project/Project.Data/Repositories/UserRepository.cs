using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Project.Domain.Models;
using Project.Domain.Repositories;

namespace Project.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ProjectContext context;

        public UserRepository(ProjectContext context)
        {
            this.context = context;
        }
        public TryAsync<UserRegistrationNumber> TryGetExistingUser(string userToCheck) => async () =>
        {
            var user = await context.Users
                                      .FirstOrDefaultAsync(user => user.UserRegistrationNumber.Equals(userToCheck));

            return new UserRegistrationNumber(user.UserRegistrationNumber);
        };

        public TryAsync<List<UserRegistrationNumber>> TryGetExistingUserRegistrationNumbers() => async () =>
        {
            var userNumbers = await context.Users
                                      .Select(u => u.UserRegistrationNumber)
                                      .ToListAsync();

            return userNumbers.Select(number => new UserRegistrationNumber(number))
                .ToList();
        };

        public TryAsync<List<UserDto>> TryGetExistingUsers() => async () => 
        {
            var users = await context.Users.ToListAsync();

            return users.Select(u => 
                            new UserDto {
                                Balance = u.Balance, 
                                CardExpiryDate = u.CardExpiryDate, 
                                CardNumber = u.CardNumber, 
                                CVV = u.CVV,
                                FirstName = u.FirstName, 
                                LastName = u.LastName, 
                                UserRegistrationNumber = u.UserRegistrationNumber 
                            }).ToList();
        };
    }
}
