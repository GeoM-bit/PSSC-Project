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
                                                 Func<UnvalidatedPlacedOrder, Option<CardDetailsDto>> checkUserPaymentDetails,
                                                 Func<CardDetailsDto, Option<CardDetailsDto>> updateCardDetails,
                                                 Func<UnvalidatedPlacedOrder, IEnumerable<EvaluatedProduct>, CardDetailsDto, Option<UnvalidatedPlacedOrder>> checkUserBalance,
                                                 UnvalidatedPlacedOrder order) =>
           ValidateOrder(checkUserExists, checkOrderExists, checkProductsExist, checkUserPaymentDetails, updateCardDetails, checkUserBalance, order)
            .MatchAsync(
               Right: validatedOrder => new ValidatedOrder(validatedOrder),
               LeftAsync: errorMessage => Task.FromResult((IOrder)new InvalidOrder(order.Order, errorMessage))
               );
               
                    
        private static Func<UnvalidatedPlacedOrder, EitherAsync<string, EvaluatedOrder>> ValidateOrder(Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists, Func<OrderNumber, Option<OrderNumber>> checkOrderExists, Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist, Func<UnvalidatedPlacedOrder, Option<CardDetailsDto>> checkUserPaymentDetails, Func<CardDetailsDto, Option<CardDetailsDto>> updateCardDetails, Func<UnvalidatedPlacedOrder, IEnumerable<EvaluatedProduct>, CardDetailsDto, Option<UnvalidatedPlacedOrder>> checkUserBalance) =>
            unvalidatedOrder => ValidateOrder(checkUserExists, checkOrderExists, checkProductsExist, checkUserPaymentDetails, updateCardDetails, checkUserBalance, unvalidatedOrder);

        private static EitherAsync<string, EvaluatedOrder> ValidateOrder(Func<UserRegistrationNumber, Option<UserRegistrationNumber>> checkUserExists,
                                                 Func<OrderNumber, Option<OrderNumber>> checkOrderExists,
                                                 Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist,
                                                 Func<UnvalidatedPlacedOrder, Option<CardDetailsDto>> checkUserPaymentDetails,
                                                 Func<CardDetailsDto, Option<CardDetailsDto>> updateCardDetails,
                                                 Func<UnvalidatedPlacedOrder, IEnumerable<EvaluatedProduct>, CardDetailsDto, Option<UnvalidatedPlacedOrder>> checkUserBalance,
                                                 UnvalidatedPlacedOrder unvalidatedOrder) =>
            from userRegistrationNumber in UserRegistrationNumber.TryParse(unvalidatedOrder.Order.UserRegistrationNumber)
                                    .ToEitherAsync($"Invalid user registration number ({unvalidatedOrder.Order.UserRegistrationNumber}).")
            from orderNumber in OrderNumber.TryParse(unvalidatedOrder.Order.OrderNumber)
                                    .ToEitherAsync($"Invalid order number ({unvalidatedOrder.Order.OrderNumber}).")
            from userExists in checkUserExists(userRegistrationNumber)
                                    .ToEitherAsync($"User ({unvalidatedOrder.Order.UserRegistrationNumber} does not exists).")
            from orderExists in checkOrderExists(orderNumber)
                                    .ToEitherAsync($"Order with order number ({unvalidatedOrder.Order.OrderNumber}) already exists.")
            from productsExist in checkProductsExist(unvalidatedOrder.Order.OrderProducts)
                                    .ToEitherAsync($"Invalid product list for order ({unvalidatedOrder.Order}).")
            from validTelephone in OrderTelephone.TryParse(unvalidatedOrder.Order.OrderTelephone)
                                    .ToEitherAsync($"Invalid telephone number ({unvalidatedOrder.Order.OrderTelephone})")
            from checkedUserPaymentDetails in checkUserPaymentDetails(unvalidatedOrder)
                                    .ToEitherAsync("Invalid or missing payment details.")
           // let res = updateCardDetails(checkedUserPaymentDetails)
            from checkedBalance in checkUserBalance(unvalidatedOrder, productsExist, checkedUserPaymentDetails)
                                    .ToEitherAsync($"Insufficient funds for paying order ({unvalidatedOrder.Order}).")
            select new EvaluatedOrder(orderNumber, new OrderPrice(0), new OrderDeliveryAddress(unvalidatedOrder.Order.OrderDeliveryAddress), new OrderTelephone(unvalidatedOrder.Order.OrderTelephone), new OrderProducts(productsExist)) 
            { 
                UserRegistrationNumber = userRegistrationNumber,
                CardDetails = new CardDetails(
                    new UserCardNumber(checkedUserPaymentDetails.CardNumber),
                    new UserCardCVV(checkedUserPaymentDetails.CVV),
                    new UserCardExpiryDate(checkedUserPaymentDetails.CardExpiryDate),
                    new UserCardBalance(checkedUserPaymentDetails.Balance),
                    checkedUserPaymentDetails.ToUpdate
                    )
            };

        

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
                            validatedOrder.Order.OrderTelephone,
                            validatedOrder.Order.OrderProducts
                            )
                       {
                           UserRegistrationNumber = validatedOrder.Order.UserRegistrationNumber,
                           CardDetails = validatedOrder.Order.CardDetails
                       }
                       );
               },
               placedOrder => placedOrder
           );
    }
}
