using Microsoft.AspNetCore.Mvc;

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
     
    }
}