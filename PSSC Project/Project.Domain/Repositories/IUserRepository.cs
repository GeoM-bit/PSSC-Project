using LanguageExt;
using Project.Domain.Models;

namespace Project.Domain.Repositories
{
    public interface IUserRepository
    {
        TryAsync<UserRegistrationNumber> TryGetExistingUserRegistrationNumber(string userToCheck);
        TryAsync<List<UserRegistrationNumber>> TryGetExistingUserRegistrationNumbers();
        TryAsync<List<UserDto>> TryGetExistingUsers();
        TryAsync<UserDto> TryGetExistingUser(string userNumberToCheck);
        TryAsync<bool> UpdateCardDetails(CardDetailsDto cardDetailsDto);
    }
}
