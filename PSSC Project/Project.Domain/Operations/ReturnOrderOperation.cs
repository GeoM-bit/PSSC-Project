using LanguageExt;
using Project.Domain.Models;
using static Project.Domain.Models.ReturnOrders;

namespace Project.Domain.Operations
{
    public static class ReturnOrderOperation
    {
        public static Task<IReturnOrder> ValidateReturnOrder(UnvalidatedReturnOrders unvalidatedReturnOrder,
                                                                            IEnumerable<UserRegistrationNumber> userRegistrationNumbers,
                                                                            IEnumerable<OrderNumber> orderNumbers,
                                                                            Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists,
                                                                            Func<OrderNumber, Option<OrderNumber>> checkOrderExists) =>
          ValidateOrder(unvalidatedReturnOrder, userRegistrationNumbers, orderNumbers, checkUserExists, checkOrderExists)
           .MatchAsync(
              Right: validatedReturnOrder => new ValidatedReturnOrders(validatedReturnOrder),
              LeftAsync: errorMessage => Task.FromResult((IReturnOrder)new InvalidReturnOrders(unvalidatedReturnOrder.Order, errorMessage))
              );
        private static EitherAsync<string, EvaluatedReturnOrder> ValidateOrder(UnvalidatedReturnOrders unvalidatedReturnOrder,
                                                                            IEnumerable<UserRegistrationNumber> userRegistrationNumbers,
                                                                            IEnumerable<OrderNumber> orderNumbers,
                                                                            Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists,
                                                                            Func<OrderNumber, Option<OrderNumber>> checkOrderExists) =>
            from userRegistrationNumber in UserRegistrationNumber.TryParse(unvalidatedReturnOrder.Order.UserRegistrationNumber)
                                    .ToEitherAsync($"Invalid user registration number ({unvalidatedReturnOrder.Order.UserRegistrationNumber}).")
            from orderNumber in OrderNumber.TryParse(unvalidatedReturnOrder.Order.OrderNumber)
                                    .ToEitherAsync($"Invalid order number ({unvalidatedReturnOrder.Order.OrderNumber}).")
            from userExists in checkUserExists(userRegistrationNumber)
                                    .ToEitherAsync($"User ({unvalidatedReturnOrder.Order.UserRegistrationNumber} does not exists).")
            from orderExists in checkOrderExists(orderNumber)
                                    .ToEitherAsync($"Order with order number ({unvalidatedReturnOrder.Order.OrderNumber}) already exists.")
            select new EvaluatedReturnOrder(userRegistrationNumber , orderNumber);
    }
}

