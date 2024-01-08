using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using Project.Dto.Events;

namespace Project.AzureMessageHandler
{
    public class OrderPlacedHandler
    {
        private readonly ILogger<OrderPlacedHandler> _logger;

        public OrderPlacedHandler(ILogger<OrderPlacedHandler> log)
        {
            _logger = log;
        }

        [FunctionName("OrderPlacedHandler")]
        public void Run([ServiceBusTrigger("order", "order-operation")] ServiceBusReceivedMessage mySbMsg)
        {
            _logger.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg}");

            var orderEvent = mySbMsg.Body.ToObjectFromJson<Azure.Messaging.CloudEvent>();
            
            var order = orderEvent.Data.ToObjectFromJson<PlacedOrderEvent>();

            _logger.LogInformation($"Received message:");
            _logger.LogInformation(order.ToString());
        }
    }
}
