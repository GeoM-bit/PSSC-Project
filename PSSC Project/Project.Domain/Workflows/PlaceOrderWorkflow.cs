using Project.Domain.Repositories;
using Microsoft.Extensions.Logging;
using static Project.Domain.WorkflowEvents.PlaceOrderEvent;
using Project.Domain.Commands;
using Project.Domain.Models;
using LanguageExt;
using static LanguageExt.Prelude;
using static Project.Domain.Operations.PlaceOrderOperation;
using static Project.Domain.Models.Orders;
using Fare;
using Project.Events;
using Project.Domain.WorkflowEvents;
using Project.Dto.Events;
using Project.Dto.Models;
using System.Text.RegularExpressions;
using LanguageExt.Pipes;

namespace Project.Domain.Workflows
{
    public class PlaceOrderWorkflow
    {
        private readonly IOrderRepository orderRepository;
        private readonly IUserRepository userRepository;
        private readonly IProductRepository productRepository;
        private readonly ILogger<PlaceOrderWorkflow> logger;
        private readonly IEventSender eventSender;

        public PlaceOrderWorkflow(IOrderRepository orderRepository, IUserRepository userRepository, IProductRepository productRepository, ILogger<PlaceOrderWorkflow> logger, IEventSender eventSender)
        {
            this.orderRepository = orderRepository;
            this.userRepository = userRepository;
            this.productRepository = productRepository;
            this.logger = logger;
            this.eventSender = eventSender;
        }
      
        public async Task<IPlaceOrderEvent> ExecuteAsync(PlaceOrderCommand command)
        {
            UnvalidatedPlacedOrder unvalidatedOrder = new UnvalidatedPlacedOrder(command.InputOrder);

            var result = from userRegistrationNumbers in userRepository.TryGetExistingUserRegistrationNumbers()
                                    .ToEither(ex => new FailedOrder(unvalidatedOrder.Order, ex) as IOrder)
                         from orderNumbers in orderRepository.TryGetExistentOrderNumbers()
                                    .ToEither(ex => new FailedOrder(unvalidatedOrder.Order, ex) as IOrder)
                         from existentProducts in productRepository.TryGetExistentProducts()
                                    .ToEither(ex => new FailedOrder(unvalidatedOrder.Order, ex) as IOrder)
                         from users in userRepository.TryGetExistingUsers()
                                    .ToEither(ex => new FailedOrder(unvalidatedOrder.Order, ex) as IOrder)
                         let checkUserExists = (Func<UserRegistrationNumber, Option<UserRegistrationNumber>>)(user => CheckUserExists(userRegistrationNumbers, user))
                         let checkOrderExists = (Func<OrderNumber, Option<OrderNumber>>)(order => CheckOrderExists(orderNumbers, order))
                         let checkProductsExist = (Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>>)(products => CheckProductsExist(existentProducts, products))
                         let checkUserPaymentDetails = (Func<UnvalidatedPlacedOrder, Option<CardDetailsDto>>)(user => CheckUserPaymentDetails(unvalidatedOrder, users))
                         let checkUserBalance = (Func<UnvalidatedPlacedOrder, IEnumerable<EvaluatedProduct>, CardDetailsDto, Option <UnvalidatedPlacedOrder>>)((unvalidatedOrder, products, cardDetails) => CheckUserBalance(users, unvalidatedOrder, products, cardDetails))
                         from placedOrder in ExecuteWorkflowAsync(unvalidatedOrder, orderNumbers, checkUserExists, checkOrderExists, checkProductsExist, checkUserPaymentDetails, checkUserBalance).ToAsync()
                         from saveResult in orderRepository.TrySaveOrder(placedOrder)
                                     .ToEither(ex => new FailedOrder(unvalidatedOrder.Order, ex) as IOrder)

                         let successfulEvent = new PlaceOrderSucceededEvent(placedOrder.Order, DateTime.Now)
                         let eventToPublish = new PlacedOrderEvent()
                         {
                             Order = new OrderDto()
                             {
                                 UserRgistrationNumber = placedOrder.Order.UserRegistrationNumber.Value,
                                 OrderNumber = placedOrder.Order.OrderNumber.Value,
                                 DeliveryAddress = placedOrder.Order.OrderDeliveryAddress.DeliveryAddress,
                                 Telephone = placedOrder.Order.OrderTelephone.Value,
                                 CardNumber = placedOrder.Order.CardDetails.UserCardNumber.CardNumber,
                                 CVV = placedOrder.Order.CardDetails.UserCardCVV.Value,
                                 CardExpiryDate = placedOrder.Order.CardDetails.UserCardExpiryDate.Value,
                                 OrderProducts = placedOrder.Order.OrderProducts.OrderProductsList.Select(
                                     p => new ProductDto()
                                     {
                                         ProductName = p.ProductName.Name,
                                         Quantity = p.Quantity.Quantity
                                     }
                                     ).ToList()
                             }
                         }
                         from publicEventResult in eventSender.SendAsync("order", eventToPublish)
                                            .ToEither(ex => new FailedOrder(unvalidatedOrder.Order, ex) as IOrder)
                         select successfulEvent;
                                    
            return await result.Match(
                Left: order => GenerateFailedEvent(order) as IPlaceOrderEvent,
                Right: order => order);
        }

        private PlaceOrderFailedEvent GenerateFailedEvent(IOrder order) =>
            order.Match<PlaceOrderFailedEvent>(
                unvalidatedPlacedOrder => new($"Invalid state {nameof(UnvalidatedPlacedOrder)}"),
                invalidOrder => new(invalidOrder.Reason),
                failedOrder =>
                {
                    logger.LogError(failedOrder.Exception, failedOrder.Exception.Message);
                    return new(failedOrder.Exception.Message);
                },
                validatedOrder => new($"Invalid state {nameof(ValidatedOrder)}"),
                placedOrder => new($"Invalid state {nameof(PlacedOrder)}")
                );

        private async Task<Either<IOrder, ValidatedOrder>> ExecuteWorkflowAsync(UnvalidatedPlacedOrder unvalidatedPlacedOrder,
                                                                             IEnumerable<OrderNumber> orderNumbers,
                                                                             Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists,
                                                                             Func<OrderNumber, Option<OrderNumber>> checkOrderExists,
                                                                             Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist,
                                                                             Func<UnvalidatedPlacedOrder, Option<CardDetailsDto>> checkUserPaymentDetails,
                                                                             Func<UnvalidatedPlacedOrder, IEnumerable<EvaluatedProduct>, CardDetailsDto, Option<UnvalidatedPlacedOrder>> checkUserBalance)
        {
            unvalidatedPlacedOrder = GenerateOrderNumber(unvalidatedPlacedOrder, orderNumbers);
            IOrder order = await ValidatePlacedOrder(checkUserExists, checkOrderExists, checkProductsExist, checkUserPaymentDetails, checkUserBalance, unvalidatedPlacedOrder);

            order = CalculatePrice(order);

            return order.Match<Either<IOrder, ValidatedOrder>>(
                unvalidatedPlacedOrder => Left(unvalidatedPlacedOrder as IOrder),
                invalidOrder => Left(invalidOrder as IOrder),
                failedOrder => Left(failedOrder as IOrder),
                validatedOrder => Right(validatedOrder),
                placedOrder => Left(placedOrder as IOrder)
            );
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

        private Option<List<EvaluatedProduct>> CheckProductsExist(IEnumerable<EvaluatedProduct> existentProducts, IEnumerable<UnvalidatedProduct> products)
        {
            if (products.All(product => existentProducts.Any(existingProduct => existingProduct.ProductName.Name == product.ProductName)) &&
                products.All(product => existentProducts.Any(existingProduct => existingProduct.Quantity.Quantity >= product.Quantity)))
            {
                List<EvaluatedProduct> result = new();
                result = (from unvalidated in products
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
            else
            {
                return None;
            }
        }

        private Option<OrderNumber> CheckOrderExists(IEnumerable<OrderNumber> orders, OrderNumber orderNumber)
        {
            if (orders.Any(o => o == orderNumber))
            {
                return None;
            }
            else
            {
                return Some(orderNumber);
            }
        }

        private Option<UnvalidatedPlacedOrder> CheckUserBalance(List<UserDto> users, UnvalidatedPlacedOrder unvalidatedPlacedOrder, IEnumerable<EvaluatedProduct> products, CardDetailsDto cardDetails)
        {
            var price = products.Sum(p => p.Price.Price * p.Quantity.Quantity);

            if (!cardDetails.ToUpdate)
            {
                var user = users.FirstOrDefault(u => u.UserRegistrationNumber == unvalidatedPlacedOrder.Order.UserRegistrationNumber);
                if (user != null && user.Balance >= price)
                {
                    return Some(unvalidatedPlacedOrder);
                }
            }
            else
            {
                if (cardDetails.Balance >= price)
                {
                    return Some(unvalidatedPlacedOrder);
                }
            }
            return None;
        }

        public static UnvalidatedPlacedOrder GenerateOrderNumber(UnvalidatedPlacedOrder unvalidatedOrder, IEnumerable<OrderNumber> orderNumbers)
        {
            Xeger xeger = new Xeger("^PSSC[0-9]{3}$");
            var orderNumber = xeger.Generate();

            while (orderNumbers.Any(n => n.Value == orderNumber))
            {
                orderNumber = xeger.Generate();
            }

            return new UnvalidatedPlacedOrder(
                new UnvalidatedOrder
                (
                    UserRegistrationNumber: unvalidatedOrder.Order.UserRegistrationNumber,
                    OrderNumber: orderNumber,
                    OrderPrice: 0,
                    OrderDeliveryAddress: unvalidatedOrder.Order.OrderDeliveryAddress,
                    OrderTelephone: unvalidatedOrder.Order.OrderTelephone,
                    CardNumber: unvalidatedOrder.Order.CardNumber,
                    CVV: unvalidatedOrder.Order.CVV,
                    CardExpiryDate: unvalidatedOrder.Order.CardExpiryDate,
                    OrderProducts: unvalidatedOrder.Order.OrderProducts
                )
                );
        }
        private Option<CardDetailsDto> CheckUserPaymentDetails(UnvalidatedPlacedOrder unvalidatedPlacedOrder, IEnumerable<UserDto> users)
        {
            if (unvalidatedPlacedOrder.Order.CardNumber == null && unvalidatedPlacedOrder.Order.CVV == null && unvalidatedPlacedOrder.Order.CardExpiryDate == null)
            {
                var user = users.FirstOrDefault(u => u.UserRegistrationNumber == unvalidatedPlacedOrder.Order.UserRegistrationNumber);
                if (user != null)
                {
                    if (user.CardNumber != null && user.CVV != null && user.CardExpiryDate != null && user.Balance != null)
                    {
                        if ((new Regex("[0-9]{16}")).IsMatch(user.CardNumber) && user.CVV.ToString().Length == 3 && user.CardExpiryDate > DateTime.Now)
                        {
                            return Some(new CardDetailsDto() 
                            {
                                ToUpdate = false});
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
    }
}

