using Project.Domain.Repositories;
using Microsoft.Extensions.Logging;
using static Project.Domain.WorkflowEvents.PlaceOrderEvent;
using Project.Domain.Commands;
using Project.Domain.Models;
using static Project.Domain.Models.Orders;

namespace Project.Domain.Workflows
{
    public class PlaceOrderWorkflow
    {
        private readonly IOrderRepository orderRepository;
        private readonly ILogger<PlaceOrderWorkflow> logger;

        public PlaceOrderWorkflow(IOrderRepository orderRepository, ILogger<PlaceOrderWorkflow> logger)
        {
            this.orderRepository = orderRepository;
            this.logger = logger;
        }
      
        public async Task<IPlaceOrderEvent> ExecuteAsync(PlaceOrderCommand command)
        {
            UnvalidatedOrders unvalidatedOrders = new UnvalidatedOrders(command.InputOrders);
            
            return null;
        }
    }
}
