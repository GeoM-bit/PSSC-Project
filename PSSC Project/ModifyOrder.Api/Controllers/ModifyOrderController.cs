using Microsoft.AspNetCore.Mvc;
using Project.Dto.Models;
using Project.Services;

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
        public async Task ReceiveEvent(OrderDto order)
        {
            if (order != null)
            {
                _eventService.SetPlacedOrderEventReceived(order);
            }
        }

        [HttpPost]
        public async Task<string> ModifyOrder(string orderNumber)
        {
            if(_eventService.IsOrderPlaced(orderNumber))
            {
                return "Order can be modified.";
            }

            return "Order can't be modified.";
        }
    }
}