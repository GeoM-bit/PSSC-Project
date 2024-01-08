using CSharp.Choices;

namespace Project.Domain.Models
{
    [AsChoice]
    public static partial class Orders
    {
        public interface IOrder { }

        public record UnvalidatedPlacedOrder : IOrder
        {
            internal UnvalidatedPlacedOrder(UnvalidatedOrder order)
            {
                Order = order;
            }

            public UnvalidatedOrder Order { get; }
        }

        public record InvalidOrder : IOrder
        {
            internal InvalidOrder(UnvalidatedOrder order, string reason)
            {
                Order = order;
                Reason = reason;
            }
            public UnvalidatedOrder Order { get; }
            public string Reason { get; }
        }
        public record FailedOrder : IOrder
        {
            internal FailedOrder(UnvalidatedOrder order, Exception exception)
            {
                Order = order;
                Exception = exception;
            }
            public UnvalidatedOrder Order { get; }
            public Exception Exception { get; }
        }
        public record ValidatedOrder : IOrder
        {
            public ValidatedOrder(EvaluatedOrder order)
            {
                Order = order;
            }

            public EvaluatedOrder Order { get; }
        }

        public record PlacedOrder : IOrder
        {
            internal PlacedOrder(EvaluatedOrder order, DateTime placedOrderDate)
            {
                Order = order;
                Date = placedOrderDate;
            }
            public EvaluatedOrder Order { get; }
            public DateTime Date { get; }
        }
    }
}
