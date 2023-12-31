using Project.Domain.Models;

namespace Project.Domain.Commands
{
    public record PlaceOrderCommand
    {
        public PlaceOrderCommand(UnvalidatedOrder inputOrder)
        {
            InputOrder = inputOrder;
        }

        public UnvalidatedOrder InputOrder { get; }
    }
}
