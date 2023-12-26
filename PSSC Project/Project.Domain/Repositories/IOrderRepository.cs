using LanguageExt;
using Project.Domain.Models;

namespace Project.Domain.Repositories
{
    public interface IOrderRepository
    {
        TryAsync<List<EvaluatedOrder>> TryGetExistentOrders();
        TryAsync<OrderNumber> TryGetExistentOrder(string orderNumberToCheck);
        TryAsync<List<OrderNumber>> TryGetExistentOrderNumbers();
    }
}
