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
                    join u in context.Users on o.UserId equals u.UserId
                    select new { u.UserId, o.OrderId})
                    .AsNoTracking()
                    .ToListAsync())
                    .Select(result => new EvaluatedOrder(
                        )
                    {
                        OrderId = result.OrderId
                    })
                    .ToList();
    }
}
