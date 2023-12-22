using CSharp.Choices;
using Project.Domain.Models;

namespace Project.Domain.WorkflowEvents
{
    [AsChoice]
    public static partial class PlaceOrderEvent
    {
        public interface IPlaceOrderEvent { }

        public record PlaceOrderSucceededEvent : IPlaceOrderEvent
        { 
            public IEnumerable<EvaluatedOrder>  Orders{ get;}
            public DateTime OrderPlacedDate { get; }

            internal PlaceOrderSucceededEvent(IEnumerable<EvaluatedOrder> orders, DateTime orderPlacedDate)
            {
                Orders = orders;
                OrderPlacedDate = orderPlacedDate;
            }
        }

        public record PlaceOrderFailEvent : IPlaceOrderEvent
        {
            public string Reason { get; }
            internal PlaceOrderFailEvent(string reason)
            {
                Reason = reason;
            }
        }
    }
}
