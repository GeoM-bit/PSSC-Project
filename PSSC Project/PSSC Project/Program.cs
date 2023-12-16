using Microsoft.EntityFrameworkCore;
using PSSC_Project;
using PSSC_Project.Repositories;

namespace PSSC
{
    class Program
    {
        private static string ConnectionString = "Server=RO-9X5DZY2\\SQLEXPRESS;Database=PSSC;Trusted_Connection=True;TrustServerCertificate=True";

        static async Task Main(string[] args)
        {
            var dbContextBuilder = new DbContextOptionsBuilder<ProjectContext>()
                                               .UseSqlServer(ConnectionString);
            ProjectContext context = new ProjectContext(dbContextBuilder.Options);

            OrderRepository repository = new OrderRepository(context);

            ProductRepository productRepository = new ProductRepository(context);

           var orders = await repository.TryGetExistentOrders().ToList();
           
            var products = await productRepository.TryGetExistentProducts().ToList();
        }
    }
}