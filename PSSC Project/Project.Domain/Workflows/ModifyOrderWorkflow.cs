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
using LanguageExt.ClassInstances;

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
                         let checkProductsExist = (Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>>)(modifiedProducts => CheckProductsExist(existentProducts, order.OrderProducts.OrderProductsList, modifiedProducts))
                         let checkUserPaymentDetails = (Func<UnvalidatedModifiedOrder, Option<CardDetailsDto>>)(c => CheckUserPaymentDetails(unvalidatedModifiedOrder, user))
                         let checkUserBalance = (Func<UnvalidatedModifiedOrder, List<EvaluatedProduct>, CardDetailsDto, Option<UnvalidatedModifiedOrder>>)((unvalidatedModifiedOrder, products, cardDetails) => CheckUserBalance(unvalidatedModifiedOrder, products, cardDetails, order, user))
                         from modifiedOrder in ExecuteWorkflowAsync(unvalidatedModifiedOrder, order, checkProductsExist, checkUserPaymentDetails, checkUserBalance).ToAsync()
                         from saveResult in orderRepository.TryUpdateOrder(modifiedOrder, order)
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
                         //from publicEventResult in eventSender.SendAsync("modifyOrder", eventToPublish)
                         //                   .ToEither(ex => new FailedOrder(unvalidatedModifiedOrder.Order, ex) as IModifyOrder)
                         select successfulEvent;

            return await result.Match(
                Left: order => GenerateFailedEvent(order) as IModifyOrderEvent,
                Right: order => order);
        }

        private async Task<Either<IModifyOrder, ValidatedModifiedOrder>> ExecuteWorkflowAsync(UnvalidatedModifiedOrder unvalidatedModifiedOrder,
                                                                            EvaluatedOrder order,
                                                                            Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist,
                                                                            Func<UnvalidatedModifiedOrder, Option<CardDetailsDto>> checkUserPaymentDetails,
                                                                            Func<UnvalidatedModifiedOrder, List<EvaluatedProduct>, CardDetailsDto, Option<UnvalidatedModifiedOrder>> checkUserBalance)
        {
            IModifyOrder modifiedOrder = await ValidateModifyOrder(unvalidatedModifiedOrder, order, checkProductsExist, checkUserPaymentDetails, checkUserBalance);

            modifiedOrder = CalculatePrice(modifiedOrder);

            return modifiedOrder.Match<Either<IModifyOrder, ValidatedModifiedOrder>>(
                unvalidatedModifiedOrder => Left(unvalidatedModifiedOrder as IModifyOrder),
                invalidModifiedOrder => Left(invalidModifiedOrder as IModifyOrder),
                failedModifiedOrder => Left(failedModifiedOrder as IModifyOrder),
                validatedModifiedOrder => Right(validatedModifiedOrder)
            );
        }

        private Option<List<EvaluatedProduct>> CheckProductsExist(IEnumerable<EvaluatedProduct> existentProducts, IEnumerable<EvaluatedProduct> orderProducts, IEnumerable<UnvalidatedProduct> modifiedProducts)
        {
            foreach (var existentProduct in existentProducts)
            {
                foreach (var orderProduct in orderProducts)
                {
                    foreach (var modifiedProduct in modifiedProducts)
                    {
                        if (existentProduct.ProductName.Name == orderProduct.ProductName.Name && orderProduct.ProductName.Name == modifiedProduct.ProductName)
                        {
                            var initialQuantity = existentProduct.Quantity.Quantity + orderProduct.Quantity.Quantity;
                            if (initialQuantity < modifiedProduct.Quantity)
                                return None;
                        }
                    }
                }
            }

            List<EvaluatedProduct> result = new();
            result = (from unvalidated in modifiedProducts
                      join evaluated in existentProducts on unvalidated.ProductName equals evaluated.ProductName.Name
                      select new
                      {
                          ProductName = unvalidated.ProductName,
                          Quantity = unvalidated.Quantity,
                          Price = evaluated.Price.Price
                      })
                         .ToList()
                         .Select(res => new EvaluatedProduct(
                             new ProductName(res.ProductName),
                             new ProductQuantity(res.Quantity),
                             new ProductPrice(res.Price)))
                         .ToList();

            return Option<List<EvaluatedProduct>>.Some(result);
        }
        private Option<CardDetailsDto> CheckUserPaymentDetails(UnvalidatedModifiedOrder unvalidatedModifiedOrder, UserDto user)
        {
            if (unvalidatedModifiedOrder.Order.CardNumber == null && unvalidatedModifiedOrder.Order.CVV == null && unvalidatedModifiedOrder.Order.CardExpiryDate == null)
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
            else if (unvalidatedModifiedOrder.Order.CardNumber != null && unvalidatedModifiedOrder.Order.CVV != null && unvalidatedModifiedOrder.Order.CardExpiryDate != null)
            {
                if ((new Regex("[0-9]{16}")).IsMatch(unvalidatedModifiedOrder.Order.CardNumber) && unvalidatedModifiedOrder.Order.CVV.ToString().Length == 3 && unvalidatedModifiedOrder.Order.CardExpiryDate > DateTime.Now)
                {

                    return Some(new CardDetailsDto()
                    {
                        UserRegistrationNumber = unvalidatedModifiedOrder.Order.UserRegistrationNumber,
                        CardNumber = unvalidatedModifiedOrder.Order.CardNumber,
                        CVV = unvalidatedModifiedOrder.Order.CVV,
                        CardExpiryDate = unvalidatedModifiedOrder.Order.CardExpiryDate,
                        Balance = new Random().NextDouble() * (7000 - 1000) + 1000,
                        ToUpdate = true
                    });
                }
            }
            return None;
        }

        private Option<UnvalidatedModifiedOrder> CheckUserBalance(UnvalidatedModifiedOrder unvalidatedModifiedOrder, IEnumerable<EvaluatedProduct> products, CardDetailsDto cardDetails, EvaluatedOrder order, UserDto user)
        {
            var price = products.Sum(p => p.Price.Price * p.Quantity.Quantity);

            if (!cardDetails.ToUpdate)
            {
                if (user.Balance + order.OrderPrice.Price >= price)
                {
                    return Some(unvalidatedModifiedOrder);
                }
            }
            else
            {
                if (cardDetails.Balance + order.OrderPrice.Price >= price)
                {
                    return Some(unvalidatedModifiedOrder);
                }
            }
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
