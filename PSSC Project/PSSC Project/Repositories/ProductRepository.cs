using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Project.Domain.Models;
using Project.Domain.Repositories;




namespace PSSC_Project.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProjectContext context;

        public ProductRepository(ProjectContext context)
        {
            this.context = context;
        }

        public TryAsync<List<Product>> TryGetExistentProducts() => async () => (await (
                  from p in context.Products
                  select new { p.ProductName, p.Quantity, p.Price })
        .AsNoTracking()
        .ToListAsync())
        .Select(o => new Product(
            productName: new(o.ProductName),
            quantity: new(o.Quantity),
            price: new(o.Price)))
        .ToList();
    }
}
