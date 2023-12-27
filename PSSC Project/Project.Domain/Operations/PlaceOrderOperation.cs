using LanguageExt;
using Project.Domain.Models;
using static Project.Domain.Models.Orders;

namespace Project.Domain.Operations
{
    public static class PlaceOrderOperation
    {
        public static Task<IOrder> ValidatePlacedOrder(Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists,
                                                 Func<OrderNumber, Option<OrderNumber>> checkOrderExists,
                                                 Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist,
                                                 Func<UnvalidatedPlacedOrder, IEnumerable<EvaluatedProduct>, Option<UnvalidatedPlacedOrder>> checkUserBalance,
                                                 UnvalidatedPlacedOrder order,
                                                 IEnumerable<UserDto> users) =>
           ValidateOrder(checkUserExists, checkOrderExists, checkProductsExist, checkUserBalance, order, users)
            .MatchAsync(
               Right: validatedOrder => new ValidatedOrder(validatedOrder),
               LeftAsync: errorMessage => Task.FromResult((IOrder)new InvalidOrder(order.Order, errorMessage))
               );
               
                    
        private static Func<UnvalidatedPlacedOrder, EitherAsync<string, EvaluatedOrder>> ValidateOrder(Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists, Func<OrderNumber, Option<OrderNumber>> checkOrderExists, Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist, Func<UnvalidatedPlacedOrder, IEnumerable<EvaluatedProduct>, Option<UnvalidatedPlacedOrder>> checkUserBalance, IEnumerable<UserDto> users) =>
            unvalidatedOrder => ValidateOrder(checkUserExists, checkOrderExists, checkProductsExist, checkUserBalance, unvalidatedOrder, users);

        private static EitherAsync<string, EvaluatedOrder> ValidateOrder(Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists,
                                                 Func<OrderNumber, Option<OrderNumber>> checkOrderExists,
                                                 Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist,
                                                 Func<UnvalidatedPlacedOrder, IEnumerable<EvaluatedProduct>, Option<UnvalidatedPlacedOrder>> checkUserBalance,
                                                 UnvalidatedPlacedOrder unvalidatedOrder,
                                                 IEnumerable<UserDto> users) =>
            from userRegistrationNumber in UserRegistrationNumber.TryParse(unvalidatedOrder.Order.userRegistrationNumber)
                                    .ToEitherAsync($"Invalid user registration number ({unvalidatedOrder.Order.userRegistrationNumber}).")
            from orderNumber in OrderNumber.TryParse(unvalidatedOrder.Order.OrderNumber)
                                    .ToEitherAsync($"Invalid order number ({unvalidatedOrder.Order.OrderNumber}).")
            from userExists in checkUserExists(userRegistrationNumber)
                                    .ToEitherAsync($"User ({unvalidatedOrder.Order.userRegistrationNumber} does not exists).")
            from orderExists in checkOrderExists(orderNumber)
                                    .ToEitherAsync($"Order with order number ({unvalidatedOrder.Order.OrderNumber}) already exists.")
            from productsExist in checkProductsExist(unvalidatedOrder.Order.OrderProducts)
                                    .ToEitherAsync($"Invalid product list for order ({unvalidatedOrder.Order}).")
            from checkedBalance in checkUserBalance(unvalidatedOrder, productsExist)
                                    .ToEitherAsync($"Insufficient funds for paying order ({unvalidatedOrder.Order}).")
            select new EvaluatedOrder(orderNumber, new OrderPrice(0), new OrderDeliveryAddress(unvalidatedOrder.Order.OrderDeliveryAddress), new OrderProducts(productsExist)) { UserRegistrationNumber = userRegistrationNumber};

        public static IOrder CalculatePrice(IOrder order) => order.Match(
           unvalidatedPlacedOrder => unvalidatedPlacedOrder,
               invalidOrder => invalidOrder,
               failedOrder => failedOrder,
               validatedOrder =>
               {
                   return new ValidatedOrder(
                       new EvaluatedOrder(
                            validatedOrder.Order.OrderNumber,
                            new OrderPrice(validatedOrder.Order.OrderProducts.OrderProductsList.Sum(p => p.Price.Price * p.Quantity.Quantity)),
                            validatedOrder.Order.OrderDeliveryAddress,
                            validatedOrder.Order.OrderProducts
                            )
                       {
                           UserRegistrationNumber = validatedOrder.Order.UserRegistrationNumber
                       }
                       );
               },
               placedOrder => placedOrder
           );

    }

    
}
