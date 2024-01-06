using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Project.Domain.Models;
using Project.Domain.Repositories;

namespace Project.Data.Repositories
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

        public TryAsync<List<EvaluatedProduct>> TryGetOrderProducts(string orderNumber) => async () => (await (
                 from order in context.Orders where order.OrderNumber == orderNumber
                 join orderDetail in context.OrderDetails on order.OrderId equals orderDetail.OrderId
                 join product in context.Products on orderDetail.ProductId equals product.ProductId
                 group new { product, orderDetail.Quantity,} by order.OrderId into grouped
                 select new
                 {   
                     ProductName = grouped.Select(x => x.product.ProductName).FirstOrDefault(),
                     Quantity = grouped.Select(x=> x.Quantity).FirstOrDefault(),
                     Price = grouped.Select(x => x.product.Price).FirstOrDefault()
                 })
                 .AsNoTracking()
                 .ToListAsync())
                 .Select(item => new EvaluatedProduct
                 (
                    new ProductName(item.ProductName),
                    new ProductQuantity(item.Quantity),
                    new ProductPrice(item.Price)
                 )
                 )
                 .ToList(); 
    }
}
