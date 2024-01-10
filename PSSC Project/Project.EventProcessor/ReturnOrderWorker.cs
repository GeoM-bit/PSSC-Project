using Microsoft.Extensions.Hosting;
using Project.Events;

namespace Project.EventProcessor
{
    internal class ReturnOrderWorker : IHostedService
    {
        private readonly IEventListener eventListener;

        public ReturnOrderWorker(IEventListener eventListener)
        {
            this.eventListener = eventListener;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Remove order worker started...");
            return eventListener.StartAsync("order", "remove-worker", cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Remove order worker stoped!");
            return eventListener.StopAsync(cancellationToken);
        }
    }
}
