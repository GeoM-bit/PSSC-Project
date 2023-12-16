using Microsoft.AspNetCore.Mvc;
using Project.Domain.Models;
using Project.Domain.Repositories;

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
    }
}
