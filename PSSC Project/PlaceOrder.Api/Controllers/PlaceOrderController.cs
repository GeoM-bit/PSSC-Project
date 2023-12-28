using Microsoft.AspNetCore.Mvc;
using PlaceOrder.Api.Models;
using Project.Domain.Commands;
using Project.Domain.Models;
using Project.Domain.Repositories;
using Project.Domain.Workflows;
using static Project.Domain.WorkflowEvents.PlaceOrderEvent;

namespace PlaceOrder.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlaceOrderController : ControllerBase
    {
        private ILogger<PlaceOrderController> logger;

        public PlaceOrderController(ILogger<PlaceOrderController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders([FromServices] IOrderRepository orderRepository) =>
            await orderRepository.TryGetExistentOrders().Match(
                Succ: GetAllOrdersHandleSuccess,
                Fail: GetAllOrdersHandleError              
                );

        private ObjectResult GetAllOrdersHandleError(Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return base.StatusCode(StatusCodes.Status500InternalServerError, "UnexpectedError");
        }

        private OkObjectResult GetAllOrdersHandleSuccess(List<EvaluatedOrder> orders) =>
        Ok(orders.Select(order => new
        {
          order.OrderNumber,
          order.OrderPrice,
          order.OrderDeliveryAddress,
          order.OrderProducts,
        }));

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromServices]PlaceOrderWorkflow placeOrderWorkflow, [FromBody]InputOrder inputOrder)
        {
            var unvalidatedOrder = MapInputOrderToUnvalidatedOrder(inputOrder);
            PlaceOrderCommand command = new(unvalidatedOrder);
            var result = await placeOrderWorkflow.ExecuteAsync(command);

            return result.Match<IActionResult>(
                placeOrderSucceededEvent => Ok(),
                placedOrderFailedEvent => StatusCode(StatusCodes.Status500InternalServerError, placedOrderFailedEvent.Reason)
                );               
        }

        private static UnvalidatedOrder MapInputOrderToUnvalidatedOrder(InputOrder inputOrder) => new UnvalidatedOrder(
            userRegistrationNumber: inputOrder.RegistrationNumber,
            OrderNumber: "",
            OrderPrice: 0,
            OrderDeliveryAddress: inputOrder.DeliveryAddress,
            OrderProducts: MapInputProductsToUnvalidatedProducts(inputOrder.OrderProducts)
            );
        private static List<UnvalidatedProduct> MapInputProductsToUnvalidatedProducts(List<InputProduct> inputProducts)
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
