using CSharp.Choices;

namespace Project.Domain.Models
{
    [AsChoice]
    public static partial class ReturnOrders
    {
        public interface IReturnOrder
        {
        }

        public record UnvalidatedReturnOrders : IReturnOrder
        {
            public UnvalidatedReturnOrders(ReturnOrderModel order)
            {
                Order = order;
            }

            public ReturnOrderModel Order { get; }
        }

        public record InvalidReturnOrders : IReturnOrder
        {
            internal InvalidReturnOrders(ReturnOrderModel order, string reason)
            {
                Order = order;
                Reason = reason;
            }
            public ReturnOrderModel Order { get; }
            public string Reason { get; }
        }
        public record FailedReturnOrders : IReturnOrder
        {
            internal FailedReturnOrders(ReturnOrderModel order, Exception exception)
            {
                Order = order;
                Exception = exception;
            }
            public ReturnOrderModel Order { get; }
            public Exception Exception { get; }
        }
        public record ValidatedReturnOrders : IReturnOrder
        {
            public ValidatedReturnOrders(EvaluatedReturnOrder order)
            {
                Order = order;
            }

            public EvaluatedReturnOrder Order { get; }
        }
    }
}
