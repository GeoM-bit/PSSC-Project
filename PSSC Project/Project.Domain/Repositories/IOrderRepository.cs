using LanguageExt;
using Project.Domain.Models;
using static Project.Domain.Models.Orders;

namespace Project.Domain.Repositories
{
    public interface IOrderRepository
    {
        TryAsync<List<EvaluatedOrder>> TryGetExistentOrders();
        TryAsync<OrderNumber> TryGetExistentOrder(string orderNumberToCheck);
        TryAsync<List<OrderNumber>> TryGetExistentOrderNumbers();
        TryAsync<Unit> TrySaveOrder(ValidatedOrder order);
    }
}
