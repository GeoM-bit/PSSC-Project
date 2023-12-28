using Project.Domain.Repositories;
using Microsoft.Extensions.Logging;
using static Project.Domain.WorkflowEvents.PlaceOrderEvent;
using Project.Domain.Commands;
using Project.Domain.Models;
using LanguageExt;
using static LanguageExt.Prelude;
using static Project.Domain.Operations.PlaceOrderOperation;
using System;
using static Project.Domain.Models.Orders;
using Fare;

namespace Project.Domain.Workflows
{
    public class PlaceOrderWorkflow
    {
        private readonly IOrderRepository orderRepository;
        private readonly IUserRepository userRepository;
        private readonly IProductRepository productRepository;
        private readonly ILogger<PlaceOrderWorkflow> logger;

        public PlaceOrderWorkflow(IOrderRepository orderRepository, IUserRepository userRepository, IProductRepository productRepository, ILogger<PlaceOrderWorkflow> logger)
        {
            this.orderRepository = orderRepository;
            this.userRepository = userRepository;
            this.productRepository = productRepository;
            this.logger = logger;
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
                         let checkUserBalance = (Func<UnvalidatedPlacedOrder, IEnumerable<EvaluatedProduct>, Option<UnvalidatedPlacedOrder>>)((unvalidatedOrder,products) => CheckUserBalance(users, unvalidatedOrder, products))
                         from placedOrder in ExecuteWorkflowAsync(unvalidatedOrder, orderNumbers, users, checkUserExists, checkOrderExists, checkProductsExist, checkUserBalance).ToAsync()
                         from saveResult in orderRepository.TrySaveOrder(placedOrder)
                                     .ToEither(ex => new FailedOrder(unvalidatedOrder.Order, ex) as IOrder)
                         let successfulEvent = new PlaceOrderSucceededEvent(placedOrder.Order, DateTime.Now)
                         
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
                                                                             IEnumerable<UserDto> users,     
                                                                             Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists,
                                                                             Func<OrderNumber, Option<OrderNumber>> checkOrderExists,
                                                                             Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist,
                                                                             Func<UnvalidatedPlacedOrder, IEnumerable<EvaluatedProduct>, Option<UnvalidatedPlacedOrder>> checkUserBalance)
        {
            unvalidatedPlacedOrder = GenerateOrderNumber(unvalidatedPlacedOrder, orderNumbers);
            IOrder order = await ValidatePlacedOrder(checkUserExists, checkOrderExists, checkProductsExist, checkUserBalance, unvalidatedPlacedOrder, users);
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

        private Option<UnvalidatedPlacedOrder> CheckUserBalance(List<UserDto> users, UnvalidatedPlacedOrder unvalidatedPlacedOrder, IEnumerable<EvaluatedProduct> products)
        {
            var price = products.Sum(p => p.Price.Price * p.Quantity.Quantity);
            var user = users.FirstOrDefault(u => u.UserRegistrationNumber == unvalidatedPlacedOrder.Order.userRegistrationNumber);
            if(user!=null && user.Balance >=  price)
            {
                return Some(unvalidatedPlacedOrder);
            }
            return None;
        }

        public static UnvalidatedPlacedOrder GenerateOrderNumber(UnvalidatedPlacedOrder unvalidatedOrder, IEnumerable<OrderNumber> orderNumbers)
        {
            Xeger xeger = new Xeger("^PSSC[0-9]{3}$");
            var orderNumber = xeger.Generate();

            while(orderNumbers.Any(n => n.Value == orderNumber))
            {
                orderNumber = xeger.Generate();
            }

            return new UnvalidatedPlacedOrder(
                new UnvalidatedOrder
                (
                    userRegistrationNumber: unvalidatedOrder.Order.userRegistrationNumber,
                    OrderNumber: orderNumber,
                    OrderPrice: 0,
                    OrderDeliveryAddress: unvalidatedOrder.Order.OrderDeliveryAddress,
                    OrderProducts: unvalidatedOrder.Order.OrderProducts
                )               
                );
        }
    }
}

