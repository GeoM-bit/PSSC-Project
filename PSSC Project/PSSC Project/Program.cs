using Microsoft.EntityFrameworkCore;
using PSSC_Project;
using PSSC_Project.Models;

namespace PSSC
{
    class Program
    {
        private static readonly Random random = new Random();

        private static string ConnectionString = "Server=RO-9X5DZY2\\SQLEXPRESS;Database=PSSC;Trusted_Connection=True;TrustServerCertificate=True";

        static async Task Main(string[] args)
        {
            var dbContextBuilder = new DbContextOptionsBuilder<ProjectContext>()
                                               .UseSqlServer(ConnectionString);
            ProjectContext context = new ProjectContext(dbContextBuilder.Options);

            UserDto user = new UserDto()
            {
                FirstName="Georgiana",
                LastName="Matei"
            };

            context.Add(user);
            await context.SaveChangesAsync();

            var users = await context.Users.ToListAsync();

            foreach (var use in users)
                Console.WriteLine(use.UserId.ToString()+" "+use.FirstName.ToString()+" "+use.LastName.ToString());
        }
    }
}