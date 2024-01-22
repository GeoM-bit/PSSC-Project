using Microsoft.AspNetCore.Mvc;
using Project.Common.Services;
using Project.Domain.Commands;
using Project.Domain.Models;
using Project.Domain.Workflows;
using Project.ReturnOrder.Models;
using ReturnOrder.Api.Models;
using Swashbuckle.AspNetCore.Filters;
using static Project.Domain.WorkflowEvents.ReturnOrderEvent;

namespace Project.ReturnOrder.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReturnOrderController : ControllerBase
    {
        private readonly ILogger<ReturnOrderController> _logger;
        private readonly IEventService _eventService;


        public ReturnOrderController(ILogger<ReturnOrderController> logger, IEventService eventService)
        {
            _logger = logger;
            _eventService = eventService;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("ReceiverReturnEvent")]
        public async Task ReceiveEvent(ReturnOrderData order)
        {
            if (order != null)
            {
                _eventService.SetOrderToRemove(order);
            }
        }

        [HttpPost]
        [SwaggerRequestExample(typeof(ReturnOrderInput), typeof(InputReturnOrderExample))]
        public async Task<IActionResult> ReturnOrder([FromServices] ReturnOrderWorkflow returnOrderWorkflow, [FromBody] ReturnOrderInput returnOrderInput)
        {
            var returnOrder = MapReturnOrderInputToReturnOrder(returnOrderInput);
            ReturnOrderCommand command = new(returnOrder);
            var result = await returnOrderWorkflow.ExecuteAsync(command);

            return result.Match<IActionResult>(
                returnOrderSucceededEvent => Ok(),
                returnOrderFailedEvent => StatusCode(StatusCodes.Status500InternalServerError, returnOrderFailedEvent.Reason)
                );
        }

        private static ReturnOrderModel MapReturnOrderInputToReturnOrder(ReturnOrderInput returnOrderInput) => new ReturnOrderModel(
            UserRegistrationNumber: returnOrderInput.UserRegistrationNumber,
            OrderNumber: returnOrderInput.OrderNumber
            );
    }
}