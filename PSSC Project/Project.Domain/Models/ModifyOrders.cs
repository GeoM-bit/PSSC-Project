using CSharp.Choices;

namespace Project.Domain.Models
{
    [AsChoice]
    public static partial class ModidyOrders
    {
        public interface IModifyOrder { }

        public record UnvalidatedModifiedOrder : IModifyOrder
        {
            internal UnvalidatedModifiedOrder(UnvalidatedOrder order)
            {
                Order = order;
            }

            public UnvalidatedOrder Order { get; }
        }

        public record InvalidModifiedOrder : IModifyOrder
        {
            internal InvalidModifiedOrder(UnvalidatedOrder order, string reason)
            {
                Order = order;
                Reason = reason;
            }
            public UnvalidatedOrder Order { get; }
            public string Reason { get; }
        }
        public record FailedModifiedOrder : IModifyOrder
        {
            internal FailedModifiedOrder(UnvalidatedOrder order, Exception exception)
            {
                Order = order;
                Exception = exception;
            }
            public UnvalidatedOrder Order { get; }
            public Exception Exception { get; }
        }
        public record ValidatedModifiedOrder : IModifyOrder
        {
            public ValidatedModifiedOrder(EvaluatedModifiedOrder order)
            {
                Order = order;
            }

            public EvaluatedModifiedOrder Order { get; }
        }
    }
}
