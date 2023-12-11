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

        public TryAsync<List<EvaluatedOrder>> TryGetExistentOrders() => async () => (await (
                    from o in context.Orders
                    join p in context.OrderDetails on o.OrderId equals p.OrderId
                    join a in context.Products on p.ProductId equals a.ProductId
                    select new { o.OrderId, o.OrderNumber, o.TotalPrice, o.DeliveryAddress})
                    .AsNoTracking()
                    .ToListAsync())
                    .Select(result => new EvaluatedOrder(
                        OrderNumber: new (result.OrderNumber),
                        OrderPrice: new (result.OrderPrice),
                        OrderDeliveryAddress: new (result.OrderDeliveryAddress),
                        OrderProducts: new (result.OrderProducts),

                        )
                    {
                        OrderId = result.OrderId
                    })
                    .ToList();
    }
}
