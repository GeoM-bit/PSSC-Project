using LanguageExt;
using Microsoft.Extensions.Logging;
using Project.Domain.Commands;
using Project.Domain.Models;
using Project.Domain.Repositories;
using Project.Dto.Events;
using Project.Dto.Models;
using static LanguageExt.Prelude;
using static Project.Domain.Models.Orders;
using static Project.Domain.Models.ReturnOrders;
using static Project.Domain.WorkflowEvents.ReturnOrderEvent;
using static Project.Domain.Operations.ReturnOrderOperation;
using static Project.Domain.WorkflowEvents.PlaceOrderEvent;

namespace Project.Domain.Workflows
{
    public class ReturnOrderWorkflow
    {
        private readonly IOrderRepository orderRepository;
        private readonly IUserRepository userRepository;
        private readonly IProductRepository productRepository;
        private readonly ILogger<ReturnOrderWorkflow> logger;

        public ReturnOrderWorkflow(IOrderRepository orderRepository, IUserRepository userRepository, IProductRepository productRepository, ILogger<ReturnOrderWorkflow> logger)
        {
            this.orderRepository = orderRepository;
            this.userRepository = userRepository;
            this.productRepository = productRepository;
            this.logger = logger;
        }

        public async Task<IReturnOrderEvent> ExecuteAsync(ReturnOrderCommand command)
        {
            UnvalidatedReturnOrders unvalidatedOrder = new UnvalidatedReturnOrders(command.ReturnOrder);

            var result = from userRegistrationNumbers in userRepository.TryGetExistingUserRegistrationNumbers()
                                    .ToEither(ex => new FailedReturnOrders(unvalidatedOrder.Order, ex) as IReturnOrder)
                         from orderNumbers in orderRepository.TryGetExistentOrderNumbers()
                                    .ToEither(ex => new FailedReturnOrders(unvalidatedOrder.Order, ex) as IReturnOrder)
                         let checkUserExists = (Func<UserRegistrationNumber, Option<UserRegistrationNumber>>)(user => CheckUserExists(userRegistrationNumbers, user))
                         let checkOrderExists = (Func<OrderNumber, Option<OrderNumber>>)(order => CheckOrderExists(orderNumbers, order))
                         from removedOrder in ExecuteWorkflowAsync(unvalidatedOrder, userRegistrationNumbers, orderNumbers, checkUserExists, checkOrderExists).ToAsync()
                         from removeResult in orderRepository.TryRemoveOrder(removedOrder)
                                     .ToEither(ex => new FailedReturnOrders(unvalidatedOrder.Order, ex) as IReturnOrder)

                         let successfulEvent = new ReturnOrderSucceededEvent(removedOrder.Order, DateTime.Now)
                       
                         select successfulEvent;

            return await result.Match(
                Left: order => GenerateFailedEvent(order) as IReturnOrderEvent,
                Right: order => order);
        }

        private ReturnOrderFailedEvent GenerateFailedEvent(IReturnOrder order) =>
          order.Match<ReturnOrderFailedEvent>(
          unvalidatedReturnOrder => new($"Invalid state {nameof(UnvalidatedReturnOrders)}"),
          invalidReturnOrder => new(invalidReturnOrder.Reason),
          failedReturnOrder =>
          {
              logger.LogError(failedReturnOrder.Exception, failedReturnOrder.Exception.Message);
              return new(failedReturnOrder.Exception.Message);
          },
          validatedReturnOrder => new($"Invalid state {nameof(ValidatedReturnOrders)}")
          );

        private Option<UserRegistrationNumber> CheckUserExists(IEnumerable<UserRegistrationNumber> users, UserRegistrationNumber userRegistrationNumber)
        {
            if (users.Any(u => u == userRegistrationNumber))
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

        private async Task<Either<IReturnOrder, ValidatedReturnOrders>> ExecuteWorkflowAsync(UnvalidatedReturnOrders unvalidatedReturnOrder, 
                                                                            IEnumerable<UserRegistrationNumber> userRegistrationNumbers,
                                                                            IEnumerable<OrderNumber> orderNumbers,
                                                                            Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists,
                                                                            Func<OrderNumber, Option<OrderNumber>> checkOrderExists)
                                                                            
        {
            IReturnOrder order = await ValidateReturnOrder(unvalidatedReturnOrder, userRegistrationNumbers, orderNumbers, checkUserExists, checkOrderExists);

            return order.Match<Either<IReturnOrder, ValidatedReturnOrders>>(
                unvalidatedReturnOrder => Left(unvalidatedReturnOrder as IReturnOrder),
                invalidReturnOrder => Left(invalidReturnOrder as IReturnOrder),
                failedReturnOrder => Left(failedReturnOrder as IReturnOrder),
                validatedReturnOrder => Right(validatedReturnOrder)
            );
        }
    }
}