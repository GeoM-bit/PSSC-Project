using Project.Domain.Models;

namespace Project.Domain.Commands
{
    public record PlaceOrderCommand
    {
        public PlaceOrderCommand(IReadOnlyCollection<UnvalidatedOrder> inputOrders)
        {
            InputOrders = inputOrders;
        }

        public IReadOnlyCollection<UnvalidatedOrder> InputOrders { get; }
    }
}
