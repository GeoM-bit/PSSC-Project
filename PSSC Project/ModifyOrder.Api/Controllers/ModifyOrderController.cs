using Microsoft.AspNetCore.Mvc;
using ModifyOrder.Api.Models;
using Project.Domain.Workflows;
using Project.Common.Services;
using Swashbuckle.AspNetCore.Filters;
using Project.Domain.Models;
using Project.Domain.Commands;
using static Project.Domain.Models.Orders;
using static Project.Domain.WorkflowEvents.ModifyOrderEvent;

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
        public async Task<IActionResult> ModifyOrder([FromServices] ModifyOrderWorkflow modifyOrderWorkflow, [FromBody] InputModifyOrder inputModifyOrder)
        {
            IModifyOrderEvent result = null;

            if (_eventService.IsOrderPlaced(inputModifyOrder.ModifyOrderNumber))
            {
                var unvalidatedModifyOrder = MapInputModifyOrderToUnvalidatedOrder(inputModifyOrder);
                ModifyOrderCommand command = new(unvalidatedModifyOrder);
                result = await modifyOrderWorkflow.ExecuteAsync(command);
            }


            return result.Match<IActionResult>(
                placeOrderSucceededEvent => Ok(),
                placedOrderFailedEvent => StatusCode(StatusCodes.Status500InternalServerError, placedOrderFailedEvent.Reason)
                );
        }
        private static UnvalidatedOrder MapInputModifyOrderToUnvalidatedOrder(InputModifyOrder inputOrder) => new UnvalidatedOrder(
            UserRegistrationNumber: inputOrder.ModifyOrderRegistrationNumber,
            OrderNumber: inputOrder.ModifyOrderNumber,
            OrderPrice: 0,
            OrderDeliveryAddress: inputOrder.DeliveryAddress,
            OrderTelephone: inputOrder.Telephone,
            CardNumber: inputOrder.CardNumber,
            CVV: inputOrder.CVV,
            CardExpiryDate: inputOrder.CardExpiryDate,
            OrderProducts: MapInputModidyProductsToUnvalidatedProducts(inputOrder.OrderProducts)
            );

        private static List<UnvalidatedProduct> MapInputModidyProductsToUnvalidatedProducts(List<InputModifyProduct> inputProducts)
        {
            List<UnvalidatedProduct> unvalidatedProducts = new List<UnvalidatedProduct>();
            foreach (var product in inputProducts)
            {
                unvalidatedProducts.Add(new UnvalidatedProduct(
                    ProductName: product.ProductName,
                    Quantity: product.Quantity
                    ));
            }

            return unvalidatedProducts;
        }
    }
}