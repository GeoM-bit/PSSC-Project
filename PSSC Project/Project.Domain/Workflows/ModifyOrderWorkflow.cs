using LanguageExt;
using Microsoft.Extensions.Logging;
using Project.Domain.Commands;
using Project.Domain.Models;
using Project.Domain.Repositories;
using Project.Dto.Events;
using Project.Dto.Models;
using Project.Events;
using static LanguageExt.Prelude;
using System.Text.RegularExpressions;
using static Project.Domain.Models.ModidyOrders;
using static Project.Domain.Models.Orders;
using static Project.Domain.WorkflowEvents.ModifyOrderEvent;
using static Project.Domain.Operations.ModifyOrderOperation;

using LanguageExt.Pipes;

namespace Project.Domain.Workflows
{
    public class ModifyOrderWorkflow
    {
        private readonly IOrderRepository orderRepository;
        private readonly IUserRepository userRepository;
        private readonly IProductRepository productRepository;
        private readonly ILogger<ModifyOrderWorkflow> logger;
        private readonly IEventSender eventSender;

        public ModifyOrderWorkflow(IOrderRepository orderRepository, IUserRepository userRepository, IProductRepository productRepository, ILogger<ModifyOrderWorkflow> logger, IEventSender eventSender)
        {
            this.orderRepository = orderRepository;
            this.userRepository = userRepository;
            this.productRepository = productRepository;
            this.logger = logger;
            this.eventSender = eventSender;
        }

        public async Task<IModifyOrderEvent> ExecuteAsync(ModifyOrderCommand command)
        {
            UnvalidatedModifiedOrder unvalidatedModifiedOrder = new UnvalidatedModifiedOrder(command.InputOrder);

            var result = from user in userRepository.TryGetExistingUser(unvalidatedModifiedOrder.Order.UserRegistrationNumber)
                                   .ToEither(ex => new FailedModifiedOrder(unvalidatedModifiedOrder.Order, ex) as IModifyOrder)
                         from order in orderRepository.TryGetExistentOrder(unvalidatedModifiedOrder.Order.OrderNumber)
                                    .ToEither(ex => new FailedModifiedOrder(unvalidatedModifiedOrder.Order, ex) as IModifyOrder)
                         from existentProducts in productRepository.TryGetExistentProducts()
                                    .ToEither(ex => new FailedModifiedOrder(unvalidatedModifiedOrder.Order, ex) as IModifyOrder)
                         from orderProducts in productRepository.TryGetOrderProducts(unvalidatedModifiedOrder.Order.UserRegistrationNumber)
                                    .ToEither(ex => new FailedModifiedOrder(unvalidatedModifiedOrder.Order, ex) as IModifyOrder)
                         let checkProductsExist = (Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>>)(modifiedProducts => CheckProductsExist(existentProducts, orderProducts, modifiedProducts))
                         let checkUserPaymentDetails = (Func<UnvalidatedModifiedOrder, Option<CardDetailsDto>>)(c => CheckUserPaymentDetails(unvalidatedModifiedOrder, user))
                         let checkUserBalance = (Func<UnvalidatedModifiedOrder, CardDetailsDto, Option<UnvalidatedModifiedOrder>>)((unvalidatedModifiedOrder, cardDetails) => CheckUserBalance(unvalidatedModifiedOrder, cardDetails, order))
                         from modifiedOrder in ExecuteWorkflowAsync(unvalidatedModifiedOrder, order, checkProductsExist, checkUserPaymentDetails, checkUserBalance).ToAsync()
                         from saveResult in orderRepository.TryUpdateOrder(modifiedOrder)
                                     .ToEither(ex => new FailedModifiedOrder(unvalidatedModifiedOrder.Order, ex) as IModifyOrder)

                         let successfulEvent = new ModifyOrderSucceededEvent(modifiedOrder.Order, DateTime.Now)
                         let eventToPublish = new ModifiedOrderEvent()
                         {
                             Order = new OrderDto()
                             {
                                 UserRgistrationNumber = modifiedOrder.Order.UserRegistrationNumber.Value,
                                 OrderNumber = modifiedOrder.Order.OrderNumber.Value,
                                 DeliveryAddress = modifiedOrder.Order.OrderDeliveryAddress.DeliveryAddress,
                                 Telephone = modifiedOrder.Order.OrderTelephone.Value,
                                 CardNumber = modifiedOrder.Order.CardDetails.UserCardNumber.CardNumber,
                                 CVV = modifiedOrder.Order.CardDetails.UserCardCVV.Value,
                                 CardExpiryDate = modifiedOrder.Order.CardDetails.UserCardExpiryDate.Value,
                                 OrderProducts = modifiedOrder.Order.OrderProducts.OrderProductsList.Select(
                                     p => new ProductDto()
                                     {
                                         ProductName = p.ProductName.Name,
                                         Quantity = p.Quantity.Quantity
                                     }
                                 ).ToList()
                             }
                         }
                         from publicEventResult in eventSender.SendAsync("modifyOrder", eventToPublish)
                                            .ToEither(ex => new FailedOrder(unvalidatedModifiedOrder.Order, ex) as IModifyOrder)
                         select successfulEvent;

            return await result.Match(
                Left: order => GenerateFailedEvent(order) as IModifyOrderEvent,
                Right: order => order);
        }

        private async Task<Either<IModifyOrder, ValidatedModifiedOrder>> ExecuteWorkflowAsync(UnvalidatedModifiedOrder unvalidatedModifiedOrder,
                                                                            EvaluatedOrder order,
                                                                            Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist,
                                                                            Func<UnvalidatedModifiedOrder, Option<CardDetailsDto>> checkUserPaymentDetails,
                                                                            Func<UnvalidatedModifiedOrder, CardDetailsDto, Option<UnvalidatedModifiedOrder>> checkUserBalance)
        {
            IModifyOrder modifiedOrder = await ValidateModifyOrder(unvalidatedModifiedOrder, order, checkProductsExist, checkUserPaymentDetails, checkUserBalance);

            // modifiedOrder = CalculatePrice(modifiedOrder);

            return modifiedOrder.Match<Either<IModifyOrder, ValidatedModifiedOrder>>(
                unvalidatedModifiedOrder => Left(unvalidatedModifiedOrder as IModifyOrder),
                invalidModifiedOrder => Left(invalidModifiedOrder as IModifyOrder),
                failedModifiedOrder => Left(failedModifiedOrder as IModifyOrder),
                validatedModifiedOrder => Right(validatedModifiedOrder)
            );
        }


        private Option<List<EvaluatedProduct>> CheckProductsExist(IEnumerable<EvaluatedProduct> existentProducts, IEnumerable<EvaluatedProduct> orderProducts, IEnumerable<UnvalidatedProduct> modifiedProducts)
        {
            return None;
        }
        private Option<CardDetailsDto> CheckUserPaymentDetails(UnvalidatedModifiedOrder unvalidatedPlacedOrder, UserDto user)
        {
            if (unvalidatedPlacedOrder.Order.CardNumber == null && unvalidatedPlacedOrder.Order.CVV == null && unvalidatedPlacedOrder.Order.CardExpiryDate == null)
            {
                if (user != null)
                {
                    if (user.CardNumber != null && user.CVV != null && user.CardExpiryDate != null && user.Balance != null)
                    {
                        if ((new Regex("[0-9]{16}")).IsMatch(user.CardNumber) && user.CVV.ToString().Length == 3 && user.CardExpiryDate > DateTime.Now)
                        {
                            return Some(new CardDetailsDto()
                            {
                                ToUpdate = false
                            });
                        }
                    }
                }
            }
            else if (unvalidatedPlacedOrder.Order.CardNumber != null && unvalidatedPlacedOrder.Order.CVV != null && unvalidatedPlacedOrder.Order.CardExpiryDate != null)
            {
                if ((new Regex("[0-9]{16}")).IsMatch(unvalidatedPlacedOrder.Order.CardNumber) && unvalidatedPlacedOrder.Order.CVV.ToString().Length == 3 && unvalidatedPlacedOrder.Order.CardExpiryDate > DateTime.Now)
                {

                    return Some(new CardDetailsDto()
                    {
                        UserRegistrationNumber = unvalidatedPlacedOrder.Order.UserRegistrationNumber,
                        CardNumber = unvalidatedPlacedOrder.Order.CardNumber,
                        CVV = unvalidatedPlacedOrder.Order.CVV,
                        CardExpiryDate = unvalidatedPlacedOrder.Order.CardExpiryDate,
                        Balance = new Random().NextDouble() * (7000 - 1000) + 1000,
                        ToUpdate = true
                    });
                }
            }
            return None;
        }

        private Option<UnvalidatedModifiedOrder> CheckUserBalance(UnvalidatedModifiedOrder unvalidatedModifiedOrder, CardDetailsDto cardDetails, EvaluatedOrder order)
        {          
            return None;
        }
        private ModifyOrderFailedEvent GenerateFailedEvent(IModifyOrder order) =>
           order.Match<ModifyOrderFailedEvent>(
               unvalidatedModifiedOrder => new($"Invalid state {nameof(UnvalidatedModifiedOrder)}"),
               invalidModifiedOrder => new(invalidModifiedOrder.Reason),
               failedModifiedOrder =>
               {
                   logger.LogError(failedModifiedOrder.Exception, failedModifiedOrder.Exception.Message);
                   return new(failedModifiedOrder.Exception.Message);
               },
               validatedModifiedOrder => new($"Invalid state {nameof(ValidatedModifiedOrder)}")
               );

    }
}
