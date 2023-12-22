using Project.Domain.Repositories;
using Microsoft.Extensions.Logging;

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
      
    }
}
