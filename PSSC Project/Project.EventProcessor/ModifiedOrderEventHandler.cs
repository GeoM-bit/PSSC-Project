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

            var order = new ReceivedOrder
            {
                UserRegistrationNumber = eventData.Order.UserRgistrationNumber,
                OrderNumber = eventData.Order.OrderNumber,
                DeliveryAddress = eventData.Order.DeliveryAddress,
                Telephone = eventData.Order.Telephone,
                CardNumber = eventData.Order.CardNumber,
                CVV = eventData.Order.CVV,
                CardExpiryDate = eventData.Order.CardExpiryDate,
                OrderProducts = eventData.Order.OrderProducts.Select(x => new ReceivedProduct { ProductName = x.ProductName, Quantity = x.Quantity }).ToList()
            };

            //var jsonEventData = JsonSerializer.Serialize(order);
            //var apiUrl = "https://localhost:7040/ModifyOrder/ReceiveEvent";
            //var content = new StringContent(jsonEventData, Encoding.UTF8, "application/json");
            //await _httpClient.PostAsync(apiUrl, content);

            return EventProcessingResult.Completed;
        }
    }
}
