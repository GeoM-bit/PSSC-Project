using Project.Common.Services;
using Project.Dto.Events;
using Project.Events;
using Project.Events.Models;
using System.Text;
using System.Text.Json;

namespace Project.EventProcessor
{
    internal class ModifiedOrderEventHandler: AbstractEventHandler<ModifiedOrderEvent>
    {
        private readonly HttpClient _httpClient = new();

        public override string[] EventTypes => new string[] { typeof(ModifiedOrderEvent).Name };

        protected override async Task<EventProcessingResult> OnHandleAsync(ModifiedOrderEvent eventData)
        {
            Console.WriteLine(eventData.ToString());

            var orderToRetun = new ReturnOrderData
            {
                UserRegistrationNumber = eventData.Order.UserRgistrationNumber,
                OrderNumber = eventData.Order.OrderNumber
            };

            var jsonReturnEventData = JsonSerializer.Serialize(orderToRetun);
            var returnApiUrl = "https://localhost:7160/ReturnOrder/ReceiverReturnEvent";
            var returnOrderContent = new StringContent(jsonReturnEventData, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(returnApiUrl, returnOrderContent);

            return EventProcessingResult.Completed;
        }
    }
}
