using LanguageExt;
using Project.Domain.Models;
using static Project.Domain.Models.ModidyOrders;
using static Project.Domain.Models.Orders;

namespace Project.Domain.Repositories
{
    public interface IOrderRepository
    {
        TryAsync<List<EvaluatedOrder>> TryGetExistentOrders();
        TryAsync<OrderNumber> TryGetExistentOrderNumber(string orderNumberToCheck);
        TryAsync<EvaluatedOrder> TryGetExistentOrder(string orderNumberToCheck);
        TryAsync<List<OrderNumber>> TryGetExistentOrderNumbers();
        TryAsync<Unit> TrySaveOrder(ValidatedOrder order);
        TryAsync<Unit> TryUpdateOrder(ValidatedModifiedOrder order);
    }
}
