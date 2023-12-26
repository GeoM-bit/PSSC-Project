using Project.Domain.Repositories;
using Microsoft.Extensions.Logging;
using static Project.Domain.WorkflowEvents.PlaceOrderEvent;
using Project.Domain.Commands;
using static Project.Domain.Models.Orders;
using Project.Domain.Models;
using LanguageExt;
using static LanguageExt.Prelude;
using static Project.Domain.Operations.PlaceOrderOperation;

namespace Project.Domain.Workflows
{
    public class PlaceOrderWorkflow
    {
        private readonly IOrderRepository orderRepository;
        private readonly IUserRepository userRepository;
        private readonly ILogger<PlaceOrderWorkflow> logger;

        public PlaceOrderWorkflow(IOrderRepository orderRepository, IUserRepository userRepository, ILogger<PlaceOrderWorkflow> logger)
        {
            this.orderRepository = orderRepository;
            this.userRepository = userRepository;
            this.logger = logger;
        }
      
        public async Task<IPlaceOrderEvent> ExecuteAsync(PlaceOrderCommand command)
        {
            UnvalidatedPlacedOrder unvalidatedOrder = new UnvalidatedPlacedOrder(command.InputOrder);

            var result = from users in userRepository.TryGetExistingUserRegistrationNumbers()
                                    .ToEither(ex => new FailedOrder(unvalidatedOrder.Order, ex) as IOrder)
                         from orders in orderRepository.TryGetExistentOrderNumbers()
                                    .ToEither(ex => new FailedOrder(unvalidatedOrder.Order, ex) as IOrder)
                         let checkUserExists = (Func<UserRegistrationNumber, Option<UserRegistrationNumber>>)(user => CheckUserExists(users, user))
                         let checkOrderExists = (Func<OrderNumber, Option<OrderNumber>>)(order => CheckOrderExists(orders, order))
                         from placedOrder in ExecuteWorkflowAsync(unvalidatedOrder, checkUserExists, checkOrderExists).ToAsync()
                            

                         select unvalidatedOrder;
                         ;
            
            return (IPlaceOrderEvent)await result.Match(
                Left: order => order,
                Right: order => order);
        }

        private async Task<Either<IOrder, PlacedOrder>> ExecuteWorkflowAsync(UnvalidatedPlacedOrder unvalidatedPlacedOrder, 
                                                                             Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists,
                                                                             Func<OrderNumber, Option<OrderNumber>> checkOrderExists)
        {
            IOrder order = await ValidatePlacedOrder(checkUserExists, checkOrderExists, unvalidatedPlacedOrder);

            return null;
        }

        private Option<UserRegistrationNumber> CheckUserExists(IEnumerable<UserRegistrationNumber> users, UserRegistrationNumber userRegistrationNumber)
        {
            if(users.Any(u => u == userRegistrationNumber)) 
            {
                return Some(userRegistrationNumber);
            }
            else
            {
                return None;
            }
        }

        private Option<OrderNumber> CheckOrderExists(IEnumerable<OrderNumber> orders, OrderNumber orderNumber)
        {
            if (orders.Any(o => o == orderNumber))
            {
                return Some(orderNumber);
            }
            else
            {
                return None;
            }
        }
    }
}
