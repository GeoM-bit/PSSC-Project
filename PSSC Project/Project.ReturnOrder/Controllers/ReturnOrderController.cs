using Microsoft.AspNetCore.Mvc;
using Project.Domain.Commands;
using Project.Domain.Models;
using Project.Domain.Workflows;
using Project.ReturnOrder.Models;
using static Project.Domain.WorkflowEvents.ReturnOrderEvent;

namespace Project.ReturnOrder.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReturnOrderController : ControllerBase
    {
        private readonly ILogger<ReturnOrderController> _logger;

        public ReturnOrderController(ILogger<ReturnOrderController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
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
            OrderNumber: returnOrderInput.InputOrderNumber
            );
    }
}