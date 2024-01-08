using Microsoft.Extensions.Hosting;
using Project.Events;

namespace Project.EventProcessor
{
    internal class ModifyOrderWorker : IHostedService
    {
        private readonly IEventListener eventListener;

        public ModifyOrderWorker(IEventListener eventListener)
        {
            this.eventListener = eventListener;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Modify order worker started...");
            return eventListener.StartAsync("order", "modify-order", cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Modify order worker stoped!");
            return eventListener.StopAsync(cancellationToken);
        }
    }
}
