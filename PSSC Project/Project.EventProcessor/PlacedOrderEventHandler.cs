using Project.Dto.Events;
using Project.Events;
using Project.Events.Models;
using System.Text.Json;
using System.Text;
using Project.Common.Services;

namespace Project.EventProcessor
{
    internal class PlacedOrderEventHandler : AbstractEventHandler<PlacedOrderEvent>
    {
        private readonly HttpClient _httpClient = new();

        public override string[] EventTypes => new string[] { typeof(PlacedOrderEvent).Name };

        protected override async Task<EventProcessingResult> OnHandleAsync(PlacedOrderEvent eventData)
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
                OrderProducts = eventData.Order.OrderProducts.Select(x=> new ReceivedProduct { ProductName=x.ProductName, Quantity = x.Quantity}).ToList()
            };

            var jsonEventData = JsonSerializer.Serialize(order);
            var apiUrl = "https://localhost:7040/ModifyOrder/ReceiveEvent";
            var content = new StringContent(jsonEventData, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(apiUrl, content);

            return EventProcessingResult.Completed;
        }
    }
}
