using LanguageExt;
using Project.Domain.Models;

namespace Project.Domain.Repositories
{
    public interface IUserRepository
    {
        TryAsync<UserRegistrationNumber> TryGetExistingUser(string userToCheck);
        TryAsync<List<UserRegistrationNumber>> TryGetExistingUserRegistrationNumbers();
        TryAsync<List<UserDto>> TryGetExistingUsers();       
    }
}
