using Project.Common.Services;
using Swashbuckle.AspNetCore.Filters;

namespace ModifyOrder.Api.Models
{
    public class InputModifyOrderExample: IExamplesProvider<InputModifyOrder>
    {
        public InputModifyOrder GetExamples()
        {
            return new InputModifyOrder
            {
                ModifyOrderRegistrationNumber = EventService.currentOrder==null? "empty":EventService.currentOrder.UserRegistrationNumber,
                ModifyOrderNumber = EventService.currentOrder == null ? "empty" : EventService.currentOrder.OrderNumber,
                DeliveryAddress = EventService.currentOrder == null ? "empty" : EventService.currentOrder.DeliveryAddress,
                Telephone = EventService.currentOrder == null ? "empty" : EventService.currentOrder.Telephone,
                CardNumber = EventService.currentOrder == null || EventService.currentOrder.CardNumber == null ? "empty" : EventService.currentOrder.CardNumber,
                CVV = EventService.currentOrder == null || EventService.currentOrder.CVV == null ? "empty" : EventService.currentOrder.CVV,
                CardExpiryDate = EventService.currentOrder == null || EventService.currentOrder.CardExpiryDate == null ? DateTime.MinValue : EventService.currentOrder.CardExpiryDate,
                OrderProducts = EventService.currentOrder == null ? new List<InputModifyProduct>() : ReceivedProductListToInputModifyProductList(EventService.currentOrder.OrderProducts)
            };        
        }

        private List<InputModifyProduct> ReceivedProductListToInputModifyProductList(List<ReceivedProduct> received)
        {
            return received.Select(x=> new InputModifyProduct
            (
                x.ProductName,
                x.Quantity
            )).ToList();
        }
    }
}
