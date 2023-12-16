using CSharp.Choices;

namespace Project.Domain.WorkflowEvents
{
    [AsChoice]
    public static partial class PlaceOrderEvent
    {
        public interface IPlaceOrderEvent { }

        public record PlaceOrderSucceededEvent : IPlaceOrderEvent
        { 
           // public IEnumerable<PlaceOrderSucceededEvent> Results { get;}
        }
    }
}
