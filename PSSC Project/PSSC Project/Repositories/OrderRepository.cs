using LanguageExt;
using Project.Domain.Models;
using Project.Domain.Repositories;

namespace PSSC_Project.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        public TryAsync<List<OrderCalculatedPrice>> TryGetExistentOrders()
        {
            throw new NotImplementedException();
        }
    }
}
