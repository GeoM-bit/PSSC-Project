using Project.Domain.Repositories;
using Microsoft.Extensions.Logging;
using static Project.Domain.WorkflowEvents.PlaceOrderEvent;
using Project.Domain.Commands;
using static Project.Domain.Models.Orders;
using Project.Domain.Models;
using LanguageExt;
using static LanguageExt.Prelude;
using static Project.Domain.Operations.PlaceOrderOperation;
using System;

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

            var result = from users in userRepository.TryGetExistingUserRegistrationNumbers()
                                    .ToEither(ex => new FailedOrder(unvalidatedOrder.Order, ex) as IOrder)
                         from orders in orderRepository.TryGetExistentOrderNumbers()
                                    .ToEither(ex => new FailedOrder(unvalidatedOrder.Order, ex) as IOrder)
                         from existentProducts in productRepository.TryGetExistentProducts()
                                    .ToEither(ex => new FailedOrder(unvalidatedOrder.Order, ex) as IOrder)
                         let checkUserExists = (Func<UserRegistrationNumber, Option<UserRegistrationNumber>>)(user => CheckUserExists(users, user))
                         let checkOrderExists = (Func<OrderNumber, Option<OrderNumber>>)(order => CheckOrderExists(orders, order))
                         let checkProductsExist = (Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>>)(products => CheckProductsExist(existentProducts, products))
                         from placedOrder in ExecuteWorkflowAsync(unvalidatedOrder, checkUserExists, checkOrderExists, checkProductsExist).ToAsync()
                            

                         select unvalidatedOrder;
                         ;
            
            return (IPlaceOrderEvent)await result.Match(
                Left: order => order,
                Right: order => order);
        }

        private async Task<Either<IOrder, ValidatedOrder>> ExecuteWorkflowAsync(UnvalidatedPlacedOrder unvalidatedPlacedOrder, 
                                                                             Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists,
                                                                             Func<OrderNumber, Option<OrderNumber>> checkOrderExists,
                                                                             Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist)
        {
            IOrder order = await ValidatePlacedOrder(checkUserExists, checkOrderExists, checkProductsExist, unvalidatedPlacedOrder);

            return order.Match<Either<IOrder, ValidatedOrder>>(
                whenUnvalidatedPlacedOrder: unvalidatedPlacedOrder => Left(unvalidatedPlacedOrder as IOrder),
                whenPlacedOrder: placedOrder => Left(placedOrder as IOrder),
                whenValidatedOrder: validOrder => Right(validOrder)
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
    }
}
