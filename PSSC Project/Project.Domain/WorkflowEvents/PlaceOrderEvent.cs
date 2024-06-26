﻿using CSharp.Choices;
using Project.Domain.Models;

namespace Project.Domain.WorkflowEvents
{
    [AsChoice]
    public static partial class PlaceOrderEvent
    {
        public interface IPlaceOrderEvent { }

        public record PlaceOrderSucceededEvent : IPlaceOrderEvent
        { 
            public EvaluatedOrder  Order{ get;}
            public DateTime OrderPlacedDate { get; }

            internal PlaceOrderSucceededEvent(EvaluatedOrder order, DateTime orderPlacedDate)
            {
                Order = order;
                OrderPlacedDate = orderPlacedDate;
            }
        }

        public record PlaceOrderFailedEvent : IPlaceOrderEvent
        {
            public string Reason { get; }
            internal PlaceOrderFailedEvent(string reason)
            {
                Reason = reason;
            }
        }
    }
}
