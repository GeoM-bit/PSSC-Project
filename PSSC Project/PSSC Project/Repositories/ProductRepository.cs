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

        public TryAsync<List<EvaluatedProduct>> TryGetExistentProducts() => async () => (await (
                  from p in context.Products
                  select new { p.ProductName, p.Quantity, p.Price })
        .AsNoTracking()
        .ToListAsync())
        .Select(o => new EvaluatedProduct(
            new ProductName(o.ProductName),
            new ProductQuantity(o.Quantity),
            new ProductPrice(o.Price)))
        .ToList();
    }
}
