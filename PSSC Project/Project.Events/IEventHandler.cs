using CloudNative.CloudEvents;
using Project.Events.Models;

namespace Project.Events
{
    public interface IEventHandler
    {
        string[] EventTypes { get; }

        Task<EventProcessingResult> HandleAsync(CloudEvent cloudEvent);
    }
}
