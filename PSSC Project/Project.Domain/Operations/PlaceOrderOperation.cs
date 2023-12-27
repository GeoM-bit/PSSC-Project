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
                                                 UnvalidatedPlacedOrder order) =>
           ValidateOrder(checkUserExists, checkOrderExists, checkProductsExist, order)
            .MatchAsync(
               Right: validatedOrder => new ValidatedOrder(validatedOrder),
               LeftAsync: errorMessage => Task.FromResult((IOrder)new InvalidOrder(order.Order, errorMessage))
               );
               
                    
        private static Func<UnvalidatedPlacedOrder, EitherAsync<string, EvaluatedOrder>> ValidateOrder(Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists, Func<OrderNumber, Option<OrderNumber>> checkOrderExists, Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist) =>
            unvalidatedOrder => ValidateOrder(checkUserExists, checkOrderExists, checkProductsExist, unvalidatedOrder);

        private static EitherAsync<string, EvaluatedOrder> ValidateOrder(Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists,
                                                 Func<OrderNumber, Option<OrderNumber>> checkOrderExists,
                                                 Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist,
                                                 UnvalidatedPlacedOrder unvalidatedOrder) =>
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
            select new EvaluatedOrder(orderNumber, default, default, default);          
    }
}
