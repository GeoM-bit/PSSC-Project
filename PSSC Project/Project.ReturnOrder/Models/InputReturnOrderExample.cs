using Project.Common.Services;
using Swashbuckle.AspNetCore.Filters;

namespace ReturnOrder.Api.Models
{
    public class InputReturnOrderExample: IExamplesProvider<ReturnOrderData>
    {
        public ReturnOrderData GetExamples()
        {
            return new ReturnOrderData
            {
                UserRegistrationNumber = EventService.currentOrderToRemove == null ? "empty" : EventService.currentOrderToRemove.UserRegistrationNumber,
                OrderNumber = EventService.currentOrderToRemove == null ? "empty" : EventService.currentOrderToRemove.OrderNumber,            
            };
        }
    }
}
