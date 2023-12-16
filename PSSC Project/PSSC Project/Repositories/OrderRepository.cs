using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Project.Domain.Models;
using Project.Domain.Repositories;

namespace PSSC_Project.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ProjectContext context;

        public OrderRepository(ProjectContext context)
        {
            this.context = context;
        }

        public TryAsync<List<EvaluatedOrder>> TryGetExistentOrders() => async () => (await(
                         from order in context.Orders
                         join orderDetail in context.OrderDetails on order.OrderId equals orderDetail.OrderId
                         join product in context.Products on orderDetail.ProductId equals product.ProductId
                         group new { product, orderDetail.Quantity, order.OrderNumber, order.TotalPrice, order.DeliveryAddress } by order.OrderId into grouped
                         select new
                         {
                             OrderId = grouped.Key,
                             OrderNumber = grouped.Select(x => x.OrderNumber).FirstOrDefault(),
                             OrderPrice = grouped.Select(x => x.TotalPrice).FirstOrDefault(),
                             OrderDeliveryAddress = grouped.Select(x => x.DeliveryAddress).FirstOrDefault(),
                             Products = grouped.Select(item => 
                             new EvaluatedProduct
                             (
                                 new ProductName(item.product.ProductName),
                                 new ProductQuantity(item.Quantity),
                                 new ProductPrice(item.product.Price)
                             ))                            
                            .ToList()
                         })
                        .AsNoTracking()
                        .ToListAsync())
                        .Select(result => new EvaluatedOrder(
                             new OrderNumber(result.OrderNumber),
                             new OrderPrice(result.OrderPrice),
                             new OrderDeliveryAddress(result.OrderDeliveryAddress),
                             new OrderProducts(result.Products)
                             )
                         { 
                             OrderId=result.OrderId
                         }).ToList();
    }
}
