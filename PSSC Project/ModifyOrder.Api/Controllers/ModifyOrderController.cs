using Microsoft.AspNetCore.Mvc;
using ModifyOrder.Api.Models;
using Project.Domain.Workflows;
using Project.Dto.Models;
using Project.Common.Services;
using Swashbuckle.AspNetCore.Filters;

namespace ModifyOrder.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ModifyOrderController : ControllerBase
    {      
        private readonly ILogger<ModifyOrderController> _logger;
        private readonly IEventService _eventService;

        public ModifyOrderController(ILogger<ModifyOrderController> logger, IEventService eventService)
        {
            _logger = logger;
            _eventService = eventService;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("ReceiveEvent")]
        public async Task ReceiveEvent(ReceivedOrder order)
        {
            if (order != null)
            {
                _eventService.SetPlacedOrderEventReceived(order);
            }
        }

        [HttpPost]
        [SwaggerRequestExample(typeof(InputModifyOrder), typeof(InputModifyOrderExample))]
        public async Task<string> ModifyOrder([FromServices] ModifyOrderWorkflow modifyOrderWorkflow, [FromBody] InputModifyOrder inputModifyOrder)
        {
            if(_eventService.IsOrderPlaced(inputModifyOrder.ModifyOrderNumber))
            {
                return "Order can be modified.";
            }

            return "Order can't be modified.";
        }
    }
}