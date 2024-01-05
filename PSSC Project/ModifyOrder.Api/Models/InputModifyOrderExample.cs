using Project.Services;
using Swashbuckle.AspNetCore.Filters;

namespace ModifyOrder.Api.Models
{
    public class InputModifyOrderExample: IExamplesProvider<InputModifyOrder>
    {
        public InputModifyOrder GetExamples()
        {
            return new InputModifyOrder
            {
                RegistrationNumber = EventService.currentOrder==null? "PSSC123":EventService.currentOrder.UserRgistrationNumber,
                OrderNumber = EventService.currentOrder == null ? "aha" : EventService.currentOrder.OrderNumber
            };        
        }
    }
}
