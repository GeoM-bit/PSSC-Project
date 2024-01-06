using CSharp.Choices;
using Project.Domain.Models;

namespace Project.Domain.WorkflowEvents
{
    [AsChoice]
    public static partial class ModifyOrderEvent
    {
        public interface IModifyOrderEvent { }

        public record ModifyOrderSucceededEvent : IModifyOrderEvent
        {
            public EvaluatedModifiedOrder Order { get; }
            public DateTime OrderModifieddDate { get; }

            internal ModifyOrderSucceededEvent(EvaluatedModifiedOrder order, DateTime orderModifiedDate)
            {
                Order = order;
                OrderModifieddDate = orderModifiedDate;
            }
        }

        public record ModifyOrderFailedEvent : IModifyOrderEvent
        {
            public string Reason { get; }
            internal ModifyOrderFailedEvent(string reason)
            {
                Reason = reason;
            }
        }
    }
}
