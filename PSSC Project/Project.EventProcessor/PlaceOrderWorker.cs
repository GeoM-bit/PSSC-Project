using Microsoft.Extensions.Hosting;
using Project.Events;

namespace Project.EventProcessor
{
    internal class PlaceOrderWorker : IHostedService
    {
        private readonly IEventListener eventListener;

        public PlaceOrderWorker(IEventListener eventListener)
        {
            this.eventListener = eventListener;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Place order worker started...");
            return eventListener.StartAsync("order", "order-operation", cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Place order worker stoped!");
            return eventListener.StopAsync(cancellationToken);
        }
    }
}
