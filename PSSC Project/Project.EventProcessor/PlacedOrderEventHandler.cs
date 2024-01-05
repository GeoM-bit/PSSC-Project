using Project.Dto.Events;
using Project.Events;
using Project.Events.Models;
using System.Text.Json;
using System.Text;

namespace Project.EventProcessor
{
    internal class PlacedOrderEventHandler : AbstractEventHandler<PlacedOrderEvent>
    {
        private readonly HttpClient _httpClient = new();

        public override string[] EventTypes => new string[] { typeof(PlacedOrderEvent).Name };

        protected override async Task<EventProcessingResult> OnHandleAsync(PlacedOrderEvent eventData)
        {
            Console.WriteLine(eventData.ToString());


            var jsonEventData = JsonSerializer.Serialize(eventData.Order);
            var apiUrl = "https://localhost:7040/ModifyOrder/ReceiveEvent";
            var content = new StringContent(jsonEventData, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(apiUrl, content);

            return EventProcessingResult.Completed;
        }
    }
}
