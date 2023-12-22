using CSharp.Choices;

namespace Project.Domain.Models
{
    [AsChoice]
    public static partial class Orders
    {
        public interface IOrders { }

        public record UnvalidatedOrders : IOrders
        {
            public UnvalidatedOrders(IReadOnlyCollection<UnvalidatedOrder> orderList)
            {
                OrderList = orderList;
            }

            public IReadOnlyCollection<UnvalidatedOrder> OrderList { get; }
        }

        public record InvalidOrders : IOrders
        {
            internal InvalidOrders(IReadOnlyCollection<UnvalidatedOrder> orderList, string reason)
            {
                OrderList = orderList;
                Reason = reason;
            }
            public IReadOnlyCollection<UnvalidatedOrder> OrderList { get; }
            public string Reason {  get; }
        }
        public record FailedOrders : IOrders
        {
            internal FailedOrders(IReadOnlyCollection<UnvalidatedOrder> orderList, Exception exception)
            {
                OrderList = orderList;
                Exception = exception;
            }
            public IReadOnlyCollection<UnvalidatedOrder> OrderList { get; }
            public Exception Exception { get; }
        }
        public record ValidatedOrders : IOrders
        {
            public ValidatedOrders(IReadOnlyCollection<EvaluatedOrder> orderList)
            {
                OrderList = orderList;
            }

            public IReadOnlyCollection<EvaluatedOrder> OrderList { get; }
        }

        public record PlacedOrders : IOrders
        {
            internal PlacedOrders(IReadOnlyCollection<EvaluatedOrder> orderList, DateTime placedOrderDate)
            {
                OrderList= orderList;
                Date = placedOrderDate;
            }
            public IReadOnlyCollection<EvaluatedOrder> OrderList { get; }
            public DateTime Date { get; }
        }
    }
}
