using Project.Domain.Models;

namespace Project.Domain.Commands
{
    public record ModifyOrderCommand
    {
        public ModifyOrderCommand(UnvalidatedOrder inputOrder)
        {
            InputOrder = inputOrder;
        }

        public UnvalidatedOrder InputOrder { get; }
    }
}
