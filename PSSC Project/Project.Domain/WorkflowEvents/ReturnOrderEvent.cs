using CSharp.Choices;
using Project.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.WorkflowEvents
{
    [AsChoice]
    public static partial class ReturnOrderEvent
    {
        public interface IReturnOrderEvent { }

        public record ReturnOrderSucceededEvent : IReturnOrderEvent
        {
            public EvaluatedReturnOrder Order { get; }
            public DateTime OrderReturnDate { get; }

            internal ReturnOrderSucceededEvent(EvaluatedReturnOrder order, DateTime orderReturnDate)
            {
                Order = order;
                OrderReturnDate = orderReturnDate;
            }
        }

        public record ReturnOrderFailedEvent : IReturnOrderEvent
        {
            public string Reason { get; }
            internal ReturnOrderFailedEvent(string reason)
            {
                Reason = reason;
            }
        }
    }
}
