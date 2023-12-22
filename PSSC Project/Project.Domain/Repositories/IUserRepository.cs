using LanguageExt;

namespace Project.Domain.Repositories
{
    public interface IUserRepository
    {
        TryAsync<List<StudentRegistrationNumber>> TryGetExistingStudents(IEnumerable<string> studentsToCheck);

    }
}
