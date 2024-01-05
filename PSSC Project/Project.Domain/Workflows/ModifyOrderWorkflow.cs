using Microsoft.Extensions.Logging;
using Project.Domain.Commands;
using Project.Domain.Repositories;
using Project.Events;
using static Project.Domain.WorkflowEvents.ModifyOrderEvent;
using static Project.Domain.WorkflowEvents.PlaceOrderEvent;

namespace Project.Domain.Workflows
{
    public class ModifyOrderWorkflow
    {
        private readonly IOrderRepository orderRepository;
        private readonly IUserRepository userRepository;
        private readonly IProductRepository productRepository;
        private readonly ILogger<ModifyOrderWorkflow> logger;
        private readonly IEventSender eventSender;

        private ModifyOrderWorkflow(IOrderRepository orderRepository, IUserRepository userRepository, IProductRepository productRepository, ILogger<ModifyOrderWorkflow> logger, IEventSender eventSender)
        {
            this.orderRepository = orderRepository;
            this.userRepository = userRepository;
            this.productRepository = productRepository;
            this.logger = logger;
            this.eventSender = eventSender;
        }

        //public async Task<IModifyOrderEvent> ExecuteAsync(ModifyOrderCommand command, [FromBody] InputOrder inputOrder)
        //{
        //}
    }
}
