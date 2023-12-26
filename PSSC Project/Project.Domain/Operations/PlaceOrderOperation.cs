using LanguageExt;
using Project.Domain.Models;
using static Project.Domain.Models.Orders;
using static LanguageExt.Prelude;
using System.Linq;

namespace Project.Domain.Operations
{
    public static class PlaceOrderOperation
    {
        public static Task<IOrder> ValidatePlacedOrder(Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists,
                                                 Func<OrderNumber, Option<OrderNumber>> checkOrderExists,
                                                 UnvalidatedPlacedOrder order) =>
           ValidateOrder(checkUserExists, checkOrderExists, order)
            .MatchAsync(
               Right: validatedOrder => new ValidatedOrder(validatedOrder),
               LeftAsync: errorMessage => Task.FromResult((IOrder)new InvalidOrder(order, errorMessage))
               );
               
                    
        private static Func<UnvalidatedPlacedOrder, EitherAsync<string, EvaluatedOrder>> ValidateOrder(Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists, Func<OrderNumber, Option<OrderNumber>> checkOrderExists) =>
            unvalidatedOrder => ValidateOrder(checkUserExists, checkOrderExists, unvalidatedOrder);

        private static EitherAsync<string, EvaluatedOrder> ValidateOrder(Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists,
                                                 Func<OrderNumber, Option<OrderNumber>> checkOrderExists,
                                                 UnvalidatedPlacedOrder unvalidatedOrder) =>
            from userRegistrationNumber in UserRegistrationNumber.TryParse(unvalidatedOrder.Order.userRegistrationNumber)
                                    .ToEitherAsync($"Invalid user registration number ({unvalidatedOrder.Order.userRegistrationNumber})")
            from userExists in checkUserExists(userRegistrationNumber)
                                    .ToEitherAsync($"User ({unvalidatedOrder.Order.userRegistrationNumber} does not exists)")
            select new EvaluatedOrder();          
    }
}
